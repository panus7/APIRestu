using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Web.Script.Serialization;
using System.Collections;
using System.ComponentModel;
using System.Xml; 
 
using System.Drawing;
using Newtonsoft.Json;

namespace APIRestServiceRestaurant
{
    public class DxData {

        public static string getValueString(object objRowValue)
        {
            if (DBNull.Value == objRowValue)
                return string.Empty;

            if (null == objRowValue)
                return string.Empty;

            return objRowValue.ToString().Trim();
        }

        public static int getValueInt(object objRowValue)
        {
            if (DBNull.Value == objRowValue)
                return (int)0;

            if (null == objRowValue)
                return (int)0;

            return (int)objRowValue;
        }

        public static byte getValueByte(object objRowValue)
        {
            if (DBNull.Value == objRowValue)
                return (byte)0;

            if (null == objRowValue)
                return (byte)0;

            return (byte)objRowValue;
        }

        public static byte[] getValueByteArray(object objRowValue)
        {
            var emptyByteArray = new List<byte>();

            if (DBNull.Value == objRowValue)
                return emptyByteArray.ToArray();

            if (null == objRowValue)
                return emptyByteArray.ToArray();

            return (byte[])objRowValue;
        }

        public static string getValueByteArrayImageToBase64(object objRowValue)
        {
            var emptyByteArray = "";

            if (DBNull.Value == objRowValue)
                return emptyByteArray;

            if (null == objRowValue)
                return emptyByteArray;

            return DxImage.ConvertByteArrayImageToBase64((byte[])objRowValue);
        }

        public static string getValueByteArrayToBase64(object objRowValue)
        {
            var emptyByteArray = "";

            if (DBNull.Value == objRowValue)
                return emptyByteArray;

            if (null == objRowValue)
                return emptyByteArray;

            return DxImage.ConvertByteArrayToBase64((byte[])objRowValue);
        }

        public static DateTime getValueDateTime(object objRowValue)
        {
            DateTime dt = new DateTime();
            if (DBNull.Value == objRowValue)
                return dt;

            if (null == objRowValue)
                return dt;

            return (DateTime)objRowValue;
        }

        public static string getValueDateTimeToString(object objRowValue)
        {
            if (DBNull.Value == objRowValue)
                return string.Empty;

            if (null == objRowValue)
                return string.Empty;

            try
            {
                if (DateTime.MinValue == (DateTime)objRowValue)
                {
                    return string.Empty;
                }

                return DxConvert.GenSortableDateTimeHHMMSSMSToString((DateTime)objRowValue);
            }
            catch (Exception e)
            { }

            return string.Empty;
        }

        public static string getValueDateTimeHHMMSSToString(object objRowValue)
        {
            if (DBNull.Value == objRowValue)
                return string.Empty;

            if (null == objRowValue)
                return string.Empty;

            try
            {
                if (DateTime.MinValue == (DateTime)objRowValue)
                {
                    return string.Empty;
                }

                return DxConvert.GenSortableDateTimeHHMMSSToString((DateTime)objRowValue);
            }
            catch (Exception e)
            { }

            return string.Empty;
        }

    }

    public class DxConvert
    {
        static public bool ConvertDataTableToTextCsvWithWriteFile(DataTable dt, string strFullPathFileName)
        {
            string strResult = ConvertDataTableToTextCsv(dt);
            if (string.Empty != strResult && string.Empty != strFullPathFileName)
            {
                File.WriteAllText(strFullPathFileName, strResult);
                return false;
            }

            return false;
        }

        static public string ConvertDataTableToTextCsv(DataTable dt)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName);
                sb.AppendLine(string.Join(",", columnNames));

                foreach (DataRow row in dt.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                    sb.AppendLine(string.Join(",", fields));
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            //return string.Empty;
            // File.WriteAllText("test.csv", sb.ToString());
        }
        

