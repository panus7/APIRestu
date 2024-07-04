using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace APIRestService
{
    public class DxDate
    { 
        public static bool IsDateTimeEmpty(DateTime DateTimeEntry)
        {
            if (DateTimeEntry == new DateTime()) return true;
            if (DateTimeEntry == new DateTime(0001, 1, 1)) return true;
            return false;
        }

        public static DateTime DateTimeNow_HHMMSSMS()
        {
            DateTime xDateTime = DateTime.Now;

            return new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second, xDateTime.Millisecond);
        }

        public static DateTime DateTimeNow_HHMMSS()
        {
            DateTime xDateTime = DateTime.Now;

            return new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second, 0);
        }

        public static DateTime DateTimeNow_HHMM()
        {
            DateTime xDateTime = DateTime.Now;

            return new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, 0, 0);
        }

        public static DateTime DateNow()
        {
            DateTime xDateTime = DateTime.Now;

            return new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day);
        }

        public static DateTime ClearTime(DateTime xDateTime)
        {
            return new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day);
        }

        public static DateTime ClearTimeMS(DateTime xDateTime)
        {
            return new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second);
        }

        public static DateTime DateTimeEmpty()
        {
            //<@>return new DateTime(0001, 1, 1);

            return DateTime.MinValue; //<@>
        }

        public static DateTime GenDateTimeFromString(string strDate)
        {
            try
            {
                DateTime xDateTime = new DateTime();

                if (string.IsNullOrEmpty(strDate))
                {
                    return xDateTime;
                }
                string strOutput = string.Empty;

                if (strDate.Length == 4) strDate += "0101";
                if (strDate.Length == 6) strDate += "01";
                if (strDate.Length != 8) return new DateTime();

                Int16 iYear = Convert.ToInt16(strDate.Substring(0, 4));
                Int16 iMonth = Convert.ToInt16(strDate.Substring(4, 2));
                Int16 iDay = Convert.ToInt16(strDate.Substring(6, 2));

                if (iMonth == 0) iMonth = 1;
                if (iDay == 0) iDay = 1;

                if (iYear > (Int16)3000) return new DateTime();
                if (iMonth > (Int16)12) return new DateTime();
                if (iDay > (Int16)31) return new DateTime();

                try
                {
                    xDateTime = new DateTime(iYear - 543, iMonth, iDay);
                }
                catch (Exception e)
                {
                    throw new Exception("Error in GenDateTimeFromString : " + e.Message);
                }

                return xDateTime;
            }
            catch (Exception ex)
            {
                try
                {
                    strDate += strDate + " Error : " + ex.ToString();
                    System.IO.File.WriteAllText(@"C:\Error_ReadIDCard.txt", strDate);
                }
                catch
                { }
            }

            return DateTime.MinValue;
        }

        public static bool DateTimeIsEmpty(DateTime xDateTime)
        {
            if (xDateTime.Equals(DateTime.MinValue)) return true; //<@>

            if (null == xDateTime) return true;
            if (1900 == xDateTime.Year && 1 == xDateTime.Month && 1 == xDateTime.Day) return true;
            if (0544 == xDateTime.Year && 1 == xDateTime.Month && 1 == xDateTime.Day) return true;
            if (1 == xDateTime.Year && 1 == xDateTime.Month && 1 == xDateTime.Day) return true;
            return (0001 == xDateTime.Year && 1 == xDateTime.Month && 1 == xDateTime.Day);
        }

        public static bool TimeIsEmpty(DateTime xDateTime)
        {
            if (null == xDateTime) return true;
            if (xDateTime.Equals(DateTime.MinValue)) return true; //<@>
            return (0 == xDateTime.Hour && 0 == xDateTime.Minute && 0 == xDateTime.Second);
        }

        public static DateTime MoveTime(DateTime xDateTimeInput)
        {
            DateTime xDateTimeOutput = xDateTimeInput;
            if (0 == xDateTimeInput.Hour && 0 == xDateTimeInput.Minute && 0 == xDateTimeInput.Second)
            {
                xDateTimeOutput = new DateTime();
            }
            else if (xDateTimeInput.Year == 1)
            {
                xDateTimeOutput = new DateTime(2000, 1, 1, xDateTimeInput.Hour, xDateTimeInput.Minute, xDateTimeInput.Second);
            }
            return xDateTimeOutput;
        }

        public static bool TimeIsInner(DateTime xDateTime, DateTime xDateTimeStart, DateTime xDateTimeEnd)
        {
            if (TimeIsEmpty(xDateTime)) return false;
            if (TimeIsEmpty(xDateTimeStart)) return false;
            if (TimeIsEmpty(xDateTimeEnd)) return false;

            DateTime xTimeStart = new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTimeStart.Hour, xDateTimeStart.Minute, xDateTimeStart.Second);
            DateTime xTimeEnd = new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTimeEnd.Hour, xDateTimeEnd.Minute, xDateTimeEnd.Second);

            if ((xDateTime >= xDateTimeStart) && (xDateTime < xDateTimeEnd)) return true;
            return false;
        }

        public static bool MakeDateTimeFromTo(DateTime xDate, DateTime xTimeFrom, DateTime xTimeTo, out DateTime xDateTimeFrom, out DateTime xDateTimeTo)
        {
            return MakeDateTimeFromTo(xDate, xTimeFrom, xTimeTo, out xDateTimeFrom, out xDateTimeTo, null);
        }

        public static bool MakeDateTimeFromTo(DateTime xDate, DateTime xTimeFrom, DateTime xTimeTo, out DateTime xDateTimeFrom, out DateTime xDateTimeTo, byte? bBackDate)
        {
            xDateTimeFrom = DateTimeEmpty();
            xDateTimeTo = DateTimeEmpty();

            if (DateTimeIsEmpty(xDate)) return false;
            //if (DateTimeIsEmpty(xTimeFrom)) return false;
            if (TimeIsEmpty(xTimeTo)) return false;

            if (xTimeFrom.Hour < xTimeTo.Hour)
            {
                xDateTimeFrom = new DateTime(xDate.Year, xDate.Month, xDate.Day, xTimeFrom.Hour, xTimeFrom.Minute, xTimeFrom.Second);
                xDateTimeTo = new DateTime(xDate.Year, xDate.Month, xDate.Day, xTimeTo.Hour, xTimeTo.Minute, xTimeTo.Second);
            }
            else
            {
                xDateTimeFrom = new DateTime(xDate.Year, xDate.Month, xDate.Day, xTimeFrom.Hour, xTimeFrom.Minute, xTimeFrom.Second);
                xDateTimeTo = new DateTime(xDate.Year, xDate.Month, xDate.Day, xTimeTo.Hour, xTimeTo.Minute, xTimeTo.Second);
                if (xDate.Hour > 0)
                {
                    if (xDate.Hour < xTimeTo.Hour)
                    {
                        xDateTimeFrom = xDateTimeFrom.AddDays(-1.0);
                    }
                    else
                    {
                        xDateTimeTo = xDateTimeTo.AddDays(1.0);
                    }
                }
                else
                {
                    if (null == bBackDate)
                    {
                        if (xTimeFrom.Hour <= 8)
                        {
                            xDateTimeFrom = xDateTimeFrom.AddDays(-1.0);
                        }
                        else
                        {
                            xDateTimeTo = xDateTimeTo.AddDays(1.0);
                        }
                    }
                    else
                    {
                        if ((byte)1 == (byte)bBackDate)
                        {
                            xDateTimeFrom = xDateTimeFrom.AddDays(-1.0);
                        }
                        else
                        {
                            xDateTimeTo = xDateTimeTo.AddDays(1.0);
                        }
                    }
                }
            }
            return true;
        }

        public static bool DateTimeIsInner(DateTime xDateTime, DateTime xDateTimeStart, DateTime xDateTimeEnd)  //$$$NING
        {
            if (DateTimeIsEmpty(xDateTime)) return false;
            if (DateTimeIsEmpty(xDateTimeStart) && DateTimeIsEmpty(xDateTimeEnd)) return true;
            bool bOk = false;

            if (DateTimeIsEmpty(xDateTimeStart))
            {
                bOk = true;
            }
            else
            {
                if (xDateTime >= xDateTimeStart) bOk = true;
            }
            if (bOk)
            {
                bOk = false;
                if (DateTimeIsEmpty(xDateTimeEnd))
                {
                    bOk = true;
                }
                else
                {
                    if (xDateTime <= xDateTimeEnd) bOk = true;
                }
            }

            return bOk;
        }

        public static bool DateTimeIsOverlap(DateTime xDateTimeMainStart, DateTime xDateTimeMainEnd, DateTime xDateTimeCheckStart, DateTime xDateTimeCheckEnd)
        {
            if (DateTimeIsEmpty(xDateTimeMainStart) || 
                DateTimeIsEmpty(xDateTimeMainEnd)) 
                return false;

            if (DateTimeIsEmpty(xDateTimeCheckStart) && 
                DateTimeIsEmpty(xDateTimeCheckEnd)) 
                return true;

            bool bOk = DateTimeIsInner(xDateTimeMainStart, xDateTimeCheckStart, xDateTimeCheckEnd);
            if (false == bOk)
            {
                bOk = DateTimeIsInner(xDateTimeMainEnd, xDateTimeCheckStart, xDateTimeCheckEnd);
            }
            return bOk;
        }

        public static string DateTimeEngString(DateTime dt)
        {
            System.Globalization.CultureInfo Culture_En = new System.Globalization.CultureInfo("en-US");
            return dt.ToString("dd/MM/yyyy HH:mm:ss", Culture_En); //สำหรับ ค.ศ.
        }

        public static string DateTimeThaiString(DateTime dt)
        {
            System.Globalization.CultureInfo Culture_Th = new System.Globalization.CultureInfo("th-TH");
            return dt.ToString("dd/MM/yyyy HH:mm:ss", Culture_Th); //สำหรับ พ.ศ.
        }

        public static string DateToStringDayMonthNameDisplay(DateTime dt, bool bShowDayName, bool bEnglish = false)
        {
            string strResult = string.Empty;
            //if (!bEnglish)
            //{
                string[] strDayArray = new string[] { "อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์" };
                string[] strMonthArray = new string[] { "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" };

                if (bEnglish)
                {
                    strMonthArray = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

                    // Summary:
                    //Sunday = 0,
                    // Monday = 1,
                    // Tuesday = 2,
                    // Wednesday = 3,
                    // Thursday = 4,
                    // Friday = 5,
                    // Saturday = 6,
                }


                int iYear = dt.Year;
                

                if (bEnglish)
                {
                    if (iYear > 2400)
                    {
                        iYear = iYear - 543;
                    }
                    
                    if (bShowDayName)
                    {
                        //	Sunday, 1 January 2012
                        strResult = string.Format("{0}, {1} {2} {3}", dt.DayOfWeek.ToString(), dt.Day, strMonthArray[dt.Month - 1], iYear);
                    }
                    else
                    {
                        strResult = string.Format("{0} {1} {2}", dt.DayOfWeek.ToString(), dt.Day, strMonthArray[dt.Month - 1], iYear);
                    }
                }
                else
                {
                    if (iYear < 2400)
                    {
                        iYear = iYear + 543;
                    }

                    if (bShowDayName)
                    {
                        strResult = string.Format("วัน{0} ที่ {1} {2} พ.ศ.{3}", strDayArray[(int)dt.DayOfWeek], dt.Day, strMonthArray[dt.Month - 1], iYear);
                    }
                    else
                    {

                        strResult = string.Format("วันที่ {0} {1} พ.ศ.{2}", dt.Day, strMonthArray[dt.Month - 1], iYear);
                    }
                }
            //}
            //else
            //{
 
            //}

            return strResult;
        }

        public static string DateTimeToStringDisplay(DateTime dt, byte byDisplayStyle, bool bEnglish = false)
        {
            //byDisplayStyle 1: d MMMM yyyy hh:mm
            string strResult = string.Empty;
            string[] strDayArray;   // = new string[] { "อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์" };
            string[] strMonthArray; // = new string[] { "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" };

            if (bEnglish)
            {
                strDayArray = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                strMonthArray = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            }
            else
            {
                strDayArray = new string[] { "อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์" };
                strMonthArray = new string[] { "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" };
            }
            // Summary:
            //Sunday = 0,
            // Monday = 1,
            // Tuesday = 2,
            // Wednesday = 3,
            // Thursday = 4,
            // Friday = 5,
            // Saturday = 6,

            int iYear = dt.Year;
            if (bEnglish)
            {
                if (iYear > 2400)
                {
                    iYear = iYear - 543;
                }
            }
            else
            {
                if (iYear < 2400)
                {
                    iYear = iYear + 543;
                }
            }
            if ((byte)1 == byDisplayStyle)
            {
                //1 January 2012 08:30
                strResult = string.Format("{0} {1} {2} {3:D2}:{4:D2}", dt.Day, strMonthArray[dt.Month - 1], iYear, dt.Hour, dt.Minute);
            }
            else if ((byte)2 == byDisplayStyle)
            {
                //1 January 2012
                strResult = string.Format("{0} {1} {2}", dt.Day, strMonthArray[dt.Month - 1], iYear);
            }
            return strResult;
        }

        public static string GetDayOfWeekName(int iDayOfWeek, bool bShortName = false, bool bEnglishView = false)
        {
            string strResult = string.Empty;
            if (iDayOfWeek > 6)
                return strResult;

            string[] strDayArray;   // = new string[] { "อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์" };

            if (bEnglishView)
            {
                if (bShortName)
                    strDayArray = new string[] { "Sun", "Mon", "Tue", "Wed", "Thur", "Fri", "Sat" };
                else
                    strDayArray = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            }
            else
            {
                if (bShortName)
                    strDayArray = new string[] { "อา.", "จ.", "อ.", "พ.", "พฤ.", "ศุ.", "ส." };
                else
                    strDayArray = new string[] { "อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์" };
            }
            strResult = strDayArray[iDayOfWeek];

            return strResult;
        }

        public static string GetMonthName(int iMonth, bool bShortName = false, bool bEnglishView = false)
        {
            string strResult = string.Empty;
            if (0 == iMonth || iMonth > 12)
                return strResult;
            
            string[] strMonthArray; // = new string[] { "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" };

            if (bEnglishView)
            {
                if (bShortName)
                    strMonthArray = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                else
                    strMonthArray = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            }
            else
            {
                if (bShortName)
                    strMonthArray = new string[] { "ม.ค.", "กุ.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค." };
                else
                    strMonthArray = new string[] { "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" };
            }
            strResult = strMonthArray[iMonth - 1];

            return strResult;
        }

        public static DateTime GenMaxTime(DateTime xDateTime)
        {
            if (DateTimeIsEmpty(xDateTime)) return xDateTime;

            DateTime xMaxDateTime = new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day, 23, 59, 59, 900);

            return xMaxDateTime;
        }

        public static DateTime GenMaxTimeMillisecond(DateTime xDateTime)
        {
            if (DateTimeIsEmpty(xDateTime)) return xDateTime;

            xDateTime = new DateTime(xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second, 900);

            return xDateTime;
        }

        public static DateTime GenDateTimeFromITime(DateTime xDate, int iTime)
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

        public static string GenSortableDateToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D4}-{1:D2}-{2:D2}", xDateTime.Year, xDateTime.Month, xDateTime.Day);
        }

        public static string GenSortableDateTimeHHMMSSMSToString(DateTime xDateTime)
        {
            if (DxDate.DateTimeIsEmpty(xDateTime)) return string.Empty;

            System.IFormatProvider provider = new System.Globalization.CultureInfo("en-US");

            return string.Format(provider, "{0:D4}-{1:D2}-{2:D2}T{3:D2}:{4:D2}:{5:D2}:{6:D3}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second, xDateTime.Millisecond);
        }

        public static  string GenDateBetweenToString(DateTime xDateTimeFrom, DateTime xDateTimeTo)
        {
            string strResult = string.Empty;
            if (DateTimeIsEmpty(xDateTimeFrom) || DateTimeIsEmpty(xDateTimeTo)) return strResult;

            string str = string.Format("{0:D4}-{1:D2}-{2:D2}", xDateTimeFrom.Year, xDateTimeFrom.Month, xDateTimeFrom.Day);
            string strTo = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", xDateTimeTo.Year, xDateTimeTo.Month, xDateTimeTo.Day, xDateTimeTo.Hour, xDateTimeTo.Minute, xDateTimeTo.Second);
            strResult = string.Format("Between '{0}' and '{1}'", str, strTo);

            return strResult;
        }

        public static  string GenDateTimeBetweenToString(DateTime xDateTimeFrom, DateTime xDateTimeTo)
        {
            string strResult = string.Empty;
            if (DateTimeIsEmpty(xDateTimeFrom) || DateTimeIsEmpty(xDateTimeTo)) return strResult;

            string str = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", xDateTimeFrom.Year, xDateTimeFrom.Month, xDateTimeFrom.Day, xDateTimeFrom.Hour, xDateTimeFrom.Minute, xDateTimeFrom.Second);
            string strTo = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", xDateTimeTo.Year, xDateTimeTo.Month, xDateTimeTo.Day, xDateTimeTo.Hour, xDateTimeTo.Minute, xDateTimeTo.Second);
            strResult = string.Format("Between '{0}' and '{1}'", str, strTo);

            return strResult;
        }

        public static string GenDateTimeHHMMSSToString(DateTime xDateTime)
        {
            string strResult = string.Empty;
            if (DateTimeIsEmpty(xDateTime)) return strResult;

            strResult = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute, xDateTime.Second);

            return strResult;
        }

        public static string GenDateTimeHHMMToString(DateTime xDateTime)
        {
            string strResult = string.Empty;
            if (DateTimeIsEmpty(xDateTime)) return strResult;

            strResult = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute);

            return strResult;
        }

        public static string GenDateTimeToShortDatehmmssString(DateTime xDateTime)
        {
            string strResult = string.Empty;
            if (DateTimeIsEmpty(xDateTime)) return strResult;

            strResult = string.Format("{0:d} {0:h}:{0:mm}:{0:ss}", xDateTime.Year, xDateTime.Month, xDateTime.Day, xDateTime.Hour, xDateTime.Minute);

            return strResult;
        }

        public static string GenDateToString(DateTime xDateTime, bool bMonthShortName)
        {
            string strResult = string.Empty;
            if (DateTimeIsEmpty(xDateTime)) return strResult;

            string strDay = xDateTime.Day.ToString();
            string strYear = String.Format("{0:yyyy}", xDateTime);

            int iYear = 0;
            int.TryParse(strYear,out iYear);
            if(iYear > 0 && iYear < 2100)
            {
                strYear = (iYear + 543).ToString();
            }

            string strMonth = "";

            if (bMonthShortName)
            {
                //มกราคม	ม.ค.	JAN
                //กุมภาพันธ์	ก.พ.	FEB
                //มีนาคม	มี.ค.	MAR
                //เมษายน	เม.ย.	APR
                //พฤษภาคม	พ.ค.	MAY
                //มิถุนายน	มิ.ย.	JUN
                //กรกฎาคม	ก.ค.	JUL
                //สิงหาคม	ส.ค.	AUG
                //กันยายน	ก.ย.	SEP
                //ตุลาคม	ต.ค.	OCT
                //พฤศจิกายนพ.ย.	NOV
                //ธันวาคม	ธ.ค.	DEC

                StringCollection strMonths = new StringCollection();
                strMonths.Add("-");
                strMonths.Add("ม.ค.");
                strMonths.Add("ก.พ.");
                strMonths.Add("มี.ค.");
                strMonths.Add("เม.ย");
                strMonths.Add("พ.ค.");
                strMonths.Add("มิ.ย.");
                strMonths.Add("ก.ค.");
                strMonths.Add("ส.ค.");
                strMonths.Add("ก.ย.");
                strMonths.Add("ต.ค.");
                strMonths.Add("พ.ย.");
                strMonths.Add("ธ.ค.");

                strMonth = strMonths[xDateTime.Month];

            }
            else
            {

                StringCollection strMonths = new StringCollection();
                strMonths.Add("-");
                strMonths.Add("มกราคม");
                strMonths.Add("กุมภาพันธ์");
                strMonths.Add("มีนาคม");
                strMonths.Add("เมษายน");
                strMonths.Add("พฤษภาคม");
                strMonths.Add("มิถุนายน");
                strMonths.Add("กรกฎาคม");
                strMonths.Add("สิงหาคม");
                strMonths.Add("กันยายน");
                strMonths.Add("ตุลาคม");
                strMonths.Add("พฤศจิกายน");
                strMonths.Add("ธันวาคม");

                strMonth = strMonths[xDateTime.Month];
            }
            strResult = string.Format("{0} {1} {2}", strDay, strMonth, strYear);

            return strResult;
        }

        public static string GenDateToStringTime(DateTime xDateTime, bool bMonthShortName)
        {
            string strResult = string.Empty;
            if (DateTimeIsEmpty(xDateTime)) return strResult;

            string strDate = GenDateToString(xDateTime, bMonthShortName);

            strResult = string.Format("{0} {1}:{2}", strDate, xDateTime.Hour, xDateTime.Minute);
            
            return strResult;
        }
        
        public static byte GetWeekNoOfMonth(DateTime xDate)
        {
            byte byWeekNo = (byte)0;
            if (DxDate.DateTimeIsEmpty(xDate))
            {
                return byWeekNo;
            }
            DateTime firstDayOfMonth = new DateTime(xDate.Year, xDate.Month, 1);
            int firstDay = (int)firstDayOfMonth.DayOfWeek;
            firstDay += (xDate.Day - 1);

            if (firstDay > 6)
            {
                double d = (double)(firstDay / 7);
                byWeekNo = (byte)Math.Ceiling(d);
            }
            byWeekNo++;
            return byWeekNo;
        }

        public static byte GetPeriodOfTimeIPD(DateTime xDateTime)     //1:Morning, 2:AfterNoon, 3:MidNight
        {
            byte byPeroidOfTime = (byte)0;
            if (DateTimeIsEmpty(xDateTime)) return byPeroidOfTime;

            if (xDateTime.Hour >= 7 &&
                xDateTime.Hour < 15)
            {
                byPeroidOfTime = (byte)1;
            }
            else if (xDateTime.Hour >= 15 &&
                    xDateTime.Hour < 23)
            {
                byPeroidOfTime = (byte)2;
            }
            else if (xDateTime.Hour >= 23 ||
                    xDateTime.Hour < 7) 
            {
                byPeroidOfTime = (byte)3;
            }

            return byPeroidOfTime;
        }


        public static double DiffDate(DateTime xDateTimeForm, DateTime xDateTimeTo)
        {
            double iDiffDate = (xDateTimeTo - xDateTimeForm).TotalDays;
            return iDiffDate;
        }


        public static string getMonthName(int iMonth)
        {
            string strMonth = "";
            if (iMonth == 1) strMonth = "ม.ค.";
            if (iMonth == 2) strMonth = "ก.พ.";
            if (iMonth == 3) strMonth = "มี.ค.";
            if (iMonth == 4) strMonth = "เม.ย.";
            if (iMonth == 5) strMonth = "พ.ค.";
            if (iMonth == 6) strMonth = "มิ.ย.";
            if (iMonth == 7) strMonth = "ก.ค.";
            if (iMonth == 8) strMonth = "ส.ค.";
            if (iMonth == 9) strMonth = "ก.ย.";
            if (iMonth == 10) strMonth = "ต.ค.";
            if (iMonth == 11) strMonth = "พ.ย.";
            if (iMonth == 12) strMonth = "ธ.ค.";

            return strMonth;
        }
    }
}
