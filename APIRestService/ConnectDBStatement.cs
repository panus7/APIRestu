using System;
using System.Data;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace APIRestServiceRestaurant
{
    public class Class_DBStatement
    {
    }

    public enum DBComparisonOperator
    {
        Equal = 1,
        NotEqualTo = 2,
        GreaterThan = 3,
        GreaterOrEqualTo = 4,
        LessThan = 5,
        LessThanOrEqualTo = 6,
        EqualEver = 99,
    }

    public enum DBLogicalOperators
    {
        NORMAL = 0,
        IS_NULL = 1,
        BETWEEN = 2,
        BETWEEN_AND_LIKE = 7,
        LIKE = 3,
        IN = 4,
        LIKE_ASSIGN = 5,
        CONTAINS = 6,
        CONTAINS_PREFIX = 8,
    }

    public enum DBBitwiseOperators
    {
        AND = 1,
        OR = 2,
    }

    public enum CheckValueType : byte
    {
        None = 0,
        Set = 1,
        Not_Set = 2,
    }

    [Serializable]
    public class DBCondition
    {
        protected string vCondition = string.Empty;

        public DBCondition()
        {
        }

        public DBCondition(string strCon)
        {
            vCondition = strCon;
        }

        public bool IsEmpty
        {
            get
            {
                return (vCondition.Length == 0) ? true : false;
            }
        }

        public static DBCondition FromStatement(string strCon) //FromStmt
        {
            return new DBCondition(strCon);
        }

        public static DBCondition ConcatStatement(IList ListColl, DBBitwiseOperators JoinWith, bool bSplitGroup)  //ConcatStmt
        {
            if ((null == ListColl) || (0 == ListColl.Count)) return new DBCondition();

            string strJoin = (DBBitwiseOperators.AND == JoinWith) ? " AND " : " OR ";
            string strCon = string.Empty;
            string strStatement = string.Empty;

            foreach (object objTemp in ListColl)
            {
                if (objTemp is DBExpression)
                {
                    strStatement = ((DBExpression)objTemp).ToString();
                    if (strStatement.Length > 0)
                    {
                        if (strCon.Length > 0) strCon += strJoin;
                        if (false == bSplitGroup)
                        {
                            strCon += strStatement;
                        }
                        else
                        {
                            strCon += string.Format("({0})", strStatement);
                        }
                    }
                }
                else if (objTemp is DBCondition)
                {
                    strStatement = ((DBCondition)objTemp).ToString();
                    if (strStatement.Length > 0)
                    {
                        if (strCon.Length > 0) strCon += strJoin;
                        if (false == bSplitGroup)
                        {
                            strCon += strStatement;
                        }
                        else
                        {
                            strCon += string.Format("({0})", strStatement);
                        }
                    }
                }
                else if (objTemp is System.String)
                {
                    strStatement = (System.String)objTemp;
                    if (strStatement.Length > 0)
                    {
                        if (strCon.Length > 0) strCon += strJoin;
                        if (false == bSplitGroup)
                        {
                            strCon += strStatement;
                        }
                        else
                        {
                            strCon += string.Format("({0})", strStatement);
                        }
                    }
                }
                else
                {
                    throw (new ArgumentException());
                }
            }

            return new DBCondition(strCon);
        }

        public static DBCondition JoinStatement(params object[] StatementList)
        {
            string strResult = string.Empty;
            bool bIgnoreOperator = false;

            if (0 == StatementList.Length % 2) throw (new ArgumentException());

            for (int count = 0; count < StatementList.Length; count++)
            {
                object objStatement = StatementList[count];
                string strTemp;

                if (0 == count % 2)
                {
                    bIgnoreOperator = true;

                    if (objStatement is DBCondition)
                    {
                        strTemp = ((DBCondition)objStatement).ToString();
                        if (strTemp.Length > 0)
                        {
                            strResult += string.Format("({0})", strTemp);
                            bIgnoreOperator = false;
                        }
                    }
                    else if (objStatement is DBExpression)
                    {
                        strTemp = ((DBExpression)objStatement).ToString();
                        if (strTemp.Length > 0)
                        {
                            strResult += string.Format("({0})", strTemp);
                            bIgnoreOperator = false;
                        }
                    }
                    else if (objStatement is System.String)
                    {
                        strTemp = (System.String)objStatement;
                        if (strTemp.Length > 0)
                        {
                            strResult += string.Format("({0})", strTemp);
                            bIgnoreOperator = false;
                        }
                    }
                    else
                    {
                        throw (new ArgumentException());
                    }
                }
                else
                {
                    if (objStatement is DBBitwiseOperators)
                    {
                        if (false == bIgnoreOperator)
                        {
                            if (DBBitwiseOperators.AND == ((DBBitwiseOperators)objStatement))
                            {
                                strResult += " AND ";
                            }
                            else
                            {
                                strResult += " OR ";
                            }
                        }
                    }
                    else
                    {
                        throw (new ArgumentException());
                    }
                }
            }

            if (true == strResult.EndsWith(" AND ")) strResult = strResult.Substring(0, strResult.Length - 5);
            if (true == strResult.EndsWith(" OR ")) strResult = strResult.Substring(0, strResult.Length - 4);

            return DBCondition.FromStatement(strResult);
        }

        private static string ConcatStatement(string strStatement1, DBBitwiseOperators JoinWith, string strStatement2)
        {
            if (DBBitwiseOperators.AND == JoinWith)
            {
                return ConcatStatement(strStatement1, "AND", strStatement2);
            }
            else if (DBBitwiseOperators.OR == JoinWith)
            {
                return ConcatStatement(strStatement1, "OR", strStatement2);
            }

            return string.Empty;
        }

        private static string ConcatStatement(string strStatement1, string strJoin, string strStatement2)
        {
            if (strStatement1.Length == 0)
            {
                if (strStatement2.Length == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return strStatement2.Trim();
                }
            }
            else
            {
                if (strStatement2.Length == 0)
                {
                    return strStatement1.Trim();
                }
                else
                {
                    strStatement1 = strStatement1.Trim();
                    strStatement2 = strStatement2.Trim();
                    if (false == strStatement1.StartsWith("(")) strStatement1 = string.Format("({0})", strStatement1);
                    if (false == strStatement2.StartsWith("(")) strStatement2 = string.Format("({0})", strStatement2);

                    return string.Format("{0} {1} {2}", strStatement1, strJoin, strStatement2);
                }
            }
        }

        public static DBCondition operator &(DBCondition SQLCon1, DBCondition SQLCon2)
        {
            return new DBCondition(DBCondition.ConcatStatement(SQLCon1.ToString(), "AND", SQLCon2.ToString()));
        }

        public static DBCondition operator &(DBCondition SQLCon, DBExpression SQLExp)
        {
            return (SQLCon & SQLExp.ToString());
        }

        public static DBCondition operator &(DBExpression SQLExp, DBCondition SQLCon)
        {
            return (SQLExp.ToString() & SQLCon);
        }

        public static DBCondition operator &(DBCondition SQLCon, string strCon)
        {
            return new DBCondition(DBCondition.ConcatStatement(SQLCon.ToString(), "AND", strCon));
        }

        public static DBCondition operator &(string strCon, DBCondition SQLCon)
        {
            return new DBCondition(DBCondition.ConcatStatement(strCon, "AND", SQLCon.ToString()));
        }

        public static DBCondition operator |(DBCondition SQLCon1, DBCondition SQLCon2)
        {
            return new DBCondition(DBCondition.ConcatStatement(SQLCon1.ToString(), "OR", SQLCon2.ToString()));
        }

        public static DBCondition operator |(DBCondition SQLCon, DBExpression SQLExp)
        {
            return (SQLCon | SQLExp.ToString());
        }

        public static DBCondition operator |(DBExpression SQLExp, DBCondition SQLCon)
        {
            return (SQLExp.ToString() | SQLCon);
        }

        public static DBCondition operator |(DBCondition SQLCon, string strCon)
        {
            return new DBCondition(DBCondition.ConcatStatement(SQLCon.ToString(), "OR", strCon));
        }

        public static DBCondition operator |(string strCon, DBCondition SQLCon)
        {
            return new DBCondition(DBCondition.ConcatStatement(strCon, "OR", SQLCon.ToString()));
        }

        public static implicit operator DBCondition(DBExpression SQLExp)
        {
            return new DBCondition(SQLExp.ToString());
        }

        internal void Cover_Parenthesis()
        {
            this.vCondition = "(" + this.vCondition + ")";
        }

        public override string ToString()
        {
            return vCondition;
        }

        public string ToString(bool bParenthesis)
        {
            return (false == bParenthesis) ? vCondition : string.Format("({0})", vCondition);
        }
    }

    [Serializable]
    public class DBExpression
    {
        #region Variables Declare
        protected string _FieldName;

        protected DBLogicalOperators _dbLogicalOperators;
        protected DBComparisonOperator _Operator;
        protected bool _bNOT;
        protected bool _bAlways = false;
        protected object _Value1;
        protected object _Value2;
        protected bool _MemoryDB = false;

        protected string vExpression = string.Empty;
        #endregion

        #region Methods Area

        public DBExpression()
        {
        }

        public DBExpression(string strExpression)
        {
            vExpression = strExpression;
        }

        private static string ConcatStatement(string strStatement1, string strJoin, string strStatement2)
        {
            if (strStatement1.Length == 0)
            {
                if (strStatement2.Length == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return strStatement2.Trim();
                }
            }
            else
            {
                if (strStatement2.Length == 0)
                {
                    return strStatement1.Trim();
                }
                else
                {
                    strStatement1 = strStatement1.Trim();
                    strStatement2 = strStatement2.Trim();
                    if (false == strStatement1.StartsWith("(")) strStatement1 = string.Format("({0})", strStatement1);
                    if (false == strStatement2.StartsWith("(")) strStatement2 = string.Format("({0})", strStatement2);

                    return string.Format("{0} {1} {2}", strStatement1, strJoin, strStatement2);
                }
            }
        }

        public static DBCondition operator &(DBExpression SQLExp1, DBExpression SQLExp2)
        {
            return (SQLExp1 & SQLExp2.ToString());
        }

        public static DBCondition operator &(DBExpression SQLExp, string strCon)
        {
            return new DBCondition(DBExpression.ConcatStatement(SQLExp.ToString(), "AND", strCon));
        }

        public static DBCondition operator &(string strCon, DBExpression SQLExp)
        {
            return new DBCondition(DBExpression.ConcatStatement(strCon, "AND", SQLExp.ToString()));
        }

        public static DBCondition operator |(DBExpression SQLExp1, DBExpression SQLExp2)
        {
            return (SQLExp1 | SQLExp2.ToString());
        }

        public static DBCondition operator |(DBExpression SQLExp, string strCon)
        {
            return new DBCondition(DBExpression.ConcatStatement(SQLExp.ToString(), "OR", strCon));
        }

        public static DBCondition operator |(string strCon, DBExpression SQLExp)
        {
            return new DBCondition(DBExpression.ConcatStatement(strCon, "OR", SQLExp.ToString()));
        }

        public override string ToString()
        {
            if (vExpression.Length > 0) return vExpression;

            string strValue1 = string.Empty;
            string strValue2 = string.Empty;
            string strResult = string.Empty;

            switch (_dbLogicalOperators)
            {
                case DBLogicalOperators.NORMAL:                    
                    {
                        if (null == _Value1) return strResult;
                        strValue1 = GetValue(_Value1, _dbLogicalOperators);

                        if (true == _bAlways)
                        {
                            if (0 == strValue1.Length) strValue1 = "''";
                            strResult = string.Format("{0} {1} {2}", _FieldName, GetOperator(_Operator, _bNOT), strValue1);
                        }
                        else
                        {
                            switch (_Operator)
                            {
                                case DBComparisonOperator.EqualEver:
                                    {
                                        if (0 == strValue1.Length) strValue1 = "''";
                                        strResult = string.Format("{0} {1} {2}", _FieldName, GetOperator(_Operator, _bNOT), strValue1);
                                    }
                                    break;

                                case DBComparisonOperator.NotEqualTo:
                                    {
                                        if (0 == strValue1.Length) return strResult;
                                        strResult = string.Format("( ({0} {1} {2}) OR ({0} IS NULL) )", _FieldName, GetOperator(_Operator, _bNOT), strValue1);
                                    }
                                    break;

                                case DBComparisonOperator.Equal:
                                default:
                                    {
                                        if (0 == strValue1.Length) return strResult;
                                        strResult = string.Format("{0} {1} {2}", _FieldName, GetOperator(_Operator, _bNOT), strValue1);
                                    }
                                    break;
                            }
                        }
                    }
                    break;

                case DBLogicalOperators.IS_NULL:
                    if (false == _bNOT)
                    {
                        strResult = string.Format("{0} IS NULL", _FieldName);
                    }
                    else
                    {
                        strResult = string.Format("{0} IS NOT NULL", _FieldName);
                    }
                    break;

                case DBLogicalOperators.BETWEEN_AND_LIKE:
                    if (_Value1 is System.String)
                    {
                        strValue1 = GetValue(_Value1, _dbLogicalOperators);
                        if (0 == strValue1.Length) return strResult;

                        strValue2 = GetValue(_Value2, _dbLogicalOperators);
                        if (0 == strValue2.Length) return strResult;

                        string strValue2_LikeFormat = string.Format("'{0}%'", strValue2.Trim('\''));

                        strResult = string.Format("( ({0} >= {1} AND {0} <= {2}) OR ({0} LIKE {3}) )", _FieldName, strValue1, strValue2, strValue2_LikeFormat);
                    }
                    else
                    {
                        if (null == _Value1) return strResult;
                        strValue1 = GetValue(_Value1, _dbLogicalOperators);
                        if (0 == strValue1.Length) return strResult;

                        strValue2 = GetValue(_Value2, _dbLogicalOperators);
                        if (0 == strValue2.Length) return strResult;

                        if (false == _bNOT)
                        {
                            strResult = string.Format("{0} BETWEEN {1} AND {2}", _FieldName, strValue1, strValue2);
                        }
                        else
                        {
                            strResult = string.Format("{0} NOT BETWEEN {1} AND {2}", _FieldName, strValue1, strValue2);
                        }
                    }
                    break;

                case DBLogicalOperators.BETWEEN:
                    if (_Value1 is System.String)
                    {
                        strValue1 = GetValue(_Value1, _dbLogicalOperators);
                        if (0 == strValue1.Length) return strResult;

                        strValue2 = GetValue(_Value2, _dbLogicalOperators);
                        if (0 == strValue2.Length) return strResult;

                        strResult = string.Format("({0} >= {1} AND {0} <= {2})", _FieldName, strValue1, strValue2);
                    }
                    else
                    {
                        if (null == _Value1) return strResult;
                        strValue1 = GetValue(_Value1, _dbLogicalOperators);
                        if (0 == strValue1.Length) return strResult;

                        strValue2 = GetValue(_Value2, _dbLogicalOperators);
                        if (0 == strValue2.Length) return strResult;

                        if (false == _bNOT)
                        {
                            strResult = string.Format("{0} BETWEEN {1} AND {2}", _FieldName, strValue1, strValue2);
                        }
                        else
                        {
                            strResult = string.Format("{0} NOT BETWEEN {1} AND {2}", _FieldName, strValue1, strValue2);
                        }
                    }
                    break;

                case DBLogicalOperators.LIKE:
                    if (null == _Value1) return strResult;
                    strValue1 = GetValue(_Value1, _dbLogicalOperators);
                    if (0 == strValue1.Length) return strResult;

                    if (false == _bNOT)
                    {
                        strResult = string.Format("{0} LIKE {1}", _FieldName, strValue1);
                    }
                    else
                    {
                        strResult = string.Format("{0} NOT LIKE {1}", _FieldName, strValue1);
                    }
                    break;


                case DBLogicalOperators.CONTAINS:
                    {
                        if (null == _Value1) return strResult;
                        if (_Value1 is System.String)
                        {
                            strResult = string.Format("CONTAINS({0}, '\"{1}\"')", _FieldName, (string)_Value1);
                        }
                    }
                    break;

                case DBLogicalOperators.CONTAINS_PREFIX:
                    {
                        if (null == _Value1) return strResult;
                        if (_Value1 is System.String)
                        {
                            strResult = string.Format("CONTAINS({0}, '\"{1}*\"')", _FieldName, (string)_Value1);
                        }
                    }
                    break;

                case DBLogicalOperators.IN:
                    if (null == _Value1) return strResult;
                    if (false == _bNOT)
                    {
                        if (_Value1 is System.String)
                        {
                            if (0 == ((string)_Value1).Length) return strResult;

                            if (true == ((string)_Value1).StartsWith("SELECT "))
                            {
                                strResult = string.Format("{0} IN ({1})", _FieldName, (string)_Value1);
                            }
                            else
                            {
                                string strTemp = GetValue((string)_Value1, _dbLogicalOperators);

                                if (strTemp.Length > 0)
                                {
                                    strResult = string.Format("{0} = {1}", _FieldName, strTemp);
                                }
                                else
                                {
                                    strResult = string.Format("{0} IS NULL", _FieldName);
                                }
                            }
                        }
                        else if (_Value1 is IList)
                        {
                            string strList = string.Empty;
                            string strTemp;

                            if (((IList)_Value1).Count == 0) return strResult;
                            if (((IList)_Value1).Count == 1)
                            {
                                strTemp = GetValue(((IList)_Value1)[0], _dbLogicalOperators);
                                if (strTemp.Length > 0)
                                {
                                    strResult = string.Format("{0} = {1}", _FieldName, strTemp);
                                }
                                else
                                {
                                    strResult = string.Format("{0} IS NULL", _FieldName);
                                }
                            }
                            else
                            {
                                foreach (object objTemp in (IList)_Value1)
                                {
                                    if (strList.Length > 0) strList += ",";
                                    strTemp = GetValue(objTemp, _dbLogicalOperators);
                                    strList += (strTemp.Length > 0) ? strTemp : "NULL";
                                }

                                strResult = string.Format("{0} IN ({1})", _FieldName, strList);
                            }
                        }
                    }
                    else
                    {
                        if (_Value1 is System.String)
                        {
                            if (0 == ((string)_Value1).Length) return strResult;
                            strResult = string.Format("{0} NOT IN ({1})", _FieldName, (string)_Value1);
                        }
                        else if (_Value1 is IList)
                        {
                            string strList = string.Empty;
                            string strTemp;

                            if (((IList)_Value1).Count == 0) return strResult;
                            if (((IList)_Value1).Count == 1)
                            {
                                strTemp = GetValue(((IList)_Value1)[0], _dbLogicalOperators);

                                if (strTemp.Length > 0)
                                {
                                    strResult = string.Format("( ({0} <> {1}) OR ({0} IS NULL) )", _FieldName, strTemp);
                                }
                                else
                                {
                                    strResult = string.Format("{0} IS NOT NULL", _FieldName);
                                }
                            }
                            else
                            {
                                foreach (object objTemp in (IList)_Value1)
                                {
                                    if (strList.Length > 0) strList += ",";
                                    strTemp = GetValue(objTemp, _dbLogicalOperators);
                                    strList += (strTemp.Length > 0) ? strTemp : "NULL";
                                }

                                strResult = string.Format("( ({0} NOT IN ({1})) OR ({0} IS NULL) )", _FieldName, strList);
                            }
                        }
                    }
                    break;
            }

            return strResult;
        }

        private string GetValue(object Value, DBLogicalOperators ExpType)
        {
            if (Value is System.Char)
            {
                return Value.ToString();
            }
            else if (Value is System.Byte)
            {
                return Value.ToString();
            }
            else if (Value is System.SByte)
            {
                return Value.ToString();
            }
            else if (Value is System.Int16)
            {
                return Value.ToString();
            }
            else if (Value is System.UInt16)
            {
                return Value.ToString();
            }
            else if (Value is System.Int32)
            {
                return Value.ToString();
            }
            else if (Value is System.UInt32)
            {
                return Value.ToString();
            }
            else if (Value is System.Int64)
            {
                return Value.ToString();
            }
            else if (Value is System.UInt64)
            {
                return Value.ToString();
            }
            else if (Value is System.Single)
            {
                return Value.ToString();
            }
            else if (Value is System.Double)
            {
                return Value.ToString();
            }
            else if (Value is System.Decimal)
            {
                return Value.ToString();
            }
            else if (Value is System.String)
            {
                string strValue = Value.ToString();
                string strResult = string.Empty;

                if (strValue.Length > 0)
                {
                    if (DBLogicalOperators.LIKE == ExpType)
                    {
                        if ("." != strValue)
                        {
                            if ('%' != strValue[strValue.Length - 1]) strValue += '%';
                        }
                    }

                    strValue = strValue.Replace("'", "''");
                    strResult = string.Format("'{0}'", strValue);
                }

                return strResult;
            }
            else if (Value is System.DateTime)
            {
                //return string.Format("'{0}'", ((DateTime)Value).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", new CultureInfo("en-US")));

                if (((DateTime)Value).Ticks % 10000 > 0)
                {
                    if ((DBLogicalOperators.BETWEEN == ExpType) || (DBLogicalOperators.BETWEEN_AND_LIKE == ExpType))
                    {
                        DateTime xDateTime_Source = (DateTime)Value;
                        DateTime xDateTime_Temp = new DateTime(xDateTime_Source.Year, xDateTime_Source.Month, xDateTime_Source.Day, xDateTime_Source.Hour, xDateTime_Source.Minute, xDateTime_Source.Second);
                        int nMSec = xDateTime_Source.Millisecond;

                        switch (nMSec % 10)
                        {
                            case 0:
                            case 1:
                                nMSec = Convert.ToInt32((nMSec / 10) * 10);
                                break;

                            case 2:
                            case 3:
                            case 4:
                                nMSec = Convert.ToInt32((nMSec / 10) * 10) + 3;
                                break;

                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                nMSec = Convert.ToInt32((nMSec / 10) * 10) + 7;
                                break;

                            case 9:
                                nMSec = Convert.ToInt32((nMSec / 10) * 10) + 10;
                                break;
                        }

                        xDateTime_Temp = xDateTime_Temp.AddMilliseconds(nMSec);

                        return string.Format("'{0}'", xDateTime_Temp.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", new CultureInfo("en-US")));
                    }
                    else
                    {
                        return string.Format("'{0}'", ((DateTime)Value).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", new CultureInfo("en-US")));
                    }
                }
                else
                {
                    if ((DBLogicalOperators.BETWEEN == ExpType) || (DBLogicalOperators.BETWEEN_AND_LIKE == ExpType))
                    {
                        DateTime xDateTime_Source = (DateTime)Value;
                        DateTime xDateTime_Temp = new DateTime(xDateTime_Source.Year, xDateTime_Source.Month, xDateTime_Source.Day, xDateTime_Source.Hour, xDateTime_Source.Minute, xDateTime_Source.Second);
                        int nMSec = xDateTime_Source.Millisecond;

                        switch (nMSec % 10)
                        {
                            case 0:
                            case 1:
                                nMSec = Convert.ToInt32((nMSec / 10) * 10);
                                break;

                            case 2:
                            case 3:
                            case 4:
                                nMSec = Convert.ToInt32((nMSec / 10) * 10) + 3;
                                break;

                            case 5:
                            case 6:
                            case 7:
                            case 8:
                                nMSec = Convert.ToInt32((nMSec / 10) * 10) + 7;
                                break;

                            case 9:
                                nMSec = Convert.ToInt32((nMSec / 10) * 10) + 10;
                                break;
                        }

                        xDateTime_Temp = xDateTime_Temp.AddMilliseconds(nMSec);

                        return string.Format("'{0}'", xDateTime_Temp.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", new CultureInfo("en-US")));
                    }
                    else
                    {
                        return string.Format("'{0}'", ((DateTime)Value).ToString("yyyy-MM-dd'T'HH:mm:ss.fff", new CultureInfo("en-US")));
                    }
                }
            }
           
            return string.Empty;
        }

        private static object ScanEmptyValue(object Value)
        {
            if (null != Value)
            {
                if (Value is System.Char)
                {
                    
                }
                else if (Value is System.Byte)
                {
                    if (0 == (System.Byte)Value) Value = null;
                }
                else if (Value is System.SByte)
                {
                    if (0 == (System.SByte)Value) Value = null;
                }
                else if (Value is System.Int16)
                {
                    if (0 == (System.Int16)Value) Value = null;
                }
                else if (Value is System.UInt16)
                {
                    if (0 == (System.UInt16)Value) Value = null;
                }
                else if (Value is System.Int32)
                {
                    if (0 == (System.Int32)Value) Value = null;
                }
                else if (Value is System.UInt32)
                {
                    if (0 == (System.UInt32)Value) Value = null;
                }
                else if (Value is System.Int64)
                {
                    if (0 == (System.Int64)Value) Value = null;
                }
                else if (Value is System.UInt64)
                {
                    if (0 == (System.UInt64)Value) Value = null;
                }
                else if (Value is System.Single)
                {
                    if (0.0 == (System.Single)Value) Value = null;
                }
                else if (Value is System.Double)
                {
                    if (0.0 == (System.Double)Value) Value = null;
                }
                else if (Value is System.Decimal)
                {
                    if (0 == (System.Decimal)Value) Value = null;
                }
                else if (Value is System.String)
                {
                    if (0 == ((System.String)Value).Length) Value = null;
                }
                else if (Value is System.DateTime)
                {
                    //Class_Db db = new Class_Db(); 
                    //if ( .DxDate.DateTimeIsEmpty((System.DateTime)Value)) Value = null;
                }
                
                else if (Value is IList)
                {
                    if (0 == ((IList)Value).Count) Value = null;
                }
            }

            return Value;
        }

        private void ClearEmptyValue()
        {
            _Value1 = DBExpression.ScanEmptyValue(_Value1);
            _Value2 = DBExpression.ScanEmptyValue(_Value2);
        }

        private string GetOperator(DBComparisonOperator Operator, bool bNot)
        {
            if (false == bNot)
            {
                switch (Operator)
                {
                    case DBComparisonOperator.Equal:
                    case DBComparisonOperator.EqualEver:
                        return "=";

                    case DBComparisonOperator.NotEqualTo:
                        return "<>";

                    case DBComparisonOperator.GreaterThan:
                        return ">";

                    case DBComparisonOperator.GreaterOrEqualTo:
                        return ">=";

                    case DBComparisonOperator.LessThan:
                        return "<";

                    case DBComparisonOperator.LessThanOrEqualTo:
                        return "<=";
                }
            }
            else
            {
                switch (Operator)
                {
                    case DBComparisonOperator.Equal:
                    case DBComparisonOperator.EqualEver:
                        return "<>";

                    case DBComparisonOperator.NotEqualTo:
                        return "=";

                    case DBComparisonOperator.GreaterThan:
                        return "<=";

                    case DBComparisonOperator.GreaterOrEqualTo:
                        return "<";

                    case DBComparisonOperator.LessThan:
                        return ">=";

                    case DBComparisonOperator.LessThanOrEqualTo:
                        return ">";
                }
            }

            return string.Empty;
        }

        #region DBExpression.Normal
        public static DBExpression Normal(string FieldName, DBComparisonOperator Operator, object Value, bool bIgnore, bool bForMemoryDB)
        {
            DBExpression Result = new DBExpression();

            if (DBComparisonOperator.EqualEver != Operator)
            {
                if (false == bIgnore)
                {
                    Result._dbLogicalOperators = DBLogicalOperators.NORMAL;
                    Result._bNOT = false;
                    Result._FieldName = FieldName.ToUpper();
                    Result._Operator = Operator;
                    Result._Value1 = Value;
                    Result._MemoryDB = bForMemoryDB;

                    if (Value is System.DateTime)
                    {
                        if ((DBComparisonOperator.LessThanOrEqualTo == Operator) || (DBComparisonOperator.LessThan == Operator))
                        {
                            Result._Value1 = (System.DateTime)Value;
                        }
                    }
                    else if (Value is System.Enum)
                    {
                        Result._Value1 = System.Convert.ChangeType(Value, System.Enum.GetUnderlyingType(Value.GetType()));
                    }


                    Result.ClearEmptyValue();
                }
            }
            else
            {
                Result = DBExpression.Normal_Always(FieldName, Operator, Value, bIgnore);
            }

            return Result;
        }

        public static DBExpression Normal(string FieldName, DBComparisonOperator Operator, object Value, bool bIgnore)
        {
            return DBExpression.Normal(FieldName, Operator, Value, bIgnore, false);
        }

        public static DBExpression Normal(string FieldName, DBComparisonOperator Operator, object Value)
        {
            return DBExpression.Normal(FieldName, Operator, Value, false, false);
        }
        #endregion

        #region DBExpression.Normal_Always
        public static DBExpression Normal_Always(string FieldName, DBComparisonOperator Operator, object Value, bool bIgnore)
        {
            DBExpression Result = new DBExpression();

            if (false == bIgnore)
            {
                Result._bAlways = true;
                Result._dbLogicalOperators = DBLogicalOperators.NORMAL;
                Result._bNOT = false;
                Result._FieldName = FieldName.ToUpper();
                Result._Operator = Operator;
                Result._Value1 = Value;

                if (Value is System.DateTime)
                {
                    if ((DBComparisonOperator.LessThanOrEqualTo == Operator) || (DBComparisonOperator.LessThan == Operator))
                    {
                        Result._Value1 = (System.DateTime)Value;
                    }
                }
                else if (Value is System.Enum)
                {
                    Result._Value1 = System.Convert.ChangeType(Value, System.Enum.GetUnderlyingType(Value.GetType()));
                }

            }

            return Result;
        }

        public static DBExpression Normal_Always(string FieldName, DBComparisonOperator Operator, object Value)
        {
            return DBExpression.Normal_Always(FieldName, Operator, Value, false);
        }

        #endregion

        #region From_CheckValueSet
        public static DBExpression From_CheckValueSet(string FieldName, SqlDbType xSqlDbType, CheckValueType Value, bool bIgnore)
        {
            DBExpression Result = new DBExpression();

            if ((false == bIgnore) && (Value != CheckValueType.None))
            {
                switch (xSqlDbType)
                {
                    case SqlDbType.Float:
                    case SqlDbType.Real:
                        if (CheckValueType.Set == Value)
                        {
                            DBExpression SQLExpTemp = new DBExpression();

                            SQLExpTemp._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp._bNOT = true;
                            SQLExpTemp._FieldName = FieldName.ToUpper();
                            SQLExpTemp._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp._Value1 = 0.0;
                            SQLExpTemp._bAlways = true;

                            DBCondition xDBCondition = DBExpression.IS_NULL(FieldName, true) & SQLExpTemp;

                            Result = new DBExpression(xDBCondition.ToString(true));
                        }
                        else if (CheckValueType.Not_Set == Value)
                        {
                            DBExpression SQLExpTemp = new DBExpression();

                            SQLExpTemp._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp._bNOT = false;
                            SQLExpTemp._FieldName = FieldName.ToUpper();
                            SQLExpTemp._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp._Value1 = 0.0;
                            SQLExpTemp._bAlways = true;

                            DBCondition xDBCondition = DBExpression.IS_NULL(FieldName) | SQLExpTemp;

                            Result = new DBExpression(xDBCondition.ToString(true));
                        }
                        break;

                    case SqlDbType.Int:
                    case SqlDbType.SmallInt:
                    case SqlDbType.TinyInt: // For Vital Sign ( relate with CheckBox na krub)
                        if (CheckValueType.Set == Value)
                        {
                            DBExpression SQLExpTemp = new DBExpression();

                            SQLExpTemp._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp._bNOT = true;
                            SQLExpTemp._FieldName = FieldName.ToUpper();
                            SQLExpTemp._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp._Value1 = 0;
                            SQLExpTemp._bAlways = true;

                            DBCondition xDBCondition = DBExpression.IS_NULL(FieldName, true) & SQLExpTemp;

                            Result = new DBExpression(xDBCondition.ToString(true));
                        }
                        else if (CheckValueType.Not_Set == Value)
                        {
                            DBExpression SQLExpTemp = new DBExpression();

                            SQLExpTemp._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp._bNOT = false;
                            SQLExpTemp._FieldName = FieldName.ToUpper();
                            SQLExpTemp._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp._Value1 = 0;
                            SQLExpTemp._bAlways = true;

                            DBCondition xDBCondition = DBExpression.IS_NULL(FieldName) | SQLExpTemp;

                            Result = new DBExpression(xDBCondition.ToString(true));
                        }
                        break;

                    case SqlDbType.DateTime:
                    case SqlDbType.Timestamp:
                    case SqlDbType.VarBinary:
                        if (CheckValueType.Set == Value)
                        {
                            DBExpression SQLExpTemp = new DBExpression();
                            SQLExpTemp._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp._bNOT = true;
                            SQLExpTemp._FieldName = FieldName.ToUpper();
                            SQLExpTemp._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp._Value1 = new DateTime(0);
                            SQLExpTemp._bAlways = true;

                            DBCondition xDBCondition = DBExpression.IS_NULL(FieldName, true);

                            Result = new DBExpression(xDBCondition.ToString(true));
                        }
                        else if (CheckValueType.Not_Set == Value)
                        {
                            DBExpression SQLExpTemp = new DBExpression();

                            SQLExpTemp._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp._bNOT = false;
                            SQLExpTemp._FieldName = FieldName.ToUpper();
                            SQLExpTemp._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp._Value1 = new DateTime(0);
                            SQLExpTemp._bAlways = true;

                            DBCondition xDBCondition = DBExpression.IS_NULL(FieldName);
                            Result = new DBExpression(xDBCondition.ToString(true));
                        }
                        break;

                    case SqlDbType.VarChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.NText:
                        if (CheckValueType.Set == Value)
                        {
                            DBExpression SQLExpTemp = new DBExpression();
                            DBExpression SQLExpTemp_Dot = new DBExpression();

                            SQLExpTemp._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp._bNOT = true;
                            SQLExpTemp._FieldName = FieldName.ToUpper();
                            SQLExpTemp._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp._Value1 = "";
                            SQLExpTemp._bAlways = true;

                            SQLExpTemp_Dot._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp_Dot._bNOT = true;
                            SQLExpTemp_Dot._FieldName = FieldName.ToUpper();
                            SQLExpTemp_Dot._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp_Dot._Value1 = ".";
                            SQLExpTemp_Dot._bAlways = true;

                            DBCondition xDBCondition = DBExpression.IS_NULL(FieldName, true) & SQLExpTemp & SQLExpTemp_Dot;

                            Result = new DBExpression(xDBCondition.ToString(true));
                        }
                        else if (CheckValueType.Not_Set == Value)
                        {
                            DBExpression SQLExpTemp = new DBExpression();
                            DBExpression SQLExpTemp_Dot = new DBExpression();

                            SQLExpTemp._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp._bNOT = false;
                            SQLExpTemp._FieldName = FieldName.ToUpper();
                            SQLExpTemp._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp._Value1 = "";
                            SQLExpTemp._bAlways = true;

                            SQLExpTemp_Dot._dbLogicalOperators = DBLogicalOperators.NORMAL;
                            SQLExpTemp_Dot._bNOT = false;
                            SQLExpTemp_Dot._FieldName = FieldName.ToUpper();
                            SQLExpTemp_Dot._Operator = DBComparisonOperator.EqualEver;
                            SQLExpTemp_Dot._Value1 = ".";
                            SQLExpTemp_Dot._bAlways = true;

                            DBCondition xDBCondition = DBExpression.IS_NULL(FieldName) | SQLExpTemp | SQLExpTemp_Dot;

                            Result = new DBExpression(xDBCondition.ToString(true));
                        }
                        break;
                }
            }

            return Result;
        }

        public static DBExpression From_CheckValueSet(string FieldName, SqlDbType xSqlDbType, CheckValueType Value)
        {
            return DBExpression.From_CheckValueSet(FieldName, xSqlDbType, Value, false);
        }

        #endregion

        #region DBExpression.IS_NULL
        public static DBExpression IS_NULL(string FieldName, bool bNot, bool bIgnore)
        {
            DBExpression Result = new DBExpression();

            if (false == bIgnore)
            {
                Result._dbLogicalOperators = DBLogicalOperators.IS_NULL;
                Result._bNOT = bNot;
                Result._FieldName = FieldName.ToUpper();
            }

            return Result;
        }

        public static DBExpression IS_NULL(string FieldName, bool bNot)
        {
            return DBExpression.IS_NULL(FieldName, bNot, false);
        }

        public static DBExpression IS_NULL(string FieldName)
        {
            return DBExpression.IS_NULL(FieldName, false, false);
        }

        #endregion

        #region DBExpression.INNER
        public static DBCondition INNER(string FieldName_Start, string FieldName_End, object Value, SqlDbType xSqlDbType, bool bNot, bool bIgnore)
        {
            Value = DBExpression.ScanEmptyValue(Value);

            if (null == Value) return new DBCondition();

            DBCondition[] xConditionList = new DBCondition[2];

            xConditionList[0] = DBExpression.From_CheckValueSet(FieldName_Start, xSqlDbType, CheckValueType.Not_Set) | DBExpression.Normal(FieldName_Start, DBComparisonOperator.LessThanOrEqualTo, Value);
            xConditionList[1] = DBExpression.From_CheckValueSet(FieldName_End, xSqlDbType, CheckValueType.Not_Set) | DBExpression.Normal(FieldName_End, DBComparisonOperator.GreaterOrEqualTo, Value);

            return DBCondition.ConcatStatement(xConditionList, DBBitwiseOperators.AND, true);
        }

        public static DBCondition INNER(string FieldName_Start, string FieldName_End, object Value, SqlDbType xSqlDbType, bool bNot)
        {
            return DBExpression.INNER(FieldName_Start, FieldName_End, Value, xSqlDbType, bNot, false);
        }

        public static DBCondition INNER(string FieldName_Start, string FieldName_End, object Value, SqlDbType xSqlDbType)
        {
            return DBExpression.INNER(FieldName_Start, FieldName_End, Value, xSqlDbType, false, false);
        }

        #endregion

        #region DBExpression.BETWEEN
        public static DBExpression BETWEEN(string FieldName, object Value1, object Value2, bool bNot, bool bIgnore, bool bForMemoryDB)
        {
            Value1 = DBExpression.ScanEmptyValue(Value1);
            Value2 = DBExpression.ScanEmptyValue(Value2);

            if (null != Value1)
            {
                if (null != Value2)
                {
                    Type xValue1_Type = Value1.GetType();
                    Type xValue2_Type = Value2.GetType();
                }
                else 
                {
                    return DBExpression.Normal(FieldName, ((bNot) ? DBComparisonOperator.LessThan : DBComparisonOperator.GreaterOrEqualTo), Value1, bIgnore, bForMemoryDB);
                    
                }
            }
            else
            {
                if (null != Value2) 
                {
                    if (Value2 is System.DateTime)
                    {
                        return DBExpression.Normal(FieldName, ((bNot) ? DBComparisonOperator.GreaterThan : DBComparisonOperator.LessThanOrEqualTo), Value2, bIgnore);
                    }
                    else
                    {
                        return DBExpression.Normal(FieldName, ((bNot) ? DBComparisonOperator.GreaterThan : DBComparisonOperator.LessThanOrEqualTo), Value2, bIgnore);
                    }
                }
                else
                {
                    return new DBExpression();
                }
            }

            DBExpression Result = new DBExpression();

            if (false == bIgnore)
            {
                if (Value1 is System.DateTime)
                {
                    if (true == bForMemoryDB)
                    {
                        DBExpression SQLExpTemp_Value1 = new DBExpression();
                        DBExpression SQLExpTemp_Value2 = new DBExpression();

                        SQLExpTemp_Value1._dbLogicalOperators = DBLogicalOperators.NORMAL;
                        SQLExpTemp_Value1._bNOT = bNot;
                        SQLExpTemp_Value1._FieldName = FieldName.ToUpper();
                        SQLExpTemp_Value1._Operator = DBComparisonOperator.GreaterOrEqualTo;
                        SQLExpTemp_Value1._Value1 = Value1;
                        SQLExpTemp_Value1._bAlways = true;

                        SQLExpTemp_Value2._dbLogicalOperators = DBLogicalOperators.NORMAL;
                        SQLExpTemp_Value2._bNOT = bNot;
                        SQLExpTemp_Value2._FieldName = FieldName.ToUpper();
                        SQLExpTemp_Value2._Operator = DBComparisonOperator.LessThanOrEqualTo;                        
                        SQLExpTemp_Value2._Value1 = (System.DateTime)Value2;
                        SQLExpTemp_Value2._bAlways = true;

                        DBCondition xDBCondition;

                        if (true == bNot)
                        {
                            xDBCondition = SQLExpTemp_Value1 | SQLExpTemp_Value2;
                        }
                        else
                        {
                            xDBCondition = SQLExpTemp_Value1 & SQLExpTemp_Value2;
                        }

                        return new DBExpression(xDBCondition.ToString(true));
                    }
                    else
                    {
                        Result._dbLogicalOperators = DBLogicalOperators.BETWEEN;
                        Result._bNOT = bNot;
                        Result._FieldName = FieldName.ToUpper();
                        Result._Value1 = Value1;
                        Result._Value2 = (System.DateTime)Value2;
                        Result.ClearEmptyValue();
                    }
                }
                else
                {
                    if (Value1 != Value2)
                    {
                        Result._dbLogicalOperators = DBLogicalOperators.BETWEEN;
                        Result._bNOT = bNot;
                        Result._FieldName = FieldName.ToUpper();
                        Result._Value1 = Value1;
                        Result._Value2 = Value2;
                        Result.ClearEmptyValue();
                    }
                    else
                    {
                        return DBExpression.Normal(FieldName.ToUpper(), ((bNot) ? DBComparisonOperator.NotEqualTo : DBComparisonOperator.EqualEver), Value1, bIgnore, bForMemoryDB);
                    }
                }
            }

            return Result;
        }

        public static DBExpression BETWEEN(string FieldName, object Value1, object Value2, bool bNot, bool bIgnore)
        {
            return DBExpression.BETWEEN(FieldName, Value1, Value2, bNot, bIgnore, false);
        }

        public static DBExpression BETWEEN(string FieldName, object Value1, object Value2, bool bNot)
        {
            return DBExpression.BETWEEN(FieldName, Value1, Value2, bNot, false, false);
        }

        public static DBExpression BETWEEN(string FieldName, object Value1, object Value2)
        {
            return DBExpression.BETWEEN(FieldName, Value1, Value2, false, false, false);
        }

        #endregion

        #region DBExpression.LIKE
        public static DBExpression LIKE(string FieldName, string Pattern, bool bNot, bool bIgnore)
        {
            DBExpression Result = new DBExpression();

            if (false == bIgnore)
            {
                Result._dbLogicalOperators = DBLogicalOperators.LIKE;
                Result._bNOT = bNot;
                Result._FieldName = FieldName.ToUpper();
                Result._Value1 = Pattern;
                Result.ClearEmptyValue();
            }

            return Result;
        }

        public static DBExpression LIKE(string FieldName, string Pattern, bool bNot)
        {
            return DBExpression.LIKE(FieldName, Pattern, bNot, false);
        }

        public static DBExpression LIKE(string FieldName, string Pattern)
        {
            return DBExpression.LIKE(FieldName, Pattern, false, false);
        }

        #endregion

        #region DBExpression.IN
        public static DBExpression IN(string FieldName, string SQLStatement, bool bNot, bool bIgnore)
        {
            DBExpression Result = new DBExpression();

            if (false == bIgnore)
            {
                Result._dbLogicalOperators = DBLogicalOperators.IN;
                Result._bNOT = bNot;
                Result._FieldName = FieldName.ToUpper();
                Result._Value1 = SQLStatement;
                Result.ClearEmptyValue();
            }

            return Result;
        }

        public static DBExpression IN(string FieldName, string SQLStatement, bool bNot)
        {
            return DBExpression.IN(FieldName, SQLStatement, bNot, false);
        }

        public static DBExpression IN(string FieldName, string SQLStatement)
        {
            return DBExpression.IN(FieldName, SQLStatement, false, false);
        }

        public static DBExpression IN(string FieldName, IList ListColl, bool bNot, bool bIgnore)
        {
            DBExpression Result = new DBExpression();
            if (ListColl.Count > 0)
            {
                if (false == bIgnore)
                {
                    Result._dbLogicalOperators = DBLogicalOperators.IN;
                    Result._bNOT = bNot;
                    Result._FieldName = FieldName.ToUpper();
                    Result._Value1 = ListColl;
                    Result.ClearEmptyValue();
                }
            }
            return Result;
        }

        public static DBExpression IN(string FieldName, IList ListColl, bool bNot)
        {
            return DBExpression.IN(FieldName, ListColl, bNot, false);
        }

        public static DBExpression IN(string FieldName, IList ListColl)
        {
            return DBExpression.IN(FieldName, ListColl, false, false);
        }
        #endregion

        #region DBExpression.CONTAINS
        public static DBExpression CONTAINS(string FieldName, string Pattern, bool bNot, bool bIgnore)
        {
            DBExpression Result = new DBExpression();

            if (false == bIgnore)
            {
                Result._dbLogicalOperators = DBLogicalOperators.CONTAINS;
                Result._bNOT = bNot;
                Result._FieldName = FieldName.ToUpper();
                Result._Value1 = Pattern;
                Result.ClearEmptyValue();
            }

            return Result;
        }

        public static DBExpression CONTAINS(string FieldName, string Pattern, bool bNot)
        {
            return DBExpression.CONTAINS(FieldName, Pattern, bNot, false);
        }

        public static DBExpression CONTAINS(string FieldName, string Pattern)
        {
            return DBExpression.CONTAINS(FieldName, Pattern, false, false);
        }

        #endregion

        #region DBExpression.CONTAINS_PREFIX
        public static DBExpression CONTAINS_PREFIX(string FieldName, string Pattern, bool bNot, bool bIgnore)
        {
            DBExpression Result = new DBExpression();

            if (false == bIgnore)
            {
                Result._dbLogicalOperators = DBLogicalOperators.CONTAINS_PREFIX;
                Result._bNOT = bNot;
                Result._FieldName = FieldName.ToUpper();
                Result._Value1 = Pattern;
                Result.ClearEmptyValue();
            }

            return Result;
        }

        public static DBExpression CONTAINS_PREFIX(string FieldName, string Pattern, bool bNot)
        {
            return DBExpression.CONTAINS_PREFIX(FieldName, Pattern, bNot, false);
        }

        public static DBExpression CONTAINS_PREFIX(string FieldName, string Pattern)
        {
            return DBExpression.CONTAINS_PREFIX(FieldName, Pattern, false, false);
        }

        #endregion

        #endregion
    }


}
