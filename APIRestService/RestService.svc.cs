 
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Net.Mail;
using System.Net;
using System.Web.Mail; 
using System.ServiceModel.Web;
using DxReportViewer;
using Newtonsoft.Json;

namespace APIRestService
{
    public class RESTService : IRestService
    {
        public static string MasterKey_USERINFO = "USERINFO";
        public static string MasterKey_MENU = "MENU";
        public static string MasterKey_STOCK = "STOCK";
        public static string MasterKey_Table = "TABLE";

        public void DoWork()
        {

        }
          
        public Result_MasterDataEnquiry EnquireMasterData(Param_MasterDataEnquiry param)
        {
            Result_MasterDataEnquiry result = new Result_MasterDataEnquiry();
            result.Status = false;

            if (string.IsNullOrEmpty(param.MasterType))
            {
                result.ErrorMessage = "MasterType empty!";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, param.MasterType)
                & DBExpression.Normal("MasterID", DBComparisonOperator.Equal, param.MasterID)
                & DBExpression.LIKE("MasterID", param.MasterIDLike)
                & DBExpression.LIKE("MasterNameEnglish", param.MasterNameEnglish)
                & DBExpression.LIKE("MasterNameThai", param.MasterNameThai);

            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "MASTERDATA";
            xGetDataOptionParam.Condition = dbCondition.ToString();

            string strErrorMessage = string.Empty;
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.Status = false;
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ListOfData = new List<MasterDataDetail>();
            MasterDataDetail masterDataDetail;
            foreach (DataRow row in xDataTable.Rows)
            {
                masterDataDetail = new MasterDataDetail();
                masterDataDetail.MasterType = (string)row["MasterType"];
                masterDataDetail.MasterID = (string)row["MasterID"];
                masterDataDetail.ImageData = DxData.getValueByteArrayImageToBase64(row["ImageData"]);
                if (DBNull.Value != row["MasterNameEnglish"]) masterDataDetail.MasterNameEnglish = (string)row["MasterNameEnglish"];
                if (DBNull.Value != row["MasterNameThai"]) masterDataDetail.MasterNameThai = (string)row["MasterNameThai"];
                if (DBNull.Value != row["MasterInfo"])
                {
                    masterDataDetail.MasterData = (string)row["MasterInfo"];
                    masterDataDetail.MasterData = masterDataDetail.MasterData.Replace("^", "\"");
                }

                if (!string.IsNullOrEmpty(param.MasterType) && !string.IsNullOrEmpty(param.MasterID))
                {
                    dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, param.MasterType)
                    & DBExpression.Normal("MasterID", DBComparisonOperator.EqualEver, param.MasterID);

                    xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
                    xGetDataOptionParam.TableName = "MASTERDATA_IMAGE";
                    xGetDataOptionParam.Condition = dbCondition.ToString();

                    strErrorMessage = string.Empty;
                    DataTable xDataTableImage = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
                    if (xDataTableImage.Rows.Count == 1)
                    {
                        masterDataDetail.ItemImageData = DxData.getValueByteArrayImageToBase64(xDataTableImage.Rows[0]["ImageData"]);
                    }
                }

                result.ListOfData.Add(masterDataDetail);
            }

            result.Status = true;
            return result;

        } 
        public Result_Update_MasterDataDetail UpdateMasterData(Update_MasterDataDetail param)
        {
            Result_Update_MasterDataDetail result = new Result_Update_MasterDataDetail();
            result.Status = false;

            if (string.IsNullOrEmpty(param.MasterType))
            {
                result.ErrorMessage = "Master type empty.";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, param.MasterType)
                & DBExpression.Normal("MasterID", DBComparisonOperator.EqualEver, param.MasterID);

            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "MASTERDATA";
            xGetDataOptionParam.Condition = dbCondition.ToString();

            string strErrorMessage = string.Empty;
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            if (xDataTable.Rows.Count == 0)
            {
                DataRow xNewRow = xDataTable.NewRow();
                xNewRow["MasterType"] = param.MasterType;
                xNewRow["MasterID"] = param.MasterID;
                xDataTable.Rows.Add(xNewRow);
            }

            foreach (DataRow row in xDataTable.Select())
            {
                row["MasterNameEnglish"] = param.MasterNameEnglish;
                row["MasterNameThai"] = param.MasterNameThai;
                row["MasterInfo"] = param.MasterData;
                if (!string.IsNullOrEmpty(param.ImageData))
                {
                    row["ImageData"] = DxImage.ConvertBase64ImageToByteArray(param.ImageData, true);
                }
            }

            condb.UpdateDataTable(xDataTable, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            if (!string.IsNullOrEmpty(param.ItemImageData))
            {
                dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, param.MasterType)
                & DBExpression.Normal("MasterID", DBComparisonOperator.EqualEver, param.MasterID);

                xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
                xGetDataOptionParam.TableName = "MASTERDATA_IMAGE";
                xGetDataOptionParam.Condition = dbCondition.ToString();

                strErrorMessage = string.Empty;
                xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
                if (xDataTable.Rows.Count == 0)
                {
                    DataRow xNewRow = xDataTable.NewRow();
                    xNewRow["MasterType"] = param.MasterType;
                    xNewRow["MasterID"] = param.MasterID;
                    xNewRow["UpdateDateTime"] = DxDate.DateTimeNow_HHMMSS();
                    xDataTable.Rows.Add(xNewRow);
                }

                foreach (DataRow row in xDataTable.Rows)
                {
                    row["UpdateDateTime"] = DxDate.DateTimeNow_HHMMSS();
                    row["ThumbnailData"] = DxImage.CreateImgThumbnail(param.ItemImageData);
                    row["ImageData"] = DxImage.ConvertBase64ImageToByteArray(param.ItemImageData, true);
                }
                condb.UpdateDataTable(xDataTable, out strErrorMessage);
            }


            result.Status = string.IsNullOrEmpty(strErrorMessage);
            return result;

        }
        public Result_Update_MasterDataDetail DeleteMasterData(Update_MasterDataDetail param)
        {
            Result_Update_MasterDataDetail result = new Result_Update_MasterDataDetail();
            result.Status = false;

            if (string.IsNullOrEmpty(param.MasterType))
            {
                result.ErrorMessage = "Master type empty.";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, param.MasterType)
                & DBExpression.Normal("MasterID", DBComparisonOperator.EqualEver, param.MasterID);

            string strErrorMessage = string.Empty;

            condb.DeleteData("MASTERDATA", dbCondition.ToString(), out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.Status = string.IsNullOrEmpty(strErrorMessage);
            return result;

        }

        public AuthenUser_Result AuthenUser(AuthenUser_Param param)
        {
            AuthenUser_Result result = new AuthenUser_Result();

            if (string.IsNullOrEmpty(param.UserID))
            {
                result.ErrorMessage = "UserID is empty!";
                return result;
            }

            Param_MasterDataEnquiry xMasterDataParam = new Param_MasterDataEnquiry();
            xMasterDataParam.ConnectKey = param.ContextKey;
            xMasterDataParam.MasterType = MasterKey_USERINFO;
            xMasterDataParam.MasterID = param.UserID;

            var rest = EnquireMasterData(xMasterDataParam);
            if (rest.ListOfData.Count == 0)
            {
                result.ErrorMessage = "UserID not found!";
                return result;
            }

            if (rest.ListOfData.Count == 1)
            {
                var mUserInfo = JsonConvert.DeserializeObject<MasterData_UserInfo>(rest.ListOfData[0].MasterData);
                if (mUserInfo.UserPassword == param.UserPassword)
                {
                    mUserInfo.UserPassword = "";                    
                    result.UserInfo = mUserInfo;
                    result.UserInfo.UserID = param.UserID;
                    result.UserInfo.UserName = rest.ListOfData[0].MasterNameThai;
                    result.ResultStatus = true;
                }
            }


            return result;
        }

        public EnquireMenu_Result EnquireMenu(EnquireMenu_Param param)
        {
            EnquireMenu_Result result = new EnquireMenu_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.MenuType) && string.IsNullOrEmpty(param.MenuName))
            {
                result.ErrorMessage = "MenuType , MenuName empty!";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, MasterKey_MENU)
                & DBExpression.Normal("MasterID", DBComparisonOperator.Equal, param.MasterID)
                & DBExpression.LIKE("MasterNameEnglish", param.MenuName)
                & DBExpression.LIKE("MasterNameThai", param.MenuName);

            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "MASTERDATA";
            xGetDataOptionParam.Condition = dbCondition.ToString();

