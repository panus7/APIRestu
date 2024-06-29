using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;

namespace APIRestServiceRestaurant
{
    public class DxCollection
    {

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 50)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        public static StringCollection ConvertStringToCollection(string strCodeList)
        {
            StringCollection strReturnCodeColl = new StringCollection();
            if (!string.IsNullOrEmpty(strCodeList))
            {
                strReturnCodeColl.AddRange(strCodeList.Split('|'));
            }
            return strReturnCodeColl;
        }
        public static List<string> ConvertStringToList(string strCodeList)
        {
            List<string> strReturnCodeColl = new List<string>();
            if (!string.IsNullOrEmpty(strCodeList))
            {
                strReturnCodeColl.AddRange(strCodeList.Split('|'));
            }
            return strReturnCodeColl;
        }
        public static StringCollection ConvertStringToCollection(string strCodeList, char[] cSeparatorColl)
        {
            StringCollection strReturnCodeColl = new StringCollection();
            if (!string.IsNullOrEmpty(strCodeList))
            {
                strReturnCodeColl.AddRange(strCodeList.Split(cSeparatorColl));
            }
            return strReturnCodeColl;
        }

        public static string ConvertCollectionToString(StringCollection strCodeCollection)
        {
            StringBuilder xStringBuilder = new StringBuilder();
            foreach (string strCode in strCodeCollection)
            {
                if (xStringBuilder.Length > 0) xStringBuilder.Append("|");
                xStringBuilder.Append(strCode);
            }
            return xStringBuilder.ToString();
        }
        public static string ConvertListCollToString(List<string> strCodeCollection)
        {
            string strReturn = string.Empty;
            for (int i = 0; i < strCodeCollection.Count; i++)
            {
                if (!string.IsNullOrEmpty(strReturn)) strReturn += "|";
                strReturn += strCodeCollection[i];
            }
            return strReturn;
        }

        public static string ConvertListTToString<T>(List<T> strCodeCollection, char cSeparatorColl)
        {
            string strReturn = string.Empty;
            for (int i = 0; i < strCodeCollection.Count; i++)
            {
                if (!string.IsNullOrEmpty(strReturn)) strReturn += cSeparatorColl;
                strReturn += strCodeCollection[i].ToString();
            }
            return strReturn;
        }
        public static List<T> ConvertStringToListT<T>(string strCodeList, char cSeparatorColl)
        {
            List<T> strReturnCodeColl = new List<T>();
            if (!string.IsNullOrEmpty(strCodeList))
            {
                int iIndex = 0;
                string[] asText = strCodeList.Split(cSeparatorColl);
                foreach (string sText in asText)
                {
                    if (!string.IsNullOrEmpty(sText))
                    {
                        strReturnCodeColl.Insert(iIndex, (T)Convert.ChangeType(sText, typeof(T)) );
                        iIndex++;
                    }
                }
            }
            return strReturnCodeColl;
        }
        //public static T GetValue<T>(String value)
        //{
        //    return (T)Convert.ChangeType(value, typeof(T));
        //}

        public static StringCollection GetDistinctCodeCollection(DataTable xInputDataTable, string strColumnName)
        {
            StringCollection strCodeColl = new StringCollection();

            if (null == xInputDataTable) return strCodeColl;
            if (null == xInputDataTable.Columns[strColumnName]) return strCodeColl;
            if (!xInputDataTable.Columns[strColumnName].DataType.Equals(typeof(System.String))) return strCodeColl;

            foreach (DataRow xDataRow in xInputDataTable.Select())
            {
                if (DBNull.Value.Equals(xDataRow[strColumnName]) || null == xDataRow[strColumnName]) continue;
                if (strCodeColl.Contains((string)xDataRow[strColumnName])) continue;
                strCodeColl.Add((string)xDataRow[strColumnName]);
            }
            return strCodeColl;
        }

        public static List<string> GetDistinctCodeList(DataTable xInputDataTable, string strColumnName)
        {
            List<string> strCodeList = new List<string>();

            if (null == xInputDataTable) return strCodeList;
            if (null == xInputDataTable.Columns[strColumnName]) return strCodeList;
            if (!xInputDataTable.Columns[strColumnName].DataType.Equals(typeof(System.String))) return strCodeList;

            foreach (DataRow xDataRow in xInputDataTable.Select())
            {
                if (DBNull.Value.Equals(xDataRow[strColumnName]) || null == xDataRow[strColumnName]) continue;
                if (strCodeList.Contains((string)xDataRow[strColumnName])) continue;
                strCodeList.Add((string)xDataRow[strColumnName]);
            }
            return strCodeList;
        }

