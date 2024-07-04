 
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

namespace APIRestServiceRestaurant
{
    public class RESTService : IRestService
    {
        public static string MasterKey_USERINFO = "USERINFO";
        public static string MasterKey_MENU = "MENU";
        public static string MasterKey_STOCK = "STOCK"; 

        public void DoWork()
        {

        }
       
        //public string TestGenRef(string runcode)
        //{
        //    string strRefNo = string.Empty;
        //    string strError = string.Empty;
        //    DxGenerate.GenRef(ServiceUtil.getConnectionString(), runcode, true, out strRefNo, out strError); 
        //    //2023-02-22 
        //    return strRefNo;
        //} 
        #region [MASTER DATA]

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
        
        public string getUserIDName(string strUserID) {

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
                var mMasterDataUserInfo = JsonConvert.DeserializeObject<MasterDataUserInfo>(strJsonData);
                if (null != mMasterDataUserInfo.UserEmail)
                {
                    xNewRow["Email"] = mMasterDataUserInfo.UserEmail;
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
                if (mUserInfo.UserID == param.UserID && mUserInfo.UserPassword == param.UserPassword)
                {
                    mUserInfo.UserPassword = "";
                    result.UserInfo = mUserInfo;
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
                var menuData = JsonConvert.DeserializeObject<MasterData_MenuInfo>(DxData.getValueString(row["MasterData"]));
                menuData.MenuNameShow = DxData.getValueString(row["MasterNameEnglish"]);
                menuData.MenuNameShowThai = DxData.getValueString(row["MasterNameThai"]);   
                result.ListOfMasterData.Add(menuData); 
            }

            result.ResultStatus = true;
            return result;

        }

        public static double getPrice(string strMenuID)
        {
            double dPrice = 0.0;



            return dPrice;
        }
          
        #endregion [MASTER DATA]


        #region [ORDER]

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
                xNewRow[TableName.ORDER_DETAIL_Field_Suffix] = iSuffix; 
                xNewRow[TableName.ORDER_DETAIL_Field_OrderNo] = itemData.OrderNo;
                xNewRow[TableName.ORDER_DETAIL_Field_MenuID] = itemData.MenuID;
                xNewRow[TableName.ORDER_DETAIL_Field_MenuMemo] = itemData.MenuMemo;

                if (string.IsNullOrEmpty(itemData.ChargeAmt))
                {
                    xNewRow[TableName.ORDER_DETAIL_Field_ChargeAmt] = getPrice(itemData.MenuID);
                }
                else
                {
                    xNewRow[TableName.ORDER_DETAIL_Field_ChargeAmt] = DxConvert.ConvertStringToDouble(itemData.ChargeAmt);
                } 

                xDataTable.Rows.Add(xNewRow); 
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
         
        #endregion [ORDER]

        public string GenQRBase64(string data)
        {
            return DxImage.ConvertByteArrayToBase64(DxImage.QRCodeGenerate(data));
        }
         
    }



}