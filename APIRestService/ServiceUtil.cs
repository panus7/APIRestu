 
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Net;
using System.Net.Mail;
using Newtonsoft.Json;

namespace APIRestServiceRestaurant
{
    class ServiceUtil
    {
        protected static string _LicenceKey = "ReU";
        protected static string _MobileContextKey = "MobileConnectDev@";

        public ServiceUtil()
        {
        }

        static public T CleanJson<T>(string jsonData)
        {
            int iError = 0;
            var json = jsonData.Replace("\t", "").Replace("\r\n", "").Replace("[INC]", "''").Replace("^", "'");  
            var loop = true;
            do
            {
                try
                {
                    var m = JsonConvert.DeserializeObject<T>(json);
                    loop = false;
                }
                catch (JsonReaderException ex)
                {
                    iError++;
                    if (iError == 100)
                    {
                        loop = false;
                    }

                    var position = ex.LinePosition;
                    var invalidChar = json.Substring(position - 2, 2);
                    invalidChar = invalidChar.Replace("\"", "'");
                    json = $"{json.Substring(0, position - 1)}{invalidChar}{json.Substring(position)}";
                }
            } while (loop);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string stringNumberComma(string strnumber, bool bWithDecimal)
        {
            decimal number = 0;
            decimal.TryParse(strnumber, out number);
            if (bWithDecimal)
            {
                return Convert.ToDecimal(number).ToString("#,##0.00");
            }
            else
            {
                return Convert.ToDecimal(number).ToString("#,##0");
            }
        }

        public static string ConvertClassToStringJson(object obj)
        {
            return ConvertClassToStringJson(obj, true, false);
        }

        public static string ConvertClassToStringJson(object obj, bool bEncodeSpecialChar, bool bBeautifyJSON)
        {
            string strJson = string.Empty;

            try
            {
                //strJson = new JavaScriptSerializer().Serialize(obj);
                strJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);     //new
                 
                if (bBeautifyJSON)
                {
                   // strJson = FormatJson(strJson);
                }

            }
            catch (Exception e)
            { }

            return strJson;
        }

        //public static string FormatJson(string json)
        //{
        //    dynamic parsedJson = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
        //}

        public static string GenerateLogExceptionID(string strMethodName)
        {
            return string.Format("{0}_{1}", strMethodName, DateTime.Now.ToString("yyyyMMddHHmmss"));
        }
           
        public static DateTime ConvertStringYYYYMMDDToDate(string strDate)
        {

            //YYYY-MM-DD (ค.ส.) 1900-12-31
            //if (null != strDate && strDate.Length == 10)
            //{
            //    try
            //    {
            //        string strYear = strDate.Substring(0, 4);
            //        int iYear = int.Parse(strYear);
            //        if (int.Parse(strYear) > 2400)
            //        {
            //            iYear = int.Parse(strYear) - 543;
            //        }
            //        ///
            //        string strMonth = strDate.Substring(5, 2);
            //        string strDay = strDate.Substring(8, 2);
            //        return new DateTime(iYear, int.Parse(strMonth), int.Parse(strDay));
            //    }
            //    catch (Exception)        //Exception ex
            //    { }
            //}
            if (null != strDate)
            {
                try
                {
                    char xSplit = ' ';
                    if (strDate.Contains('-')) xSplit = '-';
                    if (strDate.Contains('/')) xSplit = '/';
                    string[] strTemp = strDate.Split(xSplit);

                    if (strTemp.Length == 3)
                    {
                        string strYear = strTemp[0];
                        int iYear = int.Parse(strYear);
                        if (int.Parse(strYear) > 2400)
                        {
                            iYear = int.Parse(strYear) - 543;
                        }
                        ///
                        string strMonth = strTemp[1];
                        string strDay = strTemp[2];
                        return new DateTime(iYear, int.Parse(strMonth), int.Parse(strDay));
                    }
                }
                catch (Exception)        //Exception ex
                { }
            }

            return new DateTime();
        }

        public static bool getRunServerMode()
        {
            try
            {
                if ("TRUE" == WebConfigurationManager.AppSettings["DEBUG_MODE"])
                {
                    return false; //for debug
                }
            }
            catch    //(Exception e)
            { }

            return true; //Run Server            
        }
         
        public static string replaceValue(string strValue)
        {
            if (!string.IsNullOrEmpty(strValue))
            {
                strValue = strValue.Replace("~SLASH~", "/");
                strValue = strValue.Replace("~DASH~", "-");
                strValue = strValue.Replace("~UNDER~", "-");
            }
            return strValue;
        }

        public static string getConnectionString()
        {
            string ServerName = WebConfigurationManager.AppSettings["DxDBServerName"];
            string DBName = WebConfigurationManager.AppSettings["DxDBName"];
            string DBUser = WebConfigurationManager.AppSettings["DxDBUser"];
            string DBPassword = WebConfigurationManager.AppSettings["DxDBPassword"];

            return string.Format("Data Source = {0}; Initial Catalog = {1}; User Id = {2}; Password = {3};", ServerName, DBName, DBUser, DBPassword);
        }

        public static string inner_convertImgToBase64(string strOriginalPath)
        {
            try
            {
                if (File.Exists(strOriginalPath))
                {
                    FileInfo ff = new FileInfo(strOriginalPath);
                    string exten = ff.Extension;
                    exten = exten.Replace(".", "");
                    Image img;
                    using (FileStream stream = new FileStream(strOriginalPath, FileMode.Open, FileAccess.Read))
                    {
                        img = Image.FromStream(stream);

                        using (MemoryStream m = new MemoryStream())
                        {
                            img.Save(m, img.RawFormat);
                            byte[] imageBytes = m.ToArray();

                            string base64String = Convert.ToBase64String(imageBytes);
                            //data:image/png;base64,
                            return string.Format("data:image/{0};base64,{1}", exten.ToLower(), base64String);
                        }
                    }
                }
            }
            catch (Exception)    //Exception ex
            {

            }
            return string.Empty;
        }

        public static string GenDateString(DateTime dt, bool bEng)
        {
            if (bEng)
            {
                var ci = new CultureInfo("en-EN");
                string format = "MM-dd-yyyy";
                return dt.ToString(format, ci);
            }
            else
            {
                var ci = new CultureInfo("th-TH");
                string format = "MM-dd-yyyy";
                return dt.ToString(format, ci);
            }

        }
 
        public static String GetSite()
        {
            string strSite = WebConfigurationManager.AppSettings["SITE"];
            return strSite;
        }

        public static bool CheckAuthenMobileInterface(string strContextKey)
        {
            return strContextKey.Equals(ServiceUtil._MobileContextKey + GetSite());
        }

        public static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Transparent);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }
         
    }
}