            string strErrorMessage = string.Empty;
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ResultStatus = false;
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ListOfMasterData = new List<MasterData_MenuInfo>();
            foreach (DataRow row in xDataTable.Rows)
            {
                var menuData = JsonConvert.DeserializeObject<MasterData_MenuInfo>(DxData.getValueString(row["MasterInfo"]));
                if (!string.IsNullOrEmpty(param.MenuType))
                {
                    if (param.MenuType == menuData.MenuType)
                    {
                        menuData.MenuCode = DxData.getValueString(row["MasterID"]);
                        menuData.MenuNameShow = DxData.getValueString(row["MasterNameEnglish"]);
                        menuData.MenuNameShowThai = DxData.getValueString(row["MasterNameThai"]);
                        result.ListOfMasterData.Add(menuData);
                    }
                }                
            }

            result.ResultStatus = true;
            return result;

        }


        public EnquireTable_Result EnquireTable(EnquireTable_Param param)
        {
            EnquireTable_Result result = new EnquireTable_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.ViewByUserID))
            {
                result.ErrorMessage = "ViewByUserID empty!";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, MasterKey_Table);
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "MASTERDATA";
            xGetDataOptionParam.Condition = dbCondition.ToString();

            string strErrorMessage = string.Empty;
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ResultStatus = false;
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ListOfMasterData = new List<MasterData_TableInfo>();
            foreach (DataRow row in xDataTable.Rows)
            {
                var menuData = JsonConvert.DeserializeObject<MasterData_TableInfo>(DxData.getValueString(row["MasterInfo"]));
                if (null != menuData)
                {
                    menuData.TableID = DxData.getValueString(row["MasterID"]);
                    ///
                    EnquireOrderSummary_Param paramSummary = new EnquireOrderSummary_Param();
                    paramSummary.TableID = menuData.TableID;
                    EnquireOrderSummary_Result xOrderSummryRes = EnquireOrderSummary(paramSummary);
                    menuData.CurrentChargeAmt = xOrderSummryRes.TotalChargeAmt;
                    result.ListOfMasterData.Add(menuData);
                }
            }

            result.ResultStatus = true;
            return result;
        }



        public double getPrice(string strMenuID)
        {
            double dPrice = 0.0;

            Param_MasterDataEnquiry param = new Param_MasterDataEnquiry();
            param.MasterType = MasterKey_MENU;
            param.MasterID = strMenuID;
            
            var resMenu = EnquireMasterData(param);

            if (resMenu.ListOfData.Count > 0)
            {
                var xMasterData_MenuInfo = DxConvert.ConvertStringJsonToClass<MasterData_MenuInfo>(resMenu.ListOfData[0].MasterData);
                dPrice = DxConvert.ConvertStringToDouble(xMasterData_MenuInfo.Price);
            }

            return dPrice;
        }


        #region [ORDER]

        
        public bool ImportTable()
        {
            for (int i = 1; i <= 11; i++)
            {
                Update_MasterDataDetail param = new Update_MasterDataDetail();
                param.MasterType = MasterKey_Table;
                param.MasterID = i.ToString();
                param.MasterNameThai = "โต๊ะ " + i.ToString();
                param.MasterNameEnglish = "โต๊ะ " + i.ToString();
                MasterData_TableInfo xMenuInfo = new MasterData_TableInfo();
                xMenuInfo.TableID = i.ToString();
                xMenuInfo.TableStatus = "0"; //0Empty , 1Active
                param.MasterData = DxConvert.ConvertClassToStringJson(xMenuInfo);
                UpdateMasterData(param);
            }

            return true;
        }


        public bool ImportUser()
        {
            //ListofUser.Add("ID|NAME|TYPE|PASS"); 
            List<string> ListofUser = new List<string>();
            ListofUser.Add("hop|Hop Admin|99|hpd2d");
            ListofUser.Add("admin|Hope Admin|99|!@#");
            ListofUser.Add("staff1|Staff-1|1|s001");
            ListofUser.Add("staff2|Staff-2|1|s002");
            ListofUser.Add("chef1|Chef-1|2|c001");
            ListofUser.Add("chef2|Chef-2|2|c002");

            foreach (var item in ListofUser)
            {
                string[] strMenuinfo = item.Split('|'); 
                Update_MasterDataDetail param = new Update_MasterDataDetail();
                param.MasterType = MasterKey_USERINFO;
                param.MasterID = strMenuinfo[0];
                param.MasterNameThai = strMenuinfo[1];
                param.MasterNameEnglish = strMenuinfo[1];
                MasterData_UserInfo xMenuInfo = new MasterData_UserInfo();
                xMenuInfo.UserIDType = strMenuinfo[2];
                xMenuInfo.UserPassword = strMenuinfo[3]; 
                param.MasterData = DxConvert.ConvertClassToStringJson(xMenuInfo);
                UpdateMasterData(param);
            }

            return true;
        }

        public bool ImportMenu()
        {
            List<string> ListofMenuRaw = new List<string>();

            /*
            ListofMenuRaw.Add("< PRO > Beer สิงห์ (3ขวด)|AR01|2|เครื่องดื่ม|257|***โปร Beer สิงห์ 3 ขวด ***");
            ListofMenuRaw.Add("< PRO > Beer ลีโอ (3ขวด)|AR02|2|เครื่องดื่ม|229|***โปร Beer ลีโอ 3 ขวด ***");
            ListofMenuRaw.Add("< PRO > Beer ช้าง (3ขวด)|AR03|2|เครื่องดื่ม|219|***โปร Beer ช้าง 3 ขวด ***");

            ListofMenuRaw.Add("เหล้า รีเจนซี(แบน)|WK01|2|เหล้า|450|");
            ListofMenuRaw.Add("เหล้า หงส์ทอง (กลม)|WK02|2|เหล้า|350|");

            ListofMenuRaw.Add("MAHANAKHON WHITE ALE(CAN)|CF01|2|คราฟ|150|Belgian style witbier Thai craft beer brewed in Taiwan 5% ABV - 0 IBU **SOFT**");
            ListofMenuRaw.Add("Phitlok|CF02|2|คราฟ|220|IPA - Session 5% ABV - 0 IBU **SOFT**");
            ListofMenuRaw.Add("Yodbeer Bearhug pale ale |CF03|2|คราฟ|210| 5.2% ABV - 0 IBU **SOFT**");
            ListofMenuRaw.Add("Arom Weizen-IPA|CF04|2|คราฟ|210| 5.5% ABV - 30 IBU **SOFT**");

            ListofMenuRaw.Add("Imagine DDH Hazy IPA|CM01|2|คราฟ|220|Wizard Beer IPA - New England/Hazy 5.5% ABV - 40 IBU **MEDIUM**");
            ListofMenuRaw.Add("Wizard passion west coast IPA|CM02|2|คราฟ|220|Wizard Beer IPA - American 5.5% ABV - 40 IBU **MEDIUM**");
            ListofMenuRaw.Add("Sawasdee IPA|CM03|2|คราฟ|220|PHEEBOK IPA 5.8% ABV - 0 IBU **MEDIUM**");
            ListofMenuRaw.Add("ANAN (อนันต์)|CM04|2|คราฟ|240|DDH HAZY IPA IPA 6% ABV - 0 IBU **MEDIUM**");

            ListofMenuRaw.Add("CALL ME PAPA HAZY|CX01|2|คราฟ|230|HAZY MILKSHAKE IPA 6.5% ABV - 0 IBU **HARD**");
            ListofMenuRaw.Add("MAHANAKHON HAZY IPA|CX02|2|คราฟ|160|IPA NewEngland/Hazy 6.5% ABV - 0 IBU **HARD**");
            ListofMenuRaw.Add("CALL ME PAPA DOUBLE HAZY|CX03|2|คราฟ|240|DOUBLE HAZY IPA 7.2% ABV - 70 IBU **HARD**");
            ListofMenuRaw.Add("Away Gee IPA|CX04|2|คราฟ|260|Pheebok Beer IPA American 7.5% ABV - 0 IBU **HARD**");


            */
            /*
            ListofMenuRaw.Add("ข้าวผัดมันเนื้อ|RC01|1|แนะนำ|200");
            ListofMenuRaw.Add("ก้อยเสือคั่ว|RC02|1|แนะนำ|150");
            
            ListofMenuRaw.Add("ผัดมาม่าหมูสับ|RC03|1|แนะนำ|150");
            ListofMenuRaw.Add("ผัดมาม่าขี้เมา|RC04|1|แนะนำ|150");
            ListofMenuRaw.Add("เห่าดง|RC05|1|แนะนำ|120");
            ListofMenuRaw.Add("สลัดผัก|RC06|1|แนะนำ|120");

            ListofMenuRaw.Add("ต้มขมเนื้อ|SP01|1|ต้ม|100");
            ListofMenuRaw.Add("ต้มแซ่บเนื้อ|SP02|1|ต้ม|100");
            ListofMenuRaw.Add("ต้มแซ่บกระดูกอ่อน|SP03|1|ต้ม|100");
            ListofMenuRaw.Add("ซุปเปอร์ตีนไก่|SP04|1|ต้ม|80");

            ListofMenuRaw.Add("เนื้อย่าง|GL01|1|ย่าง|100");
            ListofMenuRaw.Add("สันคอหมูย่าง|GL02|1|ย่าง|100");

            ListofMenuRaw.Add("ส้มตำปูปลาร้า|YM01|1|ส้มตำ|50");
            ListofMenuRaw.Add("ตำแตงไข่ต้ม|YM02|1|ส้มตำ|60");
            ListofMenuRaw.Add("ตำถั่วหมูกรอบ|YM03|1|ส้มตำ|70");
            //ListofMenuRaw.Add("ตำกุ้งสด|YM04|1|ส้มตำ|120");

            ListofMenuRaw.Add("เอ็นแก้วตุ๋น|ST01|1|ล้วกจิ้ม|120");
            ListofMenuRaw.Add("น่องลาย|ST02|1|ล้วกจิ้ม|120");
            ListofMenuRaw.Add("สามชั้นนึ่ง|ST03|1|ล้วกจิ้ม|120");

            ListofMenuRaw.Add("กุ้งแช่น้ำปลา|YM05|1|ยำ|150");
            ListofMenuRaw.Add("ยำคอหมูย่าง|YM06|1|ยำ|100");
            ListofMenuRaw.Add("ยำแหนม|YM07|1|ยำ|120");
            ListofMenuRaw.Add("หมูมะนาว|YM08|1|ยำ|100");
            ListofMenuRaw.Add("ปูอัดวาซาบิ|YM09|1|ยำ|70");
            ListofMenuRaw.Add("ยำวุ้นเส้นหมูสับ|YM10|1|ยำ|120");
            ListofMenuRaw.Add("ยำวุ้นเส้นกุ้ง|YM11|1|ยำ|150");
            ListofMenuRaw.Add("ยำเม็ดมะม่วง|YM12|1|ยำ|120");
            ListofMenuRaw.Add("ยำถั่วทอด|YM13|1|ยำ|80");
            //ListofMenuRaw.Add("ยำไข่ดาว|YM14|1|ยำ|120");
            ListofMenuRaw.Add("ยำไข่ต้มหมูสับ|YM15|1|ยำ|90");
            ListofMenuRaw.Add("ยำเห็ดนางรมหลวง|YM16|1|ยำ|80");

            ListofMenuRaw.Add("หมูสามชั้นทอดกรอบ|FR01|1|ทอด|100");
            ListofMenuRaw.Add("สันคอหมูคั่วเค็ม|FR02|1|ทอด|120");
            ListofMenuRaw.Add("สันคอหมููแดดเดียว|FR03|1|ทอด|100");
            ListofMenuRaw.Add("เนื้อแดดเดียว|FR04|1|ทอด|120");
            ListofMenuRaw.Add("เอ็นไก่ทอด|FR05|1|ทอด|80");
            ListofMenuRaw.Add("ปีกไก่ทอดเกลือ|FR06|1|ทอด|90");

            ListofMenuRaw.Add("เฟรนซ์ฟราย|OD01|1|อาหารว่าง|60");
            ListofMenuRaw.Add("ข้าวเกรียบทอด|OD02|1|อาหารว่าง|70");
            ListofMenuRaw.Add("เส้นนี้มีปู|OD03|1|อาหารว่าง|99");
            ListofMenuRaw.Add("คางกุ้งทอดกรอบ|OD04|1|อาหารว่าง|99");
            ListofMenuRaw.Add("ลูกชิ้นทอดรวม|OD05|1|อาหารว่าง|120");

            ListofMenuRaw.Add("คะน้าหมูกรอบ|SF01|1|ผัด|120");
            ListofMenuRaw.Add("หมูผัดน้ำมันหอย|SF02|1|ผัด|100");
            ListofMenuRaw.Add("ไก่ผัดน้ำมันหอย|SF03|1|ผัด|100");
            ListofMenuRaw.Add("กระเพราหมู|SF04|1|ผัด|100");
            ListofMenuRaw.Add("กระเพราไก่|SF05|1|ผัด|100");
            ListofMenuRaw.Add("กระเพราเนื้อตุ๋น|SF06|1|ผัด|120");
            ListofMenuRaw.Add("กระเพราเนื้อแองกัส|SF07|1|ผัด|120");
            ListofMenuRaw.Add("กะหล่ำปลีผัดน้ำปลา|SF08|1|ผัด|80");

            ListofMenuRaw.Add("ข้าวไข่เจียวหมูสับ|FF01|1|จานเดียว|60"); 
            ListofMenuRaw.Add("ข้าวผัดหมู|FF03|1|จานเดียว|60");
            ListofMenuRaw.Add("ข้าวผัดไก่|FF04|1|จานเดียว|60");
            ListofMenuRaw.Add("ข้าวผัดกระเพราหมูสับ|FF05|1|จานเดียว|60");
            ListofMenuRaw.Add("ข้าวผัดกระเพราเนื้อแองกัส|FF06|1|จานเดียว|80");
            ListofMenuRaw.Add("ข้าวผัดกระเพราเนื้อตุ๋น|FF07|1|จานเดียว|80");
            ListofMenuRaw.Add("ข้าวคะน้าหมูกรอบ|FF08|1|จานเดียว|80");
            ListofMenuRaw.Add("ข้าวเนื้อน้ำมันหอย|FF09|1|จานเดียว|80");
            ListofMenuRaw.Add("ข้าวหมูสามชั้นทอดน้ำจิ้มแจ่ว|FF10|1|จานเดียว|80");
            ListofMenuRaw.Add("เพิ่มไข่ดาว|FF11|1|จานเดียว|10");
            ListofMenuRaw.Add("เพิ่มไข่เจียว|FF12|1|จานเดียว|10");
            ListofMenuRaw.Add("เพิ่มไข่ต้ม|FF13|1|จานเดียว|10");
            */

            foreach (var item in ListofMenuRaw)
            {
                string[] strMenuinfo = item.Split('|');

                Update_MasterDataDetail param = new Update_MasterDataDetail();
                param.MasterType = "MENU";
                param.MasterID = strMenuinfo[1];
                param.MasterNameThai = strMenuinfo[0];
                param.MasterNameEnglish = strMenuinfo[0];
                MasterData_MenuInfo xMenuInfo = new MasterData_MenuInfo();                
                xMenuInfo.MenuType = strMenuinfo[2]; //1อาหาร 2เครื่องดื่ม
                xMenuInfo.MenuCategory = strMenuinfo[3];
                xMenuInfo.MenuImage = param.MasterID.ToLower() + ".jpg";
                xMenuInfo.Price = strMenuinfo[4];

                if (strMenuinfo.Length == 6)
                {
                    xMenuInfo.MenuDescrption = strMenuinfo[5];
                }

                param.MasterData = DxConvert.ConvertClassToStringJson(xMenuInfo);
                UpdateMasterData(param);
            }

            //Update_MasterDataDetail param = new Update_MasterDataDetail();
            //param.MasterType = "MENU";
            //param.MasterID = "SP01";
            //param.MasterNameThai = "ต้มขมเนื้อ";
            //param.MasterNameEnglish = "ต้มขมเนื้อ";
            //MasterData_MenuInfo xMenuInfo = new MasterData_MenuInfo();
            //xMenuInfo.MenuType = "1"; //1อาหาร 2เครื่องดื่ม
            //xMenuInfo.MenuCategory = "ต้ม";
            //xMenuInfo.MenuImage = param.MasterID.ToLower() + ".jpg";            
            //xMenuInfo.Price = "";
            //param.MasterData = DxConvert.ConvertClassToStringJson(xMenuInfo);
            //UpdateMasterData(param);



            return true;

        }

        public UpdateOrder_Result UpdateNewOrder(Data_ORDER_HEAD_Param param)
        {
            UpdateOrder_Result result = new UpdateOrder_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.TableID))
            {
                result.ErrorMessage = "TableID empty.";
                return result;
            }

            if (null == param.ListOfItem || param.ListOfItem.Count == 0)
            {
                result.ErrorMessage = "Order Item empty.";
                return result;
            }

            param.OrderNo = param.TableID + DateTime.Now.Ticks.ToString();

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);

            string strErrorMessage = string.Empty;
            DataTable xDataTable = condb.GetSchemaTable(TableName.ORDER_HEAD);
            DataTable xDataTableItem = condb.GetSchemaTable(TableName.ORDER_DETAIL);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            DataRow xNewRow = xDataTable.NewRow();
            xNewRow[TableName.ORDER_HEAD_Field_OrderNo] = param.OrderNo;
            xNewRow[TableName.ORDER_HEAD_Field_TableID] = param.TableID;
            xNewRow[TableName.ORDER_HEAD_Field_OrderByUserID] = param.OrderByUserID;
            xNewRow[TableName.ORDER_HEAD_Field_OrderDateTime] = DateTime.Now;
            xDataTable.Rows.Add(xNewRow);

            int iSuffix = 1;
            foreach (var itemData in param.ListOfItem)
            {
                if (string.IsNullOrEmpty(itemData.MenuID))
                    continue;

                xNewRow = xDataTableItem.NewRow();
                xNewRow[TableName.ORDER_DETAIL_Field_OrderNo] = param.OrderNo;
                xNewRow[TableName.ORDER_DETAIL_Field_Suffix] = iSuffix++;
                xNewRow[TableName.ORDER_DETAIL_Field_MenuID] = itemData.MenuID;
                xNewRow[TableName.ORDER_DETAIL_Field_MenuName] = itemData.MenuName;
                xNewRow[TableName.ORDER_DETAIL_Field_MenuMemo] = itemData.MenuMemo;
                xNewRow[TableName.ORDER_DETAIL_Field_EntryDateTime] = DateTime.Now;
                xNewRow[TableName.ORDER_DETAIL_Field_Qty] = DxConvert.ConvertStringToDouble(itemData.Qty); 
                xNewRow[TableName.ORDER_DETAIL_Field_ChargeAmt] = DxConvert.ConvertStringToDouble(itemData.Qty) * getPrice(itemData.MenuID); 
                xDataTableItem.Rows.Add(xNewRow);
            }

            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "Order Item empty.";
                return result;
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(xDataTable);
            ds.Tables.Add(xDataTableItem);
            condb.Transaction_UpdateDataSet(ds, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;
        }

        public UpdateOrder_Result UpdateCookAckOrder(Data_ORDER_HEAD_Param param)
        {
            UpdateOrder_Result result = new UpdateOrder_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.OrderNo))
            {
                result.ErrorMessage = "ระบุเลข Order";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);

            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }
            else
            {
                if (DateTime.MinValue != DxData.getValueDateTime(xDataTable.Rows[0][TableName.ORDER_HEAD_Field_CxlDateTime]))
                {
                    result.ErrorMessage = "รายการนี้ถูกยกเลิกแล้ว";
                    return result;
                }
            }

            dbCondition = DBExpression.Normal(TableName.ORDER_DETAIL_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);
            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการอาหาร";
                return result;
            }


            foreach (DataRow RowItem in xDataTable.Rows)
            {
                RowItem[TableName.ORDER_HEAD_Field_CookAckDateTime] = DateTime.Now;
                RowItem[TableName.ORDER_HEAD_Field_CookAckUserID] = param.CookAckUserID;
            }

            foreach (DataRow RowItem in xDataTableItem.Rows)
            {
                RowItem[TableName.ORDER_DETAIL_Field_CookAckDateTime] = DateTime.Now;
                RowItem[TableName.ORDER_DETAIL_Field_CookAckUserID] = param.CookAckUserID;
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(xDataTable);
            ds.Tables.Add(xDataTableItem);
            condb.Transaction_UpdateDataSet(ds, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;
        }

        public UpdateOrder_Result UpdateCancelOrder(Data_ORDER_HEAD_Param param)
        {
            UpdateOrder_Result result = new UpdateOrder_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.OrderNo))
            {
                result.ErrorMessage = "ระบุเลข Order";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);

            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }
            else
            {
                if (DateTime.MinValue != DxData.getValueDateTime(xDataTable.Rows[0][TableName.ORDER_HEAD_Field_CxlDateTime]))
                {
                    result.ErrorMessage = "รายการนี้ถูกยกเลิกแล้ว";
                    return result;
                }

                if (DateTime.MinValue != DxData.getValueDateTime(xDataTable.Rows[0][TableName.ORDER_HEAD_Field_CookAckDateTime]))
                {
                    result.ErrorMessage = "รายการนี้ครัวกำลังทำ";
                    return result;
                }

            }

            dbCondition = DBExpression.Normal(TableName.ORDER_DETAIL_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);
            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการอาหาร";
                return result;
            }


            foreach (DataRow RowItem in xDataTable.Rows)
            {
                RowItem[TableName.ORDER_HEAD_Field_CxlDateTime] = DateTime.Now;
                RowItem[TableName.ORDER_HEAD_Field_CxlUserID] = param.CxlUserID;
            }

            foreach (DataRow RowItem in xDataTableItem.Rows)
            {
                RowItem[TableName.ORDER_DETAIL_Field_CxlDateTime] = DateTime.Now;
                RowItem[TableName.ORDER_DETAIL_Field_CxlUserID] = param.CxlUserID;
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(xDataTable);
            ds.Tables.Add(xDataTableItem);
            condb.Transaction_UpdateDataSet(ds, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;
        }

        public UpdateOrder_Result UpdateCancelItemOrder(Data_ORDER_DETAIL_Param param)
        {
            UpdateOrder_Result result = new UpdateOrder_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.OrderNo))
            {
                result.ErrorMessage = "ระบุเลข Order";
                return result;
            }

            if (string.IsNullOrEmpty(param.Suffix))
            {
                result.ErrorMessage = "ระบุเลข Suffix";
                return result;
            }

            if (string.IsNullOrEmpty(param.MenuID))
            {
                result.ErrorMessage = "ระบุเลข MenuID";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);
            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }
            else
            {
                if (DateTime.MinValue != DxData.getValueDateTime(xDataTable.Rows[0][TableName.ORDER_HEAD_Field_CxlDateTime]))
                {
                    result.ErrorMessage = "รายการนี้ถูกยกเลิกแล้ว";
                    return result;
                }

                if (DateTime.MinValue != DxData.getValueDateTime(xDataTable.Rows[0][TableName.ORDER_HEAD_Field_CookAckDateTime]))
                {
                    result.ErrorMessage = "รายการนี้ครัวกำลังทำ";
                    return result;
                }

            }

            dbCondition = DBExpression.Normal(TableName.ORDER_DETAIL_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo)
                & DBExpression.Normal(TableName.ORDER_DETAIL_Field_Suffix, DBComparisonOperator.EqualEver, DxConvert.ConvertStringToInt(param.Suffix))
                & DBExpression.Normal(TableName.ORDER_DETAIL_Field_MenuID, DBComparisonOperator.EqualEver, param.MenuID);

            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการอาหาร";
                return result;
            }

            foreach (DataRow RowItem in xDataTableItem.Rows)
            {
                RowItem[TableName.ORDER_DETAIL_Field_CxlDateTime] = DateTime.Now;
                RowItem[TableName.ORDER_DETAIL_Field_CxlUserID] = param.CxlUserID;
                break;
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(xDataTableItem);
            condb.Transaction_UpdateDataSet(ds, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;

        }

        public UpdateOrder_Result UpdateAckCookItemOrder(Data_ORDER_DETAIL_Param param)
        {
            UpdateOrder_Result result = new UpdateOrder_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.OrderNo))
            {
                result.ErrorMessage = "ระบุเลข Order";
                return result;
            }

            if (string.IsNullOrEmpty(param.Suffix))
            {
                result.ErrorMessage = "ระบุเลข Suffix";
                return result;
            }

            if (string.IsNullOrEmpty(param.MenuID))
            {
                result.ErrorMessage = "ระบุเลข MenuID";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);
            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }
            else
            {
                if (DateTime.MinValue != DxData.getValueDateTime(xDataTable.Rows[0][TableName.ORDER_HEAD_Field_CxlDateTime]))
                {
                    result.ErrorMessage = "รายการนี้ถูกยกเลิกแล้ว";
                    return result;
                }

            }

            dbCondition = DBExpression.Normal(TableName.ORDER_DETAIL_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo)
                & DBExpression.Normal(TableName.ORDER_DETAIL_Field_Suffix, DBComparisonOperator.EqualEver, DxConvert.ConvertStringToInt(param.Suffix))
                & DBExpression.Normal(TableName.ORDER_DETAIL_Field_MenuID, DBComparisonOperator.EqualEver, param.MenuID);

            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการอาหาร";
                return result;
            }


            foreach (DataRow RowHead in xDataTable.Rows)
            {
                if (DateTime.MinValue == DxData.getValueDateTime(RowHead[TableName.ORDER_HEAD_Field_CookAckDateTime]))
                {
                    RowHead[TableName.ORDER_HEAD_Field_CookAckDateTime] = DateTime.Now;
                    RowHead[TableName.ORDER_HEAD_Field_CookAckUserID] = param.CookAckUserID;
                }
            }

            foreach (DataRow RowItem in xDataTableItem.Rows)
            {
                RowItem[TableName.ORDER_DETAIL_Field_CookAckDateTime] = DateTime.Now;
                RowItem[TableName.ORDER_DETAIL_Field_CookAckUserID] = param.CookAckUserID;
                break;
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(xDataTableItem);
            condb.Transaction_UpdateDataSet(ds, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;

        }


        public UpdateOrder_Result UpdateFinishCookItemOrder(Data_ORDER_DETAIL_Param param)
        {
            UpdateOrder_Result result = new UpdateOrder_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.OrderNo))
            {
                result.ErrorMessage = "ระบุเลข Order";
                return result;
            }

            if (string.IsNullOrEmpty(param.Suffix))
            {
                result.ErrorMessage = "ระบุเลข Suffix";
                return result;
            }

            if (string.IsNullOrEmpty(param.MenuID))
            {
                result.ErrorMessage = "ระบุเลข MenuID";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);
            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }
            else
            {
                if (DateTime.MinValue != DxData.getValueDateTime(xDataTable.Rows[0][TableName.ORDER_HEAD_Field_CxlDateTime]))
                {
                    result.ErrorMessage = "รายการนี้ถูกยกเลิกแล้ว";
                    return result;
                }

            }

            dbCondition = DBExpression.Normal(TableName.ORDER_DETAIL_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo)
                & DBExpression.Normal(TableName.ORDER_DETAIL_Field_Suffix, DBComparisonOperator.EqualEver, DxConvert.ConvertStringToInt(param.Suffix))
                & DBExpression.Normal(TableName.ORDER_DETAIL_Field_MenuID, DBComparisonOperator.EqualEver, param.MenuID);

            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการอาหาร";
                return result;
            }

            foreach (DataRow RowItem in xDataTableItem.Rows)
            {
                RowItem[TableName.ORDER_DETAIL_Field_FinishCookDateTime] = DateTime.Now;
                RowItem[TableName.ORDER_DETAIL_Field_FinishCookUserID] = param.FinishCookUserID;
                break;
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(xDataTableItem);
            condb.Transaction_UpdateDataSet(ds, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;

        }

        public UpdateOrder_Result UpdateServeCookItemOrder(Data_ORDER_DETAIL_Param param)
        {
            UpdateOrder_Result result = new UpdateOrder_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.OrderNo))
            {
                result.ErrorMessage = "ระบุเลข Order";
                return result;
            }

            if (string.IsNullOrEmpty(param.Suffix))
            {
                result.ErrorMessage = "ระบุเลข Suffix";
                return result;
            }

            if (string.IsNullOrEmpty(param.MenuID))
            {
                result.ErrorMessage = "ระบุเลข MenuID";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo);
            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }
            else
            {
                if (DateTime.MinValue != DxData.getValueDateTime(xDataTable.Rows[0][TableName.ORDER_HEAD_Field_CxlDateTime]))
                {
                    result.ErrorMessage = "รายการนี้ถูกยกเลิกแล้ว";
                    return result;
                }

            }

            dbCondition = DBExpression.Normal(TableName.ORDER_DETAIL_Field_OrderNo, DBComparisonOperator.EqualEver, param.OrderNo)
                & DBExpression.Normal(TableName.ORDER_DETAIL_Field_Suffix, DBComparisonOperator.EqualEver, DxConvert.ConvertStringToInt(param.Suffix))
                & DBExpression.Normal(TableName.ORDER_DETAIL_Field_MenuID, DBComparisonOperator.EqualEver, param.MenuID);

            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการอาหาร";
                return result;
            }

            foreach (DataRow RowItem in xDataTableItem.Rows)
            {
                RowItem[TableName.ORDER_DETAIL_Field_ServeCookDateTime] = DateTime.Now;
                RowItem[TableName.ORDER_DETAIL_Field_ServeCookUserID] = param.ServeCookUserID;
                break;
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(xDataTableItem);
            condb.Transaction_UpdateDataSet(ds, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;

        }

        public EnquireOrderSummary_Result EnquireOrderSummary(EnquireOrderSummary_Param param)
        {
            EnquireOrderSummary_Result result = new EnquireOrderSummary_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.TableID))
            {
                result.ErrorMessage = "ระบุเลขโต๊ะ";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_TableID, DBComparisonOperator.EqualEver, param.TableID)
                & DBExpression.From_CheckValueSet(TableName.ORDER_HEAD_Field_CxlDateTime, SqlDbType.DateTime, CheckValueType.Not_Set)
                & DBExpression.From_CheckValueSet(TableName.ORDER_HEAD_Field_ReceiveDateTime, SqlDbType.DateTime, CheckValueType.Not_Set);

            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }

            List<string> ListOfOrderNo = DxCollection.GetDistinctCodeList(xDataTable, TableName.ORDER_HEAD_Field_OrderNo);
            dbCondition = DBExpression.IN(TableName.ORDER_DETAIL_Field_OrderNo, ListOfOrderNo);
            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }
            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการอาหาร";
                return result;
            }

            double dChargeAmt = 0.0;
            foreach (DataRow xRow in xDataTable.Rows)
            {
                var xDataHeader = new Data_ORDER_HEAD_Param();
                xDataHeader.OrderDateTime = DxData.getValueDateTimeToString(xRow[TableName.ORDER_HEAD_Field_OrderDateTime]);
                xDataHeader.CxlDateTime = DxData.getValueDateTimeToString(xRow[TableName.ORDER_HEAD_Field_CxlDateTime]);
                xDataHeader.CookAckDateTime = DxData.getValueDateTimeToString(xRow[TableName.ORDER_HEAD_Field_CookAckDateTime]);
                xDataHeader.ReceiveDateTime = DxData.getValueDateTimeToString(xRow[TableName.ORDER_HEAD_Field_ReceiveDateTime]);
                xDataHeader.OrderNo = DxData.getValueString(xRow[TableName.ORDER_HEAD_Field_OrderNo]);
                xDataHeader.TableID = DxData.getValueString(xRow[TableName.ORDER_HEAD_Field_TableID]);
                xDataHeader.OrderByUserID = DxData.getValueString(xRow[TableName.ORDER_HEAD_Field_OrderByUserID]);
                xDataHeader.CxlUserID = DxData.getValueString(xRow[TableName.ORDER_HEAD_Field_CxlUserID]);
                xDataHeader.CookAckUserID = DxData.getValueString(xRow[TableName.ORDER_HEAD_Field_CookAckUserID]);
                xDataHeader.ReceiveUserID = DxData.getValueString(xRow[TableName.ORDER_HEAD_Field_ReceiveUserID]);

                dbCondition = DBExpression.Normal(TableName.ORDER_DETAIL_Field_OrderNo, DBComparisonOperator.EqualEver, xDataHeader.OrderNo);

                double OrderChargeAmt = 0;
                foreach (DataRow RowItem in xDataTableItem.Select(dbCondition.ToString(), TableName.ORDER_DETAIL_Field_Suffix))
                {
                    var itemData = new Data_ORDER_DETAIL_Param();
                    itemData.CookAckDateTime = DxData.getValueDateTimeToString(RowItem[TableName.ORDER_DETAIL_Field_CookAckDateTime]);
                    itemData.CxlDateTime = DxData.getValueDateTimeToString(RowItem[TableName.ORDER_DETAIL_Field_CxlDateTime]);
                    itemData.OrderNo = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_OrderNo]);
                    itemData.Suffix = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_Suffix]);
                    itemData.MenuName = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_MenuName]);
                    itemData.MenuID = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_MenuID]);
                    itemData.MenuMemo = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_MenuMemo]);
                    itemData.CookAckUserID = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_CookAckUserID]);
                    itemData.CxlUserID = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_CxlUserID]);
                    itemData.ChargeAmt = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_ChargeAmt]);
                    itemData.Qty = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_Qty]);

                    if (string.IsNullOrEmpty(xDataHeader.CxlDateTime) && string.IsNullOrEmpty(itemData.CxlDateTime))
                    {
                        OrderChargeAmt += DxData.getValueDouble(RowItem[TableName.ORDER_DETAIL_Field_ChargeAmt]);
                        dChargeAmt += DxData.getValueDouble(RowItem[TableName.ORDER_DETAIL_Field_ChargeAmt]);
                    }

                    xDataHeader.ListOfItem.Add(itemData);
                }

                xDataHeader.ChargeAmt = OrderChargeAmt.ToString();
                result.ListOfSummaryOrder.Add(xDataHeader);
            }
            result.ChargeAmt = dChargeAmt.ToString();
            result.TotalChargeAmt = dChargeAmt.ToString();

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;
        }

        public EnquireOrderCooking_Result EnquireOrderCooking(EnquireOrderCooking_Param param)
        {
            EnquireOrderCooking_Result result = new EnquireOrderCooking_Result();
            result.ResultStatus = false;

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_TableID, DBComparisonOperator.Equal, param.TableID)
                & DBExpression.From_CheckValueSet(TableName.ORDER_HEAD_Field_CxlDateTime, SqlDbType.DateTime, CheckValueType.Not_Set)
                & DBExpression.From_CheckValueSet(TableName.ORDER_HEAD_Field_ReceiveDateTime, SqlDbType.DateTime, CheckValueType.Not_Set);

            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }

            List<string> ListOfOrderNo = DxCollection.GetDistinctCodeList(xDataTable, TableName.ORDER_HEAD_Field_OrderNo);
            dbCondition = DBExpression.IN(TableName.ORDER_DETAIL_Field_OrderNo, ListOfOrderNo);
            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }
            if (xDataTableItem.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการอาหาร";
                return result;
            }

            foreach (DataRow RowItem in xDataTableItem.Select(dbCondition.ToString(), TableName.ORDER_DETAIL_Field_EntryDateTime))
            {
                var itemData = new Data_ORDER_DETAIL_Param();
                itemData.CookAckDateTime = DxData.getValueDateTimeToString(RowItem[TableName.ORDER_DETAIL_Field_CookAckDateTime]);
                itemData.FinishCookDateTime = DxData.getValueDateTimeToString(RowItem[TableName.ORDER_DETAIL_Field_FinishCookDateTime]);
                itemData.ServeCookDateTime = DxData.getValueDateTimeToString(RowItem[TableName.ORDER_DETAIL_Field_ServeCookDateTime]);
                itemData.CxlDateTime = DxData.getValueDateTimeToString(RowItem[TableName.ORDER_DETAIL_Field_CxlDateTime]);
                itemData.OrderNo = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_OrderNo]);
                itemData.Suffix = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_Suffix]);
                itemData.MenuID = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_MenuID]);
                itemData.MenuName = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_MenuName]);
                itemData.MenuMemo = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_MenuMemo]);
                itemData.CookAckUserID = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_CookAckUserID]);
                itemData.CxlUserID = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_CxlUserID]);
                itemData.ChargeAmt = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_ChargeAmt]);
                itemData.Qty = DxData.getValueString(RowItem[TableName.ORDER_DETAIL_Field_Qty]);
                itemData.EntryDateTime = DxData.getValueDateTimeHHMMSSToString(RowItem[TableName.ORDER_DETAIL_Field_EntryDateTime]);

                dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_OrderNo, DBComparisonOperator.EqualEver, itemData.OrderNo);
                xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
                xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
                xGetDataOptionParam.Condition = dbCondition.ToString();
                DataTable xDataTableHead = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
                if (xDataTableHead.Rows.Count > 0)
                {
                    itemData.TableID = DxData.getValueString(xDataTableHead.Rows[0][TableName.ORDER_HEAD_Field_TableID]);
                }

                if (!string.IsNullOrEmpty(itemData.ServeCookDateTime))
                {
                    result.ListOfItemServed.Add(itemData);
                }
                else if (!string.IsNullOrEmpty(itemData.FinishCookDateTime))
                {
                    result.ListOfItemCookFinish.Add(itemData);
                }
                else if (!string.IsNullOrEmpty(itemData.CookAckDateTime))
                {
                    result.ListOfItemAcked.Add(itemData);
                }
                else
                {
                    result.ListOfItemWaitAck.Add(itemData);
                }
            }

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;
        }

        public UpdateOrder_Result UpdatePayment(Data_ORDER_HEAD_Param param)
        {
            UpdateOrder_Result result = new UpdateOrder_Result();
            result.ResultStatus = false;

            if (string.IsNullOrEmpty(param.TableID))
            {
                result.ErrorMessage = "ระบุเลข Order";
                return result;
            }

            if (string.IsNullOrEmpty(param.ChargeAmt))
            {
                result.ErrorMessage = "ระบุยอดเงินที่ชำระ";
                return result;
            }

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());
            DBCondition dbCondition = DBExpression.Normal(TableName.ORDER_HEAD_Field_TableID, DBComparisonOperator.EqualEver, param.TableID)
                & DBExpression.From_CheckValueSet(TableName.ORDER_HEAD_Field_CxlDateTime, SqlDbType.DateTime, CheckValueType.Not_Set)
                & DBExpression.From_CheckValueSet(TableName.ORDER_HEAD_Field_ReceiveDateTime, SqlDbType.DateTime, CheckValueType.Not_Set);

            string strErrorMessage = string.Empty;
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_HEAD;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTable = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
            if (xDataTable.Rows.Count == 0)
            {
                result.ErrorMessage = "ไม่พบรายการที่สั่ง!";
                return result;
            }

            DateTime dtReceiveDateTime = DateTime.Now;
            foreach (DataRow RowOrderHead in xDataTable.Rows)
            {
                RowOrderHead[TableName.ORDER_HEAD_Field_ReceiveDateTime] = dtReceiveDateTime;
                RowOrderHead[TableName.ORDER_HEAD_Field_ReceiveUserID] = param.ReceiveUserID;
            }

            List<string> ListOfOrderNo = DxCollection.GetDistinctCodeList(xDataTable, TableName.ORDER_HEAD_Field_OrderNo);
            dbCondition = DBExpression.IN(TableName.ORDER_DETAIL_Field_OrderNo, ListOfOrderNo);
            xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = TableName.ORDER_DETAIL;
            xGetDataOptionParam.Condition = dbCondition.ToString();
            DataTable xDataTableItem = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);
             
            ////
            DataTable xDataTableRec = condb.GetSchemaTable(TableName.RECEIVE_HEAD);
            DataTable xDataTableRecDtl = condb.GetSchemaTable(TableName.RECEIVE_DETAIL);


            if (string.IsNullOrEmpty(param.PaidType))
            {
                param.PaidType = "CASH";
            }

            DataRow xNewRow = xDataTableRec.NewRow();
            string strReceiveNo = param.TableID + dtReceiveDateTime.Ticks;
            ///
            xNewRow[TableName.RECEIVE_HEAD_Field_ReceiveDateTime] = dtReceiveDateTime;
            xNewRow[TableName.RECEIVE_HEAD_Field_RecevieNo] = strReceiveNo;
            xNewRow[TableName.RECEIVE_HEAD_Field_TableID] = param.TableID;
            xNewRow[TableName.RECEIVE_HEAD_Field_ReceiveUserID] = param.ReceiveUserID;
            xNewRow[TableName.RECEIVE_HEAD_Field_Memo] = param.Memo;
            xNewRow[TableName.RECEIVE_HEAD_Field_PaidType] = param.PaidType;
            xNewRow[TableName.RECEIVE_HEAD_Field_ChargeAmt] = DxConvert.ConvertStringToDouble(param.ChargeAmt);
            xNewRow[TableName.RECEIVE_HEAD_Field_DiscountAmt] = DxConvert.ConvertStringToDouble(param.DiscountAmt);
            xNewRow[TableName.RECEIVE_HEAD_Field_TotalAmt] = DxConvert.ConvertStringToDouble(param.TotalAmt);
            xDataTableRec.Rows.Add(xNewRow);

            int iSuffix = 1;
            foreach (DataRow RowOrderHead in xDataTable.Rows)
            {
                xNewRow = xDataTableRecDtl.NewRow();
                xNewRow[TableName.RECEIVE_DETAIL_Field_RecevieNo] = strReceiveNo;
                xNewRow[TableName.RECEIVE_DETAIL_Field_OrderNo] = RowOrderHead[TableName.ORDER_HEAD_Field_OrderNo];
                ////
                dbCondition = DBExpression.Normal(TableName.ORDER_DETAIL_Field_OrderNo, DBComparisonOperator.EqualEver, DxData.getValueString(RowOrderHead[TableName.ORDER_HEAD_Field_OrderNo]));
                ///
                double OrderChargeAmt = 0;
                foreach (DataRow RowItem in xDataTableItem.Select(dbCondition.ToString(), TableName.ORDER_DETAIL_Field_Suffix))
                {
                    string strCxlDateTime = DxData.getValueDateTimeToString(RowItem[TableName.ORDER_DETAIL_Field_CxlDateTime]);
                    if (string.IsNullOrEmpty(strCxlDateTime))
                    {
                        OrderChargeAmt += DxData.getValueDouble(RowItem[TableName.ORDER_DETAIL_Field_ChargeAmt]);
                    }
                }

                xNewRow[TableName.RECEIVE_DETAIL_Field_ChargeAmt] = OrderChargeAmt;
                xNewRow[TableName.RECEIVE_DETAIL_Field_Suffix] = iSuffix++;
                xDataTableRecDtl.Rows.Add(xNewRow);
            }

            DataSet ds = new DataSet();
            ds.Tables.Add(xDataTable);
            ds.Tables.Add(xDataTableRec);
            ds.Tables.Add(xDataTableRecDtl);

            condb.Transaction_UpdateDataSet(ds, out strErrorMessage);

            if (!string.IsNullOrEmpty(strErrorMessage))
            {
                result.ErrorMessage = strErrorMessage;
                return result;
            }

            result.ResultStatus = string.IsNullOrEmpty(strErrorMessage);
            return result;
        }

        #endregion [ORDER]

        public string GenQRBase64(string data)
        {
            return DxImage.ConvertByteArrayToBase64(DxImage.QRCodeGenerate(data));
        }


        public string getMasterDataUserName(string strUserID)
        {
            string strUserName = strUserID;

            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, "USERINFO")
                & DBExpression.Normal("MasterID", DBComparisonOperator.EqualEver, strUserID);

            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "MASTERDATA";
            xGetDataOptionParam.Condition = dbCondition.ToString();
            string strErrorMessage = string.Empty;
            DataTable xDataTableMasterUserID = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);

            DataTable xDataTableReturn = new DataTable("USERINFO");
            xDataTableReturn.Columns.Add("UserID");
            xDataTableReturn.Columns.Add("UserNameEN");
            xDataTableReturn.Columns.Add("UserNameTH");
            xDataTableReturn.Columns.Add("Email");
            xDataTableReturn.Columns.Add("ImageData", typeof(System.Byte[]));

            DataRow xNewRow;
            foreach (DataRow item in xDataTableMasterUserID.Rows)
            {
                strUserName = DxData.getValueString(item["MasterNameEnglish"]);
                if (string.IsNullOrEmpty(strUserName))
                {
                    strUserName = DxData.getValueString(item["MasterNameThai"]);
                }
            }

            return strUserName;

        }
        public DataTable getMasterDataUserID(string strUserID)
        {
            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, "USERINFO")
                & DBExpression.Normal("MasterID", DBComparisonOperator.EqualEver, strUserID);

            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "MASTERDATA";
            xGetDataOptionParam.Condition = dbCondition.ToString();
            string strErrorMessage = string.Empty;
            DataTable xDataTableMasterUserID = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);

            DataTable xDataTableReturn = new DataTable("USERINFO");
            xDataTableReturn.Columns.Add("UserID");
            xDataTableReturn.Columns.Add("UserNameEN");
            xDataTableReturn.Columns.Add("UserNameTH");
            xDataTableReturn.Columns.Add("Email");
            xDataTableReturn.Columns.Add("ImageData", typeof(System.Byte[]));

            DataRow xNewRow;
            foreach (DataRow item in xDataTableMasterUserID.Rows)
            {
                xNewRow = xDataTableReturn.NewRow();
                xNewRow["UserID"] = DxData.getValueString(item["MasterID"]);
                xNewRow["UserNameEN"] = DxData.getValueString(item["MasterNameEnglish"]);
                xNewRow["UserNameTH"] = DxData.getValueString(item["MasterNameThai"]);
                xNewRow["Email"] = string.Empty;
                xNewRow["ImageData"] = DxData.getValueByteArray(item["ImageData"]);

                string strJsonData = ((string)item["MasterInfo"]).Replace("^", "\"");
                var mMasterDataUserInfo = JsonConvert.DeserializeObject<MasterData_UserInfo>(strJsonData);
                if (null != mMasterDataUserInfo.UserID)
                {
                    xNewRow["Email"] = mMasterDataUserInfo.UserID;
                }

                xDataTableReturn.Rows.Add(xNewRow);
            }

            return xDataTableReturn;
        }
        private DataTable getMasterDataSystem()
        {
            ConnectDB condb = new ConnectDB(ServiceUtil.getConnectionString());

            DBCondition dbCondition = DBExpression.Normal("MasterType", DBComparisonOperator.EqualEver, "SYSTEM")
                & DBExpression.Normal("MasterID", DBComparisonOperator.EqualEver, "SYS");

            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "MASTERDATA";
            xGetDataOptionParam.Condition = dbCondition.ToString();
            string strErrorMessage = string.Empty;
            DataTable xDataTableMasterUserID = condb.GetDataTable(xGetDataOptionParam, out strErrorMessage);

            //var mMasterDataSystem = JsonConvert.DeserializeObject<MasterDataSystem>(param.MasterData)

            DataTable xDataTableReturn = new DataTable("TB_OFFICE_INFO");
            //xDataTableReturn.Columns.Add("FormatNumberQuatation");
            //xDataTableReturn.Columns.Add("FormatNumberInvoice");
            xDataTableReturn.Columns.Add("OfficerName");
            xDataTableReturn.Columns.Add("OfficerAddress");
            xDataTableReturn.Columns.Add("OfficerEmail");
            xDataTableReturn.Columns.Add("OfficerTax");
            xDataTableReturn.Columns.Add("OfficerTelePhone");
            xDataTableReturn.Columns.Add("OfficerMobilePhone");
            xDataTableReturn.Columns.Add("CreditDay");
            xDataTableReturn.Columns.Add("BankAccount");
            xDataTableReturn.Columns.Add("BankAccountName");
            xDataTableReturn.Columns.Add("BankBrance");
            xDataTableReturn.Columns.Add("BankName");

            DataRow xNewRow;
            foreach (DataRow item in xDataTableMasterUserID.Rows)
            {
                xNewRow = xDataTableReturn.NewRow();
                xNewRow["OfficerName"] = string.Empty;
                xNewRow["OfficerAddress"] = string.Empty;
                xNewRow["OfficerEmail"] = string.Empty;
                xNewRow["OfficerTax"] = string.Empty;
                xNewRow["OfficerTelePhone"] = string.Empty;
                xNewRow["OfficerMobilePhone"] = string.Empty;
                xNewRow["CreditDay"] = string.Empty;
                xNewRow["BankAccount"] = string.Empty;
                xNewRow["BankAccountName"] = string.Empty;
                xNewRow["BankBrance"] = string.Empty;
                xNewRow["BankName"] = string.Empty;

                string strJsonData = ((string)item["MasterInfo"]).Replace("^", "\"");
                var mMasterDataUserInfo = JsonConvert.DeserializeObject<MasterDataSystem>(strJsonData);
                xNewRow["OfficerName"] = mMasterDataUserInfo.OfficerName;
                xNewRow["OfficerAddress"] = mMasterDataUserInfo.OfficerAddress;
                xNewRow["OfficerEmail"] = mMasterDataUserInfo.OfficerEmail;
                xNewRow["OfficerTax"] = mMasterDataUserInfo.OfficerTax;
                xNewRow["OfficerTelePhone"] = mMasterDataUserInfo.OfficerTelePhone;
                xNewRow["OfficerMobilePhone"] = mMasterDataUserInfo.OfficerMobilePhone;
                xNewRow["CreditDay"] = mMasterDataUserInfo.CreditDay;
                ///
                xNewRow["BankAccount"] = mMasterDataUserInfo.BankAccount;
                xNewRow["BankAccountName"] = mMasterDataUserInfo.BankAccountName;
                xNewRow["BankBrance"] = mMasterDataUserInfo.BankBrance;
                xNewRow["BankName"] = mMasterDataUserInfo.BankName;

                xDataTableReturn.Rows.Add(xNewRow);
            }

            return xDataTableReturn;
        }

        public string getUserIDName(string strUserID)
        {
            return strUserID;

            DataTable dt = getMasterDataUserID(strUserID);
            if (dt.Rows.Count > 0)
            {
                string nameEng = DxData.getValueString(dt.Rows[0]["UserNameEN"]);
                if (string.IsNullOrEmpty(nameEng))
                {
                    nameEng = DxData.getValueString(dt.Rows[0]["UserNameTH"]);
                }
                if (string.IsNullOrEmpty(nameEng))
                {
                    nameEng = strUserID;
                }

                if (string.IsNullOrEmpty(nameEng))
                {
                    return strUserID;
                }

                return nameEng;
            }

            return strUserID;
        }


    }



}