        public static List<DateTime> GetDistinctDateList(DataTable xInputDataTable, string strColumnName, bool bDesc)
        {
            List<DateTime> strCodeList = new List<DateTime>();
            List<string> strCodeListCheckDup = new List<string>();

            if (null == xInputDataTable) return strCodeList;
            if (null == xInputDataTable.Columns[strColumnName]) return strCodeList;
            if (!xInputDataTable.Columns[strColumnName].DataType.Equals(typeof(System.DateTime))) return strCodeList;

            string strSort = "";
            if (bDesc)
            {
                strSort = " " + strColumnName + " DESC";
            }

            foreach (DataRow xDataRow in xInputDataTable.Select("" , strSort))
            {
                if (DBNull.Value.Equals(xDataRow[strColumnName]) || null == xDataRow[strColumnName]) continue;
                if (strCodeListCheckDup.Contains(DxData.getValueDateTimeToString(xDataRow[strColumnName]))) continue;

                strCodeListCheckDup.Add(DxData.getValueDateTimeToString(xDataRow[strColumnName]));
                strCodeList.Add(DxData.getValueDateTime(xDataRow[strColumnName]));
            }
            return strCodeList;
        }


        public static ArrayList CopyArrayList(ArrayList oInputColl)
        {
            ArrayList oReturnCollection = new ArrayList();
            for (int i = 0; i < oInputColl.Count; i++)
            {
                oReturnCollection.Add(oInputColl[i]);
            }
            return oReturnCollection;
        }

        public static List<T> CopyListWithReverse<T>(List<T> xListOfInput)
        {
            List<T> xListOfReturn = new List<T>();
            for (int i = xListOfInput.Count - 1; i >= 0; i++)
            {
                xListOfReturn.Add(xListOfInput[i]);
            }
            return xListOfReturn;
        }

        public static List<T> CopyList<T>(List<T> xListOfInput)
        {
            List<T> xListOfReturn = new List<T>();
            for (int i = 0; i < xListOfInput.Count; i++)
            {
                xListOfReturn.Add(xListOfInput[i]);
            }
            return xListOfReturn;
        }

        public static List<T> GetPartialCode<T>(List<T> xListOfInput, int iWanted, out List<T> xListOfRemain)
        {
            xListOfRemain = new List<T>();
            if (xListOfInput.Count <= iWanted) return xListOfInput;
            List<T> xListOfReturn = new List<T>();
            int iCount = 0;

            foreach (T xTValue in xListOfInput)
            {
                if (iCount < iWanted)
                {
                    {
                        xListOfReturn.Add(xTValue);
                        iCount++;
                    }
                }
                else
                {
                    xListOfRemain.Add(xTValue);
                }
            }
            return xListOfReturn;
        }

        public static IList GetPartialCodeList(IList xListOfInput, int iReq, out List<object> xListOfRemain)
        {
            xListOfRemain = new List<object>();
            if (xListOfInput.Count <= iReq) return xListOfInput;
            List<object> xListOfReturn = new List<object>();
            int iCount = 0;

            foreach (object xTValue in xListOfInput)
            {
                if (iCount < iReq)
                {
                    xListOfReturn.Add(xTValue);
                    iCount++;
                }
                else
                {
                    xListOfRemain.Add(xTValue);
                }
            }
            return xListOfReturn;
        }

        public static ArrayList GetPartialCode(ArrayList strInputColl, int iRequire, out ArrayList strRemainColl)
        {
            strRemainColl = default(ArrayList);
            strRemainColl = new ArrayList();
            if (strInputColl.Count <= iRequire) return strInputColl;
            ArrayList strReturnCollection = new ArrayList();
            int iCount = 0;
            foreach (object strCode in strInputColl)
            {
                if (iCount < iRequire)
                {
                    strReturnCollection.Add(strCode);
                    iCount++;
                }
                else
                {
                    strRemainColl.Add(strCode);
                }
            }
            return strReturnCollection;
        }

        public static StringCollection GetPartialCode(StringCollection strInputColl, int iRequire, out StringCollection strRemainColl)
        {
            strRemainColl = default(StringCollection);
            strRemainColl = new StringCollection();
            StringCollection strReturnCollection = new StringCollection();
            int iCount = 0;
            foreach (string strCode in strInputColl)
            {
                if (iCount < iRequire)
                {
                    strReturnCollection.Add(strCode);
                    iCount++;
                }
                else
                {
                    strRemainColl.Add(strCode);
                }
            }
            return strReturnCollection;
        }