        public static byte ConvertStringToByte(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return (byte)0;
            }
            try
            {
                return byte.Parse(s);
            }
            catch
            { }
            return (byte)0;
        }

        public static int ConvertStringToInt(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return (int)0;
            }
            try
            {
                return int.Parse(s);
            }
            catch
            { }
            return (int)0;
        }

        public static double ConvertStringToDouble(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return (double)0.0;
            }
            try
            {
                return double.Parse(s);
            }
            catch
            { }
            return (double)0.0;
        }

        public static float ConvertStringToFloat(string s)
        {
            float f = 0;
            float.TryParse(s, out f);
            return f;
        }

        
        public static StringCollection ConvertListStringToStringCollection(List<String> ListOfString)
        {
            StringCollection collection = new StringCollection();            
            foreach (string item in ListOfString)
            {
                if (!collection.Contains(item) && string.Empty != item)
                {
                    collection.Add(item);
                }
            }
            return collection;
        }

        public static List<string> ConvertStringCollectionToListString(StringCollection collection)
        {
            List<string> list = new List<string>();
            foreach (string item in collection)
            {
                if (!list.Contains(item) && string.Empty != item)
                {
                    list.Add(item);
                }
            }
            return list;
            
        }

        public static byte[] ConvertStringToByteArray(string str)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                return enc.GetBytes(str);
            }
            catch
            { }
            return new byte[0];
        }

        public static string ConverStringDateTimeEntry(DateTime dtEntry)
        {
            if (DxDate.DateTimeIsEmpty(dtEntry)) return string.Empty;

            //31122550.123099
            int iDay = dtEntry.Day;
            int iMonth = dtEntry.Month;
            int iYear = dtEntry.Year;
            if (iYear < 2200)
            {
                iYear = iYear + 543;
            }

            return string.Format("{0:D2}{1:D2}{2:D4}.{3:D2}{4:D2}{5:D2}", iDay, iMonth, iYear, dtEntry.Hour, dtEntry.Minute, dtEntry.Second);
        }

        public static TimeSpan ConvertIntToTimeSpan(int iTime)
        {
            TimeSpan ts = new TimeSpan();

            if (iTime > 0)
            {
                int iHour = iTime / 1000000;
                int iMinute = iTime % 1000000;
                if (iMinute > 0)
                    iMinute = iMinute / 10000;
                int iSec = iTime % 10000;
                if (iSec > 0)
                    iSec = iSec / 100;

                ts = new TimeSpan(0, iHour, iMinute, iSec, 0);
            }

            
            //string strTime = iTime.ToString();

            //string strHr = string.Empty;
            //string strMin = string.Empty;
            //string strSec = string.Empty;
            //string strMSec = string.Empty;

            ////08.30 = 83000000, 10.00 = 100000000
            //if (strTime.Length == (int)8)
            //{
            //    //8 30 00 000
            //    strHr = strTime.Substring(0,1);
            //    strMin = strTime.Substring(1,2);
            //    strSec = strTime.Substring(3,2);
            //    strMSec = strTime.Substring(5,3);
            //    ///
            //    //string str = string.Format("{0}{1}{2}{3}", strHr, strMin, strSec, strMSec);                
            //}
            //else if (strTime.Length == (int)9)
            //{
            //    //10 00 00 000
            //    strHr = strTime.Substring(0, 2);
            //    strMin = strTime.Substring(2, 2);
            //    strSec = strTime.Substring(4, 2);
            //    strMSec = strTime.Substring(6, 3);
            //    ///
            //    //string str = string.Format("{0}{1}{2}{3}", strHr, strMin, strSec, strMSec);

            //}

            //try
            //{
            //    ts = new TimeSpan(0, int.Parse(strHr), int.Parse(strMin), int.Parse(strSec), int.Parse(strMSec));
            //}
            //catch (Exception ex)
            //{ }

            return ts;
        }

        public static int ConvertTimeSpanToInt(TimeSpan timespan)
        {
            int iTime = (int)0;

            int iHour = 0;
            int iMinute = 0;
            int iSeconds = 0;
            if (timespan.Hours > 0)
                iHour = timespan.Hours * 1000000;
            if (timespan.Minutes > 0)
                iMinute = timespan.Minutes * 10000;
            if (timespan.Seconds > 0)
                iSeconds = timespan.Seconds * 100;

            iTime = iHour + iMinute + iSeconds;

            //string strHr = timespan.Hours.ToString();
            //int iMin = int.Parse(timespan.Minutes.ToString());
            //int iSec = int.Parse(timespan.Seconds.ToString());
            //int iMSec = int.Parse(timespan.Milliseconds.ToString());
            //string str = string.Format("{0}{1:D2}{2:D2}{3:D2}", strHr, iMin, iSec, iMSec);

            //try
            //{
            //    iTime = int.Parse(str);
            //}
            //catch (Exception ex)
            //{ }

            return iTime;
        }

        public static DateTime ConvertIntToDateTime(DateTime xDate, int iTime)
        {
            DateTime dtDateTime = DxDate.DateTimeEmpty();

            if (iTime > 0)
            {
                int iHour = iTime / 1000000;
                if (iHour > 24)
                {
                    iTime = iTime / 10;
                    iHour = iTime / 1000000;
                }
                int iMinute = iTime % 1000000;
                if (iMinute > 0)
                    iMinute = iMinute / 10000;

                dtDateTime = new DateTime(xDate.Year, xDate.Month, xDate.Day, iHour, iMinute, 0);
            }

            return dtDateTime;
        }

        public static int ConvertDateTimeToInt(DateTime xDateTime)
        {
            int iTime = (int)0;

            if (false == DxDate.TimeIsEmpty(xDateTime))
            {
                int iHour = 0;
                int iMinute = 0;
                if (xDateTime.Hour > 0)
                    iHour = xDateTime.Hour * 1000000;
                if (xDateTime.Minute > 0)
                    iMinute = xDateTime.Minute * 10000;
                iTime = iHour + iMinute;
            }

            return iTime;
        }

        public static string GenSortableDateTimeHHMMSSToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second);
        }

        public static string GenSortableDateTimeHHMMSSMSToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}:{6:D3}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second, xDateTime.Millisecond);
        }

        public static string ConvertDateYYYYMMDDToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D4}{1:D2}{2:D2}", xDateTime.Year, xDateTime.Month, xDateTime.Day);
        }

        public static string ConvertDateYYYYMMDDHHMMSSMSToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}{6:D3}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second, xDateTime.Millisecond);
        }

        public static string ConvertDateDDMMYYYYToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D2}{1:D2}{2:D4}", xDateTime.Day, xDateTime.Month, xDateTime.Year);
        }

        public static string GenDateTimeHHMMSSToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second);
        }

        public static string GenDateTimeHHMMToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute);
        }
        
        public static DateTime ConvertStringEntryToDateTime(string strEntry)
        {
            bool bOK = true;
            DateTime xDateTimeEmpty = new DateTime(0001, 1, 1);
            //if (15 != strEntry.Length) bOK = false;
            //if (8 != strEntry.IndexOf('.')) bOK = false;
            if (!strEntry.Contains(".")) bOK = false;
            if (!bOK) return xDateTimeEmpty;

            //31122550.123099

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            int iYear = (int)0;
            int iMonth = (int)0;
            int iDay = (int)0;
            int iHour = (int)0;
            int iMint = (int)0;
            int iSeco = (int)0;

            string strDate = string.Empty;
            string strTime = string.Empty;

            DateTime dtEntry = new DateTime();
            if (strEntry.Contains(".")) //%%%%
            {
                try
                {
                    string[] strTemp = strEntry.Split('.');
                    strDate = strTemp[0];
                    strTime = strTemp[1];

                    if (strDate.Length == 1)
                    {
                        //iYear = DateTime.Today.Year;
                        //iYear = 2500 + iYear;
                        //iMonth = DateTime.Today.Month;
                        //iDay = DateTime.Today.Day;

                        string strTime4digi = string.Format("{0:4D}", strTime);
                        string iHH = strTime4digi.Substring(0, 2);
                        string iMM = strTime4digi.Substring(2, 2);
                        dtEntry = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, int.Parse(iHH), int.Parse(iMM), 0);
                        dtEntry = dtEntry.AddDays(double.Parse(strDate));
                    }
                    else
                    {
                        if (strDate.Length == 6)
                        {
                            iYear = int.Parse(strDate.Substring(4, 2));
                            iYear = 2500 + iYear;
                            iMonth = int.Parse(strDate.Substring(2, 2));
                            iDay = int.Parse(strDate.Substring(0, 2));
                            if (iMonth > 12) return xDateTimeEmpty;
                        }
                        else if (strDate.Length == 8)
                        {
                            iYear = int.Parse(strDate.Substring(4, 4));
                            iMonth = int.Parse(strDate.Substring(2, 2));
                            iDay = int.Parse(strDate.Substring(0, 2));
                            if (iMonth > 12) return xDateTimeEmpty;
                        }

                        if (strTime.Length == 4)
                        {
                            iHour = int.Parse(strTime.Substring(0, 2));
                            iMint = int.Parse(strTime.Substring(2, 2));
                            iSeco = 0;
                        }
                        else if (strTime.Length == 4)
                        {
                            iHour = int.Parse(strTime.Substring(0, 2));
                            iMint = int.Parse(strTime.Substring(2, 2));
                            iSeco = 0;
                        }
                        else if (strTime.Length == 6)
                        {
                            iHour = int.Parse(strTime.Substring(0, 2));
                            iMint = int.Parse(strTime.Substring(2, 2));
                            iSeco = int.Parse(strTime.Substring(4, 2));
                        }
                        if (iYear > 2200) iYear = iYear - 543;
                        dtEntry = new DateTime(iYear, iMonth, iDay, iHour, iMint, iSeco);
                    }


                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);

                }
            }

            return dtEntry;
        }

        public static DateTime ConvertStringYYYYMMDDToDate(string strDate)
        {
            //$$$$pt start
            DateTime xDateTimeEmpty = DxDate.DateTimeEmpty();

            bool bOK = true;
            if (string.IsNullOrEmpty(strDate)) bOK = false;

            if (!bOK) return xDateTimeEmpty;
            //$$$$pt end
            int iYear = (int)0;
            int iMonth = (int)0;
            int iDay = (int)0;
            int iHour = (int)0;
            int iMint = (int)0;
            int iSeco = (int)0;

            DateTime dtEntry = new DateTime();
            try
            {
                if (strDate.Length == 6)
                {
                    iYear = int.Parse(strDate.Substring(0, 2));
                    iYear = 2500 + iYear;
                    iMonth = int.Parse(strDate.Substring(2, 2));
                    iDay = int.Parse(strDate.Substring(4, 2));
                    if (iMonth > 12) return xDateTimeEmpty;
                }
                else if (strDate.Length == 8)
                {
                    iYear = int.Parse(strDate.Substring(0, 4));
                    iMonth = int.Parse(strDate.Substring(4, 2));
                    iDay = int.Parse(strDate.Substring(6, 2));
                    if (iMonth > 12) return xDateTimeEmpty;
                }
                if (iYear > 2200) iYear = iYear - 543;
                dtEntry = new DateTime(iYear, iMonth, iDay, iHour, iMint, iSeco);
            }
            catch (Exception)
            {
            }
            return dtEntry;
        }

        public static DateTime ConvertStringHHMMSSToTime(string strEntry)
        {
            DateTime dtEntry = DxDate.DateTimeEmpty();
            bool bOK = true;
            if (string.IsNullOrEmpty(strEntry)) bOK = false;

            if (!bOK) return dtEntry;

            //123099
            System.IFormatProvider format = new System.Globalization.CultureInfo("en-US");

            DateTime xToday = DateTime.Today;
            int iYear = xToday.Year;
            int iMonth = xToday.Month;
            int iDay = xToday.Day;
            int iHour = (int)0;
            int iMint = (int)0;
            int iSeco = (int)0;
            int iMsec = (int)0;

            try
            {
                if (strEntry.Length == 4)
                {
                    iHour = int.Parse(strEntry.Substring(0, 2));
                    iMint = int.Parse(strEntry.Substring(2, 2));
                    iSeco = 0;
                }
                else if (strEntry.Length == 6)
                {
                    iHour = int.Parse(strEntry.Substring(0, 2));
                    iMint = int.Parse(strEntry.Substring(2, 2));
                    iSeco = int.Parse(strEntry.Substring(4, 2));
                }
                else if (strEntry.Length == 9)
                {
                    iHour = int.Parse(strEntry.Substring(0, 2));
                    iMint = int.Parse(strEntry.Substring(2, 2));
                    iSeco = int.Parse(strEntry.Substring(4, 2));
                    iMsec = int.Parse(strEntry.Substring(6, 3));
                }
                dtEntry = new DateTime(iYear, iMonth, iDay, iHour, iMint, iSeco, iMsec);
            }
            catch (Exception)
            {

            }

            return dtEntry;
        }

        public static DateTime ConvertStringYYYYMMDDHHMNSSToDateTime(string strDateTime)
        {
            DateTime dtEntry = DxDate.DateTimeEmpty();
            bool bOK = true;
            if (string.IsNullOrEmpty(strDateTime)) bOK = false;

            if (!bOK) return dtEntry;

            string strDate = string.Empty;
            string strTime = string.Empty;

            if (strDateTime.Length == 14)
            {
                strDate = strDateTime.Substring(0, 8);
                strTime = strDateTime.Substring(8, 6);
            }
            else if (strDateTime.Length == 12)
            {
                strDate = strDateTime.Substring(0, 8);
                strTime = strDateTime.Substring(8, 4);
            }

            DateTime dDate = ConvertStringYYYYMMDDToDate(strDate);
            DateTime dTime = ConvertStringHHMMSSToTime(strTime);

            dtEntry = new DateTime(dDate.Year, dDate.Month, dDate.Day, dTime.Hour, dTime.Minute, dTime.Second);

            return dtEntry;
        }

        public static DateTime ConvertStringDateParamToDateTime(string strDateTime)
        {
            if (string.IsNullOrEmpty(strDateTime))
                return new DateTime();

            //2021-04-20T00:00:00:000
            if (strDateTime.Length == 10)
            {
                strDateTime = strDateTime + "T00:00:00:000";
            }

            if (strDateTime.Length == 19)
            {
                return ConvertStringYYYYMMDDHHMNSS_STDToDateTime(strDateTime);
            }
            else if (strDateTime.Length == 23)
            {
                return ConvertStringYYYYMMDDHHMNSSMS_STDToDateTime(strDateTime);
            }

            return new DateTime();
        }

        public static DateTime ConvertStringYYYYMMDDHHMNSS_STDToDateTime(string strDateTime)
        {
            //"2008-03-09T16:05:07"             SortableDateTime
            DateTime dtEntry = DxDate.DateTimeEmpty();
            bool bOK = true;
            if (string.IsNullOrEmpty(strDateTime))
            {
                bOK = false;
            }
            else
            {
                if (!strDateTime.Contains("T")) bOK = false;
            }
            if (!bOK) return dtEntry;

            string strDate = string.Empty;
            string strTime = string.Empty;
            string strTemp;

            string[] arrTemp = strDateTime.Split('T');
            string strDateFull = arrTemp[0];
            string strTimeFull = arrTemp[1];

            arrTemp = strDateFull.Split('-');
            strDate = arrTemp[0];

            strTemp = arrTemp[1];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strDate += strTemp;

            strTemp = arrTemp[2];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strDate += strTemp;

            arrTemp = strTimeFull.Split(':');

            strTemp = arrTemp[0];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strTime = strTemp;

            strTemp = arrTemp[1];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strTime += strTemp;

            strTemp = arrTemp[2];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strTime += strTemp;

            DateTime dDate = ConvertStringYYYYMMDDToDate(strDate);
            DateTime dTime = ConvertStringHHMMSSToTime(strTime);

            dtEntry = new DateTime(dDate.Year, dDate.Month, dDate.Day, dTime.Hour, dTime.Minute, dTime.Second);

            return dtEntry;
        }

        public static DateTime ConvertStringYYYYMMDDHHMNSSMS_STDToDateTime(string strDateTime)
        {
            //"2008-03-09T16:05:07"             SortableDateTime
            DateTime dtEntry = DxDate.DateTimeEmpty();
            bool bOK = true;
            if (string.IsNullOrEmpty(strDateTime))
            {
                bOK = false;
            }
            else
            {
                if (!strDateTime.Contains("T")) bOK = false;
            }
            if (!bOK) return dtEntry;

            string strDate = string.Empty;
            string strTime = string.Empty;
            string strTemp;

            string[] arrTemp = strDateTime.Split('T');
            string strDateFull = arrTemp[0];
            string strTimeFull = arrTemp[1];

            arrTemp = strDateFull.Split('-');
            strDate = arrTemp[0];

            strTemp = arrTemp[1];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strDate += strTemp;

            strTemp = arrTemp[2];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strDate += strTemp;

            arrTemp = strTimeFull.Split(':');

            strTemp = arrTemp[0];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strTime = strTemp;

            strTemp = arrTemp[1];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strTime += strTemp;

            strTemp = arrTemp[2];
            if (strTemp.Length == 1)
                strTemp = "0" + strTemp;
            strTime += strTemp;
            
            //strTime = arrTemp[0];
            //strTime += arrTemp[1];
            //strTime += arrTemp[2];
            strTime += arrTemp[3];

            DateTime dDate = ConvertStringYYYYMMDDToDate(strDate);
            DateTime dTime = ConvertStringHHMMSSToTime(strTime);

            dtEntry = new DateTime(dDate.Year, dDate.Month, dDate.Day, dTime.Hour, dTime.Minute, dTime.Second, dTime.Millisecond);

            return dtEntry;
        }

        public static DateTime ConvertStringYYYYMMDD_STDToDate(string strDateTime)
        {
            //"2008-03-09"             SortableDateTime
            DateTime dtEntry = DxDate.DateTimeEmpty();
            bool bOK = true;
            if (string.IsNullOrEmpty(strDateTime))
            {
                bOK = false;
            }
            else
            {
                if (!strDateTime.Contains("-") &&
                    !strDateTime.Contains("/")) bOK = false;
            }
            if (!bOK) return dtEntry;

            string strDate = string.Empty;
            string[] arrTemp;
            string strTemp;

            if (strDateTime.Contains("-"))
            {
                arrTemp = strDateTime.Split('-');
                if (arrTemp.Count() == 3)
                {
                    strDate = arrTemp[0];

                    strTemp = arrTemp[1];
                    if (strTemp.Length == 1)
                        strTemp = "0" + strTemp;
                    strDate += strTemp;

                    strTemp = arrTemp[2];
                    if (strTemp.Length == 1)
                        strTemp = "0" + strTemp;
                    strDate += strTemp;
                }
            }
            else if (strDateTime.Contains("/"))
            {
                arrTemp = strDateTime.Split('/');
                if (arrTemp.Count() == 3)
                {
                    strDate = arrTemp[0];

                    strTemp = arrTemp[1];
                    if (strTemp.Length == 1)
                        strTemp = "0" + strTemp;
                    strDate += strTemp;

                    strTemp = arrTemp[2];
                    if (strTemp.Length == 1)
                        strTemp = "0" + strTemp;
                    strDate += strTemp;
                }
            }

            DateTime dDate = ConvertStringYYYYMMDDToDate(strDate);

            dtEntry = new DateTime(dDate.Year, dDate.Month, dDate.Day);

            return dtEntry;
        }

        public static DateTime ConvertStringYYMMDDHHMNSSToDateTime(string strDateTime)
        {
            DateTime dtEntry = DxDate.DateTimeEmpty();
            bool bOK = true;
            if (string.IsNullOrEmpty(strDateTime)) bOK = false;

            if (!bOK) return dtEntry;

            string strDate = string.Empty;
            string strTime = string.Empty;

            if (strDateTime.Length == 12)
            {
                strDate = strDateTime.Substring(0, 6);
                strTime = strDateTime.Substring(8, 6);
            }
            else if (strDateTime.Length == 10)
            {
                strDate = strDateTime.Substring(0, 6);
                strTime = strDateTime.Substring(8, 4);
            }

            DateTime dDate = ConvertStringYYYYMMDDToDate(strDate);
            DateTime dTime = ConvertStringHHMMSSToTime(strTime);

            dtEntry = new DateTime(dDate.Year, dDate.Month, dDate.Day, dTime.Hour, dTime.Minute, dTime.Second);

            return dtEntry;
        }

        public string ConverAmtStringDateTimeEntry(object xObject)
        {
            string strReturn = string.Empty;
            if (xObject is double)
            {
                double dbAmt = (double)xObject;
                if (dbAmt > 0.0)
                {
                    strReturn = dbAmt.ToString("###,###,###.##");
                }
                else if (dbAmt < 0.0)
                {
                    dbAmt = 0 - dbAmt;
                    strReturn = dbAmt.ToString("###,###,###.##");
                    strReturn = string.Format("({0})", strReturn);
                }
            }

            return strReturn;
        }

        public static byte[] strToByteArray(string str)
        {
            return ConvertStringToByteArray(str);
            //System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            //return enc.GetBytes(str);
        }

        public static string ConvertStreamToBase64(Stream input)
        {
            //byte[] buffer = ConvertStreamToByteArray(input);
            //if (buffer.Length > 0)
            //{
            //    return Convert.ToBase64String(buffer);
            //}
            try
            {
                return Convert.ToBase64String(ReadToEnd(input));
            }
            catch (Exception ex)
            { }

            return string.Empty;
        }

        private static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public static Dictionary<Byte, String> ConvertEnumByteToDictionary<T>()
        {
            Dictionary<Byte, String> result = new Dictionary<Byte, String>();
            foreach(var enumt in System.Enum.GetValues(typeof(T)))
            {
                result.Add((byte)enumt, enumt.ToString());
            }

            return result;
        }

        public static Dictionary<Int32, String> ConvertEnumIntToDictionary<T>()
        {
            Dictionary<Int32, String> result = new Dictionary<Int32, String>();
            foreach (var enumt in System.Enum.GetValues(typeof(T)))
            {
                result.Add((int)enumt, enumt.ToString());
            }

            return result;
        }
         


        public static string ConvertDoubleToString(double dNumber)
        {
            return ProcessChangeNumberToWord(dNumber.ToString());
        }

        private static string ProcessChangeNumberToWord(string numberVar1)
        {
            char cha1;
            string ProcessValue;

            string[] NumberWord;
            string[] NumberWord2;
            string Num3 = "";
            cha1 = '.';
            NumberWord = numberVar1.Split(cha1);
            cha1 = ',';
            NumberWord2 = NumberWord[0].Split(cha1);
            for (int i = 0; i <= NumberWord2.Length - 1; i++)
            {
                Num3 = Num3 + NumberWord2[i];
            }
            ProcessValue = SplitWord(Num3);
            if (NumberWord.Length > 1)
            {
                if (int.Parse(NumberWord[1]) > 0)
                {
                    if ((int)NumberWord[1].Length == (int)1)
                    {
                        NumberWord[1] = NumberWord[1] + (string)"0";
                    }
                    ProcessValue = ProcessValue + "บาท" + SplitWord(NumberWord[1]) + "สตางค์";
                }
                else
                {
                    ProcessValue = ProcessValue + "บาทถ้วน";
                }
            }
            else
            {
                ProcessValue = ProcessValue + "บาทถ้วน";
            }
            return ProcessValue;
        }

        private static string SplitWord(string numberVar)
        {
            int i = numberVar.Length;
            int k = 0;
            int n = i;
            int m = i;
            int b = 6;
            //char value2;
            char[] value1;
            string CurrencyWord = "";
            value1 = numberVar.ToCharArray();
            for (int a = 0; a <= i; a = a + 7)
            {
                if (n <= a + 7 && n > 0)
                {
                    b = n - 1;
                    if (i > 7)
                    {
                        k = 1;
                    }
                }
                else
                {
                    b = 6;
                }
                if (n > 0)
                {
                    for (int j = 0; j <= b; j++)
                    {
                        n--;
                        k++;
                        CurrencyWord = GetWord(value1[n].ToString(), k) + CurrencyWord;
                    }
                }
            }
            return CurrencyWord;
        }

        private static string GetWord(string str1, int Num1)
        {
            string value1 = GetCurrency(Num1);
            switch (str1)
            {
                case "1":
                    if (Num1 == 1)
                    {
                        value1 = value1 + "เอ็ด";
                    }
                    else if (Num1 > 2)
                    {
                        value1 = "หนึ่ง" + value1;
                    }
                    break;
                case "2":
                    if (Num1 == 2)
                    {
                        value1 = "ยี่" + value1;
                    }
                    else
                    {
                        value1 = "สอง" + value1;
                    }
                    break;
                case "3":
                    value1 = "สาม" + value1;
                    break;
                case "4":
                    value1 = "สี่" + value1;
                    break;
                case "5":
                    value1 = "ห้า" + value1;
                    break;
                case "6":
                    value1 = "หก" + value1;
                    break;
                case "7":
                    value1 = "เจ็ด" + value1;
                    break;
                case "8":
                    value1 = "แปด" + value1;
                    break;
                case "9":
                    value1 = "เก้า" + value1;
                    break;
                default:
                    value1 = "";
                    break;
            }
            return value1;
        }

        private static string GetCurrency(int Num2)
        {
            string value1;
            switch (Num2)
            {
                case 1:
                    value1 = "";
                    break;
                case 2:
                    value1 = "สิบ";
                    break;
                case 3:
                    value1 = "ร้อย";
                    break;
                case 4:
                    value1 = "พัน";
                    break;
                case 5:
                    value1 = "หมื่น";
                    break;
                case 6:
                    value1 = "แสน";
                    break;
                case 7:
                    value1 = "ล้าน";
                    break;
                default:
                    value1 = "";
                    break;
            }
            return value1;
        }

        public static string ConvertDigitToWord(byte byNumber, bool bEnglishView)
        {
            string strOutput = string.Empty;
            switch (byNumber)
            {
                case (byte)1:
                    if (bEnglishView)
                        strOutput = "One";
                    else
                        strOutput = "หนึ่ง";
                    break;
                case (byte)2:
                    if (bEnglishView)
                        strOutput = "Two";
                    else
                        strOutput = "สอง";
                    break;
                case (byte)3:
                    if (bEnglishView)
                        strOutput = "Three";
                    else
                        strOutput = "สาม";
                    break;
                case (byte)4:
                    if (bEnglishView)
                        strOutput = "Four";
                    else
                        strOutput = "สี่";
                    break;
                case (byte)5:
                    if (bEnglishView)
                        strOutput = "Five";
                    else
                        strOutput = "ห้า";
                    break;
                case (byte)6:
                    if (bEnglishView)
                        strOutput = "Six";
                    else
                        strOutput = "หก";
                    break;
                case (byte)7:
                    if (bEnglishView)
                        strOutput = "Seven";
                    else
                        strOutput = "เจ็ด";
                    break;
                case (byte)8:
                    if (bEnglishView)
                        strOutput = "Eight";
                    else
                        strOutput = "แปด";
                    break;
                case (byte)9:
                    if (bEnglishView)
                        strOutput = "Nine";
                    else
                        strOutput = "เก้า";
                    break;
                case (byte)0:
                    if (bEnglishView)
                        strOutput = "Zero";
                    else
                        strOutput = "ศูนย์";
                    break;
                default:
                    strOutput = "";
                    break;
            }
            return strOutput;
        }
          
        public static int ConvertHHMMToTotalMinute(int iHour, int iMinute)
        {
            int iTotalMinute = iMinute;

            if (iHour > 0) iTotalMinute += iHour * (int)60;

            return iTotalMinute;
        }

        public static void ConvertTotalMinuteToHHMM(int iTotalMinute, out int iHour, out int iMinute)
        {
            iHour = 0;
            iMinute = 0;

            if (iTotalMinute < 60)
            {
                iMinute = iTotalMinute;
            }
            else
            {
                while (iTotalMinute >= 60)
                {
                    iHour++;
                    iTotalMinute = iTotalMinute - 60;
                }
            }
            iMinute = iTotalMinute;
        }

        public static string ConvertTextToRTF(string sPlainText)
        {
            return ConvertTextToRTF(sPlainText, new Font("Tahoma", 11));
        }

        public static string ConvertTextToRTF(string sPlainText, Font xFont)
        {
            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Text = sPlainText;
            richTextBox.Font = xFont;
            return richTextBox.Rtf;
        }

        public static DataSet ConvertExcelToDataSet(string excelFilename, bool bGenColumnFormFirstRow)
        {
            var dataSet = new DataSet();
            string connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=No;IMEX=1\";", excelFilename);

            // Create a connection to the excel file
            using (var oleDbConnection = new OleDbConnection(connectionString))
            {
                // Get the excel's sheet names
                oleDbConnection.Open();
                var schemaDataTable = (DataTable)oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                oleDbConnection.Close();
                var sheetsName = GetSheetsName(schemaDataTable);

                // For each sheet name
                OleDbCommand selectCommand = null;
                for (var i = 0; i < sheetsName.Count; i++)
                {
                    // Setup select command
                    selectCommand = new OleDbCommand();
                    selectCommand.CommandText = "SELECT * FROM [" + sheetsName[i] + "]";
                    selectCommand.Connection = oleDbConnection;

                    // Get the data from the sheet
                    oleDbConnection.Open();
                    using (var oleDbDataReader = selectCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        // Convert data to DataTable
                        var dataTable = new DataTable(sheetsName[i].Replace("$", "").Replace("'", ""));
                        dataTable.Load(oleDbDataReader);

                        // Add to Dataset
                        if (bGenColumnFormFirstRow)
                        {
                            DataTable xDataTable = new DataTable(dataTable.TableName);
                            if (null != dataTable)
                            {
                                if (dataTable.Rows.Count != 0)
                                {
                                    int iIndex = 0;
                                    var xItemArray = dataTable.Rows[0].ItemArray;
                                    foreach (object xName in xItemArray)
                                    {
                                        if (dataTable.Columns[iIndex].DataType != typeof(System.String))
                                        {
                                            xDataTable.Columns.Add(string.Format("NoName_{0}", iIndex), dataTable.Columns[iIndex++].DataType);
                                        }
                                        else if (DBNull.Value == xName || string.IsNullOrEmpty((string)xName))
                                        {
                                            xDataTable.Columns.Add(string.Format("NoName_{0}", iIndex), dataTable.Columns[iIndex++].DataType);
                                        }
                                        else
                                        {
                                            xDataTable.Columns.Add((string)xName, dataTable.Columns[iIndex++].DataType);
                                        }
                                    }
                                    iIndex = 0;
                                    foreach (DataRow xRow in dataTable.Rows)
                                    {
                                        if (0 == iIndex++) continue;

                                        DataRow xNewRow = xDataTable.NewRow();
                                        xNewRow.ItemArray = xRow.ItemArray;
                                        xDataTable.Rows.Add(xNewRow);
                                    }
                                }
                            }
                            dataSet.Tables.Add(xDataTable);
                        }
                        else
                        {
                            dataSet.Tables.Add(dataTable);
                        }
                    }
                }

                return dataSet;
            }
        }

        private static List<string> GetSheetsName(DataTable schemaDataTable)
        {
            var sheets = new List<string>();
            foreach (var dataRow in schemaDataTable.AsEnumerable())
            {
                sheets.Add(dataRow.ItemArray[2].ToString());
            }

            return sheets;
        }
 

        //^
        public static string mEncodeCircumpFlex = "[!CircumpFlex!]";
        public static string mDecodeCircumpFlex = "^";

        public static string mEncodeDoubleQuote = "[!DoubleQuote!]";
        public static string mDecodeDoubleQuote = "\"";

        public static string mEncodeSingleQuote = "[!SingleQuote!]";
        public static string mDecodeSingleQuote = "\'";

        public static T ConvertStringJsonToClass<T>(string strJson) where T : new() 
        {
            try
            {
                strJson = strJson.Replace(mEncodeCircumpFlex, mDecodeCircumpFlex); 
                strJson = strJson.Replace(mEncodeDoubleQuote, mDecodeDoubleQuote);
                strJson = strJson.Replace(mEncodeSingleQuote, mDecodeSingleQuote);

                return JsonConvert.DeserializeObject<T>(strJson);
            }
            catch
            { }

            return new T();
        }

        public static DataTable JsonToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
          
        public static DataTable DataTableFromXml(string strTableName, string XMLString)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(XMLString));
            DataTable table = new DataTable(strTableName);
            try
            {

                XmlNode NodoEstructura = xmlDoc.FirstChild.FirstChild;
                //  Table structure (columns definition) 
                foreach (XmlNode columna in NodoEstructura.ChildNodes)
                {
                    table.Columns.Add(columna.Name, typeof(String));
                }

                XmlNode Filas = xmlDoc.FirstChild;
                //  Data Rows 
                foreach (XmlNode Fila in Filas.ChildNodes)
                {
                    List<string> Valores = new List<string>();
                    foreach (XmlNode Columna in Fila.ChildNodes)
                    {
                        Valores.Add(Columna.InnerText);
                    }
                    table.Rows.Add(Valores.ToArray());
                }
            }
            catch (Exception)
            {

            }
            return table;
        }
         
        public static string AddSpaceToString(string strText, int iSize)
        {
            string strReturn = string.Empty;

            if (strText.Length > iSize)
            {
                strReturn = strText.Substring(0, iSize);
            }
            else
            {
                strReturn = strText;

                int iDiff = iSize - strText.Length;
                if (iDiff <= 0) return strReturn;

                for (int count = 0; count < iDiff; count++)
                {
                    strReturn += " ";
                }
            }
            return strReturn;
        }

        public static string AddZeroToString(string strText, int iSize)
        {
            string strReturn = string.Empty;

            if (strText.Length > iSize)
            {
                strReturn = strText;
            }
            else 
            {
                int iDiff = iSize - strText.Length;
                if (iDiff <= 0)
                {
                    strReturn = strText;
                    return strReturn;
                }
                for (int count = 0; count < iDiff; count++)
                {
                    strReturn += "0";
                }
                strReturn += strText;
            }
            return strReturn;
        }

        public static Stream GenerateStreamFromString(string strText)
        {
            return GenerateStreamFromString(strText, Encoding.Default);
        }

        public static Stream GenerateStreamFromString(string strText, Encoding encoding)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream, encoding);
            writer.Write(strText);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static DataTable ClassToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static string OrdinalNumber(int iNumber)     //Ordinal Number=เลขลำดับที่, Cardinal Number=เลขจำนวนนับ
        {
            string strReturn = iNumber.ToString();
            if (iNumber < 1)
            {
                return strReturn;
            }
            iNumber %= 100;
            if (iNumber >= 11 && iNumber <= 13)
            {
                strReturn = strReturn + "th";
            }
            else
            {
                iNumber %= 10;
                switch (iNumber)
                {
                    case 1:
                        strReturn = strReturn + "st";
                        break;
                    case 2:
                        strReturn = strReturn + "nd";
                        break;
                    case 3:
                        strReturn = strReturn + "rd";
                        break;
                    default:
                        strReturn = strReturn + "th";
                        break;
                }
            }
            return strReturn;
        }

    }
}
