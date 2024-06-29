using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace APIRestServiceRestaurant
{
    public class DxGenerate
    {


        #region GenRunningDev
         

        public static bool GenRef(string strConnectionMaster, string strRunningRefIndex, bool Test, out string strRefNo, out string strError)
        {
            bool bReturn = false;
            strRefNo = string.Empty;
            strError = string.Empty;

            int iCountDataErrorAll = 0;
            using (SqlConnection sqlConnectionMaster = new SqlConnection(strConnectionMaster))
            {
                sqlConnectionMaster.Open();
                using (SqlTransaction transMaster = sqlConnectionMaster.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {   
                            try
                            {
                                bReturn = GenRef(sqlConnectionMaster, transMaster, strConnectionMaster, strRunningRefIndex, Test, out strRefNo, out strError);
                            }
                            catch (InvalidOperationException Ex)
                            {
                                strError += string.Format("Commit Exception Type: {0} ", Ex.GetType());
                                strError += string.Format("  Message: {0}", Ex.Message);

                                transMaster.Rollback();
                                iCountDataErrorAll++;
                                //strError += Ex.Message.ToString();
                                throw Ex;
                            }
                            catch (SqlException SQLEx)
                            {
                                strError += string.Format("Commit Exception Type: {0} ", SQLEx.GetType());
                                strError += string.Format("  Message: {0}", SQLEx.Message);
                                transMaster.Rollback();
                                iCountDataErrorAll++;
                                //strError += SQLEx.Message.ToString();
                            }
                            catch (Exception Ex)
                            {
                                strError += string.Format("Commit Exception Type: {0} ", Ex.GetType());
                                strError += string.Format("  Message: {0}", Ex.Message);
                                transMaster.Rollback();
                                iCountDataErrorAll++;
                                //strError += Ex.Message.ToString();

                                throw Ex;
                            }
                            finally
                            {
                                if (0 == iCountDataErrorAll)
                                {
                                    try
                                    {
                                        transMaster.Commit();
                                    }
                                    catch (Exception Ex)
                                    {
                                        strError += string.Format("Commit Exception Type: {0} ", Ex.GetType());
                                        strError += string.Format("  Message: {0}", Ex.Message);
                                    }
                                }
                            }
                                         
                }
                sqlConnectionMaster.Close();
            }
            return bReturn;
        }

        public static bool GenRef(SqlConnection sqlConnectionMaster, SqlTransaction transMaster, string strConnectionMaster, string strRunningCode, bool Test, out string strRefNo, out string strError)
        {
            strRefNo = string.Empty;
            strError = string.Empty;

            DataTable xDataTableRunningRefMaster = GetRunningRefMasterData(strConnectionMaster, strRunningCode, out strError);
            if (xDataTableRunningRefMaster.Rows.Count == 0)
            {
                return false;
            }

            strRefNo = (string)xDataTableRunningRefMaster.Rows[0]["RuningFormat"];

            strRefNo = strRefNo.Trim();
            if (string.IsNullOrEmpty(strRefNo))
            {
                strError = "Runing Format empty";
                return false;
            }

            //TB_SETUP_RUNNING_NUMBER tbRunNumber = new TB_SETUP_RUNNING_NUMBER();
            string tbRunNumber = "SETUP_RUNNING_NUMBER";
            string strDBCondition;
            DBCondition xDBCondition;
            DataTable xDataTable = null;

            string strTemp, strSearch;
            DateTime dtDateNow = DateTime.Now;
            DateTime vGenDate = new DateTime(dtDateNow.Year, dtDateNow.Month, dtDateNow.Day, dtDateNow.Hour, dtDateNow.Minute, dtDateNow.Second, dtDateNow.Millisecond);
            DateTime vLastRuningDateTime = DxDate.DateTimeEmpty();

            bool bDayChanged = false;
            bool bMonthChanged = false;
            bool bYearChanged = false;
            int iRunningNo1 = 0;
            int iRunningNo2 = 0;
            int iYear = -1;
            int iMonth = -1;


            ConnectDB condb = new ConnectDB(strConnectionMaster);
            DBCondition dbCondition = DBExpression.Normal("Code", DBComparisonOperator.EqualEver, strRunningCode);
            ///
            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "SETUP_RUNNING_NUMBER";
            xGetDataOptionParam.Condition = dbCondition.ToString();
            xGetDataOptionParam.ScanFillUp = true;

            xDataTable = condb.GetDataTable(xGetDataOptionParam, out strError);
            if (!string.IsNullOrEmpty(strError))
            {
                return false;
            }
            xDataTable.TableName = "SETUP_RUNNING_NUMBER";

            if (xDataTable.Rows.Count > 0)
            {
                iRunningNo1 = (int)xDataTable.Rows[0]["Runing"];
                iRunningNo2 = (int)xDataTable.Rows[0]["Runing2"];
                vLastRuningDateTime = (DateTime)xDataTable.Rows[0]["LastRuningDateTime"];
            }
            else
            {
                iRunningNo1 = (int)xDataTableRunningRefMaster.Rows[0]["Runing"];
                iRunningNo2 = (int)xDataTableRunningRefMaster.Rows[0]["Runing2"];
            }

            if ((byte)xDataTableRunningRefMaster.Rows[0]["RunningRefResetType"] != (byte)RunningRefResetType.None &&
                !DxDate.DateTimeIsEmpty(vLastRuningDateTime))
            {
                if (vGenDate < vLastRuningDateTime)
                {
                    TimeSpan xTimeSpan;
                    xTimeSpan = vLastRuningDateTime.Subtract(vGenDate);
                    if (xTimeSpan.TotalDays > 2.0)
                    {
                        strError = "ToDay Date Less Than LastRuningDateTime";
                        return false;
                    }
                }
                if (vGenDate.Year != vLastRuningDateTime.Year)
                {
                    bDayChanged = true;
                    bMonthChanged = true;
                    bYearChanged = true;
                }
                else if (vGenDate.Month != vLastRuningDateTime.Month)
                {
                    bDayChanged = true;
                    bMonthChanged = true;
                }
                else if (vGenDate.Day != vLastRuningDateTime.Day)
                {
                    bDayChanged = true;
                }

                bool bOk = false;
                if ((byte)xDataTableRunningRefMaster.Rows[0]["RunningRefResetType"] == (byte)RunningRefResetType.Daily)
                {
                    if (bDayChanged) bOk = true;
                }
                else if ((byte)xDataTableRunningRefMaster.Rows[0]["RunningRefResetType"] == (byte)RunningRefResetType.Monthly)
                {
                    if (bMonthChanged) bOk = true;
                }
                else if ((byte)xDataTableRunningRefMaster.Rows[0]["RunningRefResetType"] == (byte)RunningRefResetType.Yearly)
                {
                    if (bYearChanged) bOk = true;
                }

                if (bOk)
                {
                    iRunningNo1 = 0;
                    iRunningNo2 = 0;
                    if ((int)xDataTableRunningRefMaster.Rows[0]["StartAtRuning"] > 0)
                    {
                        iRunningNo1 = (int)xDataTableRunningRefMaster.Rows[0]["StartAtRuning"] - 1;
                    }
                }
            }

            iRunningNo1++;
            if ((int)xDataTableRunningRefMaster.Rows[0]["ResetAtRuning"] > 0)
            {
                if (iRunningNo1 >= (int)xDataTableRunningRefMaster.Rows[0]["ResetAtRuning"])
                {
                    iRunningNo1 = 1;
                    if ((int)xDataTableRunningRefMaster.Rows[0]["StartAtRuning"] > 0)
                    {
                        iRunningNo1 = (int)xDataTableRunningRefMaster.Rows[0]["StartAtRuning"];
                    }
                    iRunningNo2++;
                }
            }

            iMonth = vGenDate.Month;
            iYear = vGenDate.Year;

            strSearch = "[D]";
            if (strRefNo.Contains(strSearch))
            {
                strTemp = string.Format("{0}", vGenDate.Day);
                strRefNo = strRefNo.Replace(strSearch, strTemp);
            }
            else
            {
                strSearch = "[DD]";
                if (strRefNo.Contains(strSearch))
                {
                    strTemp = string.Format("{0:D2}", vGenDate.Day);
                    strRefNo = strRefNo.Replace(strSearch, strTemp);
                }
            }

            strSearch = "[DOY]";
            if (strRefNo.Contains(strSearch))
            {
                strTemp = string.Format("{0}", vGenDate.DayOfYear);
                strRefNo = strRefNo.Replace(strSearch, strTemp);
            }
            else
            {
                strSearch = "[DDOY]";
                if (strRefNo.Contains(strSearch))
                {
                    strTemp = string.Format("{0:D3}", vGenDate.DayOfYear);
                    strRefNo = strRefNo.Replace(strSearch, strTemp);
                }
            }

            strSearch = "[M]";
            if (strRefNo.Contains(strSearch))
            {
                strTemp = string.Format("{0}", iMonth);
                strRefNo = strRefNo.Replace(strSearch, strTemp);
            }
            else
            {
                strSearch = "[MM]";
                if (strRefNo.Contains(strSearch))
                {
                    strTemp = string.Format("{0:D2}", iMonth);
                    strRefNo = strRefNo.Replace(strSearch, strTemp);
                }
            }

            strSearch = "[Y]";
            if (strRefNo.Contains(strSearch))
            {
                strTemp = string.Format("{0:D4}", iYear);
                strTemp = strTemp.Remove(0, 2);
                strRefNo = strRefNo.Replace(strSearch, strTemp);
            }

            strSearch = "[YY]";
            if (strRefNo.Contains(strSearch))
            {
                strTemp = string.Format("{0:D4}", iYear);
                strTemp = strTemp.Remove(0, 2);
                strRefNo = strRefNo.Replace(strSearch, strTemp);
            }
            else
            {
                strSearch = "[YYYY]";
                if (strRefNo.Contains(strSearch))
                {
                    strTemp = string.Format("{0:D4}", iYear);
                    strRefNo = strRefNo.Replace(strSearch, strTemp);
                }
            }

            strSearch = "[T]";
            if (strRefNo.Contains(strSearch))
            {
                strTemp = string.Format("{0:D4}", (iYear + 543));
                strTemp = strTemp.Remove(0, 2);
                strRefNo = strRefNo.Replace(strSearch, strTemp);
            }

            strSearch = "[TT]";
            if (strRefNo.Contains(strSearch))
            {
                strTemp = string.Format("{0:D4}", (iYear + 543));
                strTemp = strTemp.Remove(0, 2);
                strRefNo = strRefNo.Replace(strSearch, strTemp);
            }
            else
            {
                strSearch = "[TTTT]";
                if (strRefNo.Contains(strSearch))
                {
                    strTemp = string.Format("{0:D4}", (iYear + 543));
                    strRefNo = strRefNo.Replace(strSearch, strTemp);
                }
            }

            string strMainSearch = string.Empty;
            int i = 0;

            for (int iMain = 1; iMain <= 2; iMain++)
            {
                strMainSearch = string.Empty;
                if (1 == iMain) strMainSearch = "[A";
                else if (2 == iMain) strMainSearch = "[B";

                if (-1 == strRefNo.IndexOf(strMainSearch))
                    continue;

                for (int iLoop = 1; iLoop < 10; iLoop++)
                {
                    strSearch = strMainSearch;
                    strTemp = string.Format("{0}", iLoop);
                    strSearch += strTemp;
                    strSearch += "]";
                    i = strRefNo.IndexOf(strSearch);
                    if (-1 != i)
                    {
                        strTemp = string.Empty;
                        if (1 == iMain) strTemp = string.Format("{0:D9}", iRunningNo1);
                        else if (2 == iMain) strTemp = string.Format("{0:D9}", iRunningNo2);

                        while (strTemp.Length > iLoop)
                        {
                            if (!strTemp.StartsWith("0")) break;
                            strTemp = strTemp.Remove(0, 1);
                        }
                        strRefNo = strRefNo.Replace(strSearch, strTemp);
                    }
                }
            }

            if (Test)
            {
                return true;
            }

            DataRow xNewRow;

            if (0 == xDataTable.Rows.Count)
            {
                xNewRow = xDataTable.NewRow();
                xNewRow["Code"] = strRunningCode;
                xDataTable.Rows.Add(xNewRow);
            }
            xDataTable.Rows[0]["Runing"] = iRunningNo1;
            xDataTable.Rows[0]["Runing2"] = iRunningNo2;
            xDataTable.Rows[0]["LastRuningDateTime"] = vGenDate;

            strError = string.Empty;
            //if (!db.UpdateDataTable(xDataTable.Rows[0], out strError)) return false;
            if (!condb.Transaction_UpdateDataTable(sqlConnectionMaster, transMaster, xDataTable, out strError)) return false;

            if ((int)xDataTable.Rows[0]["Runing"] > 0)
            {

                //ConnectDB condb = new ConnectDB(strConnectionMaster);
                dbCondition = DBExpression.Normal("Code", DBComparisonOperator.EqualEver, strRunningCode);
                ///
                xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
                xGetDataOptionParam.TableName = "SETUP_RUNNING_REF";
                xGetDataOptionParam.Condition = dbCondition.ToString();
                xGetDataOptionParam.ScanFillUp = true;


                xGetDataOptionParam.ColumnsSelectedColl = new System.Collections.Specialized.StringCollection();
                xGetDataOptionParam.ColumnsSelectedColl.Add("Code");
                xGetDataOptionParam.ColumnsSelectedColl.Add("Runing");
                xGetDataOptionParam.ColumnsSelectedColl.Add("Runing2");

                xDataTable = condb.GetDataTable(xGetDataOptionParam, out strError);

                if (xDataTable.Rows.Count > 0)
                {
                    xDataTable.Rows[0]["Runing"] = 0;
                    xDataTable.Rows[0]["Runing2"] = 0;

                    if (!condb.Transaction_UpdateDataTable(sqlConnectionMaster, transMaster, xDataTable, out strError)) return false;
                }

            }
            return true;
        }

        public static DataTable GetRunningRefMasterData(string strConnectionMaster, string strRunningCode, out string strError)
        {
            /*
             *  [Code]
      ,[LocalName]
      ,[EngName]
      ,[RuningFormat]
      ,[Runing]
      ,[ResetAtRuning]
      ,[RunningRefResetType]
      ,[RunningRefType]
      ,[RunningWriteLog]
      ,[RunningCheckDigitType]
      ,[Runing2]
      ,[StartAtRuning]
             */

            ConnectDB condb = new ConnectDB(strConnectionMaster);
            DBCondition dbCondition = DBExpression.Normal("Code", DBComparisonOperator.EqualEver, strRunningCode);                

            ConnectDB.GetDataOptionParam xGetDataOptionParam = new ConnectDB.GetDataOptionParam();
            xGetDataOptionParam.TableName = "SETUP_RUNNING_REF";
            xGetDataOptionParam.Condition = dbCondition.ToString();
            xGetDataOptionParam.ScanFillUp = true;

            DataTable xDataTableRunRef = condb.GetDataTable(xGetDataOptionParam, out strError);
            return xDataTableRunRef;
        }

        //public static bool GenRefDev(string strFormat, int iRunningNo1, byte bRunningCheckDigitType, out string strRefNo)
        //{
        //    int iRunningNo2 = 0;
        //    return GenRefDev(strFormat, iRunningNo1, iRunningNo2, bRunningCheckDigitType, out strRefNo);
        //}
        //public static bool GenRefDev(string strFormat, int iRunningNo1, int iRunningNo2, byte bRunningCheckDigitType, out string strRefNo)
        //{
        //    DateTime vGenDate = DxDate.DateTimeNow_HHMMSSMS();

        //    return GenRefDev(strFormat, iRunningNo1, iRunningNo2, bRunningCheckDigitType, vGenDate, out strRefNo);
        //}
        //public static bool GenRefDev(string strFormat, int iRunningNo1, int iRunningNo2, byte bRunningCheckDigitType, DateTime vGenDate, out string strRefNo)
        //{
        //    strRefNo = strFormat;
        //    string strError = string.Empty;
        //    string strTemp, strSearch;
        //    if (DxDate.DateTimeIsEmpty(vGenDate))
        //    {
        //        vGenDate = DxDate.DateTimeNow_HHMMSSMS();
        //    }
        //    int iYear = -1;
        //    int iMonth = -1;

        //    iMonth = vGenDate.Month;
        //    iYear = vGenDate.Year;
        //    //bool bOk = false;

        //    //iRunningNo++;

        //    iMonth = vGenDate.Month;
        //    iYear = vGenDate.Year;

        //    strSearch = "[D]";
        //    if (strRefNo.Contains(strSearch))
        //    {
        //        strTemp = string.Format("{0}", vGenDate.Day);
        //        strRefNo = strRefNo.Replace(strSearch, strTemp);
        //    }
        //    else
        //    {
        //        strSearch = "[DD]";
        //        if (strRefNo.Contains(strSearch))
        //        {
        //            strTemp = string.Format("{0:D2}", vGenDate.Day);
        //            strRefNo = strRefNo.Replace(strSearch, strTemp);
        //        }
        //    }

        //    strSearch = "[DOY]";
        //    if (strRefNo.Contains(strSearch))
        //    {
        //        strTemp = string.Format("{0}", vGenDate.DayOfYear);
        //        strRefNo = strRefNo.Replace(strSearch, strTemp);
        //    }
        //    else
        //    {
        //        strSearch = "[DDOY]";
        //        if (strRefNo.Contains(strSearch))
        //        {
        //            strTemp = string.Format("{0:D3}", vGenDate.DayOfYear);
        //            strRefNo = strRefNo.Replace(strSearch, strTemp);
        //        }
        //    }

        //    strSearch = "[M]";
        //    if (strRefNo.Contains(strSearch))
        //    {
        //        strTemp = string.Format("{0}", iMonth);
        //        strRefNo = strRefNo.Replace(strSearch, strTemp);
        //    }
        //    else
        //    {
        //        strSearch = "[MM]";
        //        if (strRefNo.Contains(strSearch))
        //        {
        //            strTemp = string.Format("{0:D2}", iMonth);
        //            strRefNo = strRefNo.Replace(strSearch, strTemp);
        //        }
        //    }

        //    strSearch = "[Y]";
        //    if (strRefNo.Contains(strSearch))
        //    {
        //        strTemp = string.Format("{0:D4}", iYear);
        //        strTemp = strTemp.Remove(0, 2);
        //        strRefNo = strRefNo.Replace(strSearch, strTemp);
        //    }

        //    strSearch = "[YY]";
        //    if (strRefNo.Contains(strSearch))
        //    {
        //        strTemp = string.Format("{0:D4}", iYear);
        //        strTemp = strTemp.Remove(0, 2);
        //        strRefNo = strRefNo.Replace(strSearch, strTemp);
        //    }
        //    else
        //    {
        //        strSearch = "[YYYY]";
        //        if (strRefNo.Contains(strSearch))
        //        {
        //            strTemp = string.Format("{0:D4}", iYear);
        //            strRefNo = strRefNo.Replace(strSearch, strTemp);
        //        }
        //    }

        //    strSearch = "[T]";
        //    if (strRefNo.Contains(strSearch))
        //    {
        //        strTemp = string.Format("{0:D4}", (iYear + 543));
        //        strTemp = strTemp.Remove(0, 2);
        //        strRefNo = strRefNo.Replace(strSearch, strTemp);
        //    }

        //    strSearch = "[TT]";
        //    if (strRefNo.Contains(strSearch))
        //    {
        //        strTemp = string.Format("{0:D4}", (iYear + 543));
        //        strTemp = strTemp.Remove(0, 2);
        //        strRefNo = strRefNo.Replace(strSearch, strTemp);
        //    }
        //    else
        //    {
        //        strSearch = "[TTTT]";
        //        if (strRefNo.Contains(strSearch))
        //        {
        //            strTemp = string.Format("{0:D4}", (iYear + 543));
        //            strRefNo = strRefNo.Replace(strSearch, strTemp);
        //        }
        //    }

        //    string strMainSearch = string.Empty;
        //    int i = 0;

        //    for (int iMain = 1; iMain <= 2; iMain++)
        //    {
        //        strMainSearch = string.Empty;
        //        if (1 == iMain) strMainSearch = "[A";
        //        else if (2 == iMain) strMainSearch = "[B";

        //        if (-1 == strRefNo.IndexOf(strMainSearch))
        //            continue;

        //        for (int iLoop = 1; iLoop < 10; iLoop++)
        //        {
        //            strSearch = strMainSearch;
        //            strTemp = string.Format("{0}", iLoop);
        //            strSearch += strTemp;
        //            strSearch += "]";
        //            i = strRefNo.IndexOf(strSearch);
        //            if (-1 != i)
        //            {
        //                strTemp = string.Empty;
        //                if (1 == iMain) strTemp = string.Format("{0:D9}", iRunningNo1);
        //                else if (2 == iMain) strTemp = string.Format("{0:D9}", iRunningNo2);

        //                while (strTemp.Length > iLoop)
        //                {
        //                    if (!strTemp.StartsWith("0")) break;
        //                    strTemp = strTemp.Remove(0, 1);
        //                }
        //                strRefNo = strRefNo.Replace(strSearch, strTemp);
        //            }
        //        }
        //    }
 

        //    return true;
        //}

        #endregion 


    }

    public enum RunningRefResetType : byte
    {
        None = 0,
        Daily = 1,
        Monthly = 2,
        Yearly = 3
    }

}