        public static List<string> SplitTextConcatedToListOfString(string sConcatedText)
        {
            List<string> ListOfText = new List<string>();
            string[] asText = sConcatedText.Split('|');
            foreach (string sText in asText)
            {
                if (!string.IsNullOrEmpty(sText))
                {
                    if (!ListOfText.Contains(sText))
                        ListOfText.Add(sText);
                }
            }
            return ListOfText;
        }

        public static List<string> MergeList(List<string> List1, List<string> List2)
        {
            List<string> ListOutput = new List<string>();
            foreach (string sText in List1)
            {
                if (!string.IsNullOrEmpty(sText))
                {
                    if (!ListOutput.Contains(sText))
                    {
                        ListOutput.Add(sText);
                    }
                }
            }

            foreach (string sText in List2)
            {
                if (!string.IsNullOrEmpty(sText))
                {
                    if (!ListOutput.Contains(sText))
                    {
                        ListOutput.Add(sText);
                    }
                }
            }
            return ListOutput;
        }

        public static void GenCollectionToString(StringCollection strCollection, out string strResult)
        {
            strResult = string.Empty;
            if (0 == strCollection.Count) return;

            foreach (string strTemp in strCollection)
            {
                if (string.Empty != strTemp)
                {
                    if (string.Empty != strResult) strResult += ", ";
                    strResult += string.Format("'{0}'", strTemp);
                }
            }
        }

        public static StringCollection GetPartialCollection(StringCollection strInputColl, int iWanted, out StringCollection strRemainColl)
        {
            strRemainColl = new StringCollection();
            StringCollection strReturnCollection = new StringCollection();
            int iCount = 0;
            foreach (string strCode in strInputColl)
            {
                if (iCount < iWanted)
                {
                    strReturnCollection.Add(strCode);
                    iCount++;
                }
                else
                {
                    strRemainColl.Add(strCode);
                }
            }
            return strReturnCollection;
        }

        public static StringCollection CopyStringCollection(StringCollection strInputColl1, StringCollection strInputColl2)
        {
            StringCollection strReturnCollection = new StringCollection();

            foreach (string strCode in strInputColl1)
            {
                strReturnCollection.Add(strCode);
            }
            foreach (string strCode in strInputColl2)
            {
                strReturnCollection.Add(strCode);
            }
            return strReturnCollection;
        }

        #region GetSubStringCollect
        public static string GetSubStringCollect(string strMain, out string strSub2)
        {
            string strSub1 = string.Empty;
            strSub2 = string.Empty;

            if (!string.IsNullOrEmpty(strMain))
            {
                int iIndex = strMain.IndexOf("|");
                if (iIndex == -1)
                {
                    strSub1 = strMain;
                }
                else
                {
                    strSub1 = strMain.Substring(0, iIndex);
                    if (strMain.Length > (iIndex + 1))
                    {
                        strSub2 = strMain.Substring(iIndex + 1);
                    }
                }
            }
            return strSub1;
        }
        #endregion GetSplitStringList

        #region GetVisitDateVN Dictionary
        public static Dictionary<DateTime, List<string>> GetVisitDateVNDictionary(DataTable xInputDataTable, string VisitDateColumnName, string VNColumnName)
        {
            var _Dict = new Dictionary<DateTime, List<string>>();

            if (!xInputDataTable.Columns.Contains(VisitDateColumnName)) return _Dict;
            if (!xInputDataTable.Columns.Contains(VNColumnName)) return _Dict;

            foreach (DataRow xRow in xInputDataTable.Rows)
            {
                List<DateTime> asKeys = _Dict.Keys.ToList();

                string strVN = xRow[VNColumnName].ToString();
                DateTime xVisitDate = (DateTime)xRow[VisitDateColumnName];

                if (!asKeys.Contains(xVisitDate))
                {
                    List<string> strCodeColl = new List<string>();
                    _Dict.Add(xVisitDate, strCodeColl);
                }

                if (!_Dict[xVisitDate].Contains(strVN))
                {
                    _Dict[xVisitDate].Add(strVN);
                }
            }

            return _Dict;
        }

        #endregion

        public static bool CompareArrayLists(ArrayList arr1, ArrayList arr2)
        {
            //Check if the two arraysLists have the same length
            if (arr1.Count != arr2.Count)
                return false;

            //Iterate through each element and compare if it is equal to the

            for (int i = 0; i < arr1.Count; i++)
            {
                if (!arr1[i].Equals(arr2[i]))
                    return false;
            }

            return true;
        }
    }
}
