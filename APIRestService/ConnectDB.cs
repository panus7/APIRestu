using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Transactions;

using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace APIRestServiceRestaurant
{

    public class ConnectDB
    { 
        private string x_ConnString = string.Empty; 
         
        public ConnectDB(string strConnection)
        {
            x_ConnString = strConnection;
        }
         

        #region [Method Get Data]
         
        public DataTable GetDataTable(GetDataOptionParam xParam, out string strError)
        {
            DataTable tbReturn = new DataTable();
            strError = string.Empty;

            if (string.Empty == xParam.TableName)
            {
                strError = "Table Name Can't Empty.";
                return tbReturn;
            }

            StringCollection strColumnsSelectedColl = new StringCollection();
            if (xParam.ColumnsSelectedColl.Count > 0)
            {
                StringCollection MasterColumnCollSchemaTable = GetColumnCollSchemaTable(xParam.TableName);
                foreach (string strFiledName in xParam.ColumnsSelectedColl)
                {
                    if (MasterColumnCollSchemaTable.Contains(strFiledName.ToUpper()))
                    {
                        strColumnsSelectedColl.Add(strFiledName);
                    }
                }

                if (strColumnsSelectedColl.Count == 0)
                {
                    strError = "Not found column in this table!!" + xParam.TableName;
                    return tbReturn;
                }
            }
            else
            {
                foreach (string strFiledName in xParam.ColumnsSelectedColl)
                {
                    strColumnsSelectedColl.Add(strFiledName);
                }
            }

            xParam.Condition = xParam.Condition.Replace("0001-01-01T", "9999-01-01T");

            SqlConnection DBCon = new SqlConnection(x_ConnString);
            SqlCommand Command = new SqlCommand();
            SqlDataReader OrderReader = null;
            IAsyncResult AsyncResult;
            System.Threading.WaitHandle WHandle;

            try
            {
                string strCommandText = string.Format("SELECT * FROM {0} ", xParam.TableName);

                if (strColumnsSelectedColl.Count > 0)
                {
                    string strColSelected = string.Empty;
                    foreach (string strCol in strColumnsSelectedColl)
                    {
                        strColSelected += strCol + ",";
                    }

                    // xxx,yyy,
                    if (strColSelected.Length > 0)
                    {
                        strColSelected = strColSelected.Substring(0, strColSelected.Length - 1);
                    }

                    strCommandText = string.Format("SELECT {0} FROM {1} ", strColSelected, xParam.TableName);
                }

                if (string.Empty != xParam.Condition)
                {
                    strCommandText += " WHERE " + xParam.Condition;
                }

                //@@@@ Command.CommandText = strCommandText;
                Command.CommandText = strCommandText.ToUpper();
                Command.CommandType = CommandType.Text;
                Command.Connection = DBCon;

                DBCon.Open();

                //if (Class_Conn.MSSQLVerType.MSSQL2000 == x_MSSQLVerType)
                //{
                //    OrderReader = Command.ExecuteReader();
                //    tbReturn.Load(OrderReader);
                //    tbReturn.TableName = xParam.TableName.ToUpper(); 
                //    //@@@@
                //    foreach (DataColumn xCol in tbReturn.Columns)
                //    {
                //        if (!xParam.LimitMaxLenght) xCol.MaxLength = -1;
                //        xCol.ColumnName = xCol.ColumnName.ToUpper();
                //    } 
                //}
                //else
                {
                    AsyncResult = Command.BeginExecuteReader();
                    WHandle = AsyncResult.AsyncWaitHandle;

                    if (WHandle.WaitOne() == true)
                    {
                        OrderReader = Command.EndExecuteReader(AsyncResult);
                        tbReturn.Load(OrderReader);
                        tbReturn.TableName = xParam.TableName.ToUpper();

                        //@@@@
                        foreach (DataColumn xCol in tbReturn.Columns)
                        {
                            if (!xParam.LimitMaxLenght) xCol.MaxLength = -1;
                            xCol.ColumnName = xCol.ColumnName.ToUpper();
                        }

                    }
                }

                DataTable xDataTable_Key = GetTableInfoSchemaKey(xParam.TableName, out strError);
                StringCollection strPKColl = new StringCollection();

                List<DataColumn> ListOfPK = new List<DataColumn>();
                foreach (DataRow xRowInfo in xDataTable_Key.Rows)
                {
                    ListOfPK.Add(tbReturn.Columns[(string)xRowInfo["COLUMN_NAME"]]);
                }

                tbReturn.PrimaryKey = ListOfPK.ToArray();

            }
            catch (SqlException SQLEx)
            {
                strError += string.Format("(101) Table {0} Error: {1}", xParam.TableName, SQLEx.Message.ToString());
            }
            catch (Exception Ex)
            {
                strError += string.Format("(101) Table {0} Error: {1}", xParam.TableName, Ex.Message.ToString());
            }
            finally
            {
                if (null != OrderReader) OrderReader.Close();
                Command.Dispose();
                DBCon.Close();
                DBCon.Dispose();
            }

            if (xParam.ScanFillUp) Scan_FillUp(tbReturn);


            if (tbReturn.TableName == "DATAQUOTATION_HEADER")
            {
                if (tbReturn.Columns.Contains("CustomerPersonName"))
                {
                    foreach (DataRow row in tbReturn.Rows)
                    {
                        if (DBNull.Value != row["CustomerPersonName"])
                        {
                            row["CustomerPersonName"] = ((string)row["CustomerPersonName"]).Replace("^", "'");
                        }
                    }
                }
            }
            else if (tbReturn.TableName == "DATAQUOTATION_ITEM")
            {
                if (tbReturn.Columns.Contains("StockName"))
                {
                    foreach (DataRow row in tbReturn.Rows)
                    {
                        if (DBNull.Value != row["StockName"])
                        {
                            row["StockName"] = ((string)row["StockName"]).Replace("^", "'");
                        }
                    }
                }
            }


            return tbReturn;
        }
          
        public void Scan_FillUp(DataTable dataTable)
        {
            //foreach (DataColumn xColumn in dataTable.Columns)
            //{
            //     if(typeof(System.String) == xColumn.DataType)
            //     {
            //        if(xColumn.MaxLength < 100)
            //        {
            //            xColumn.MaxLength = 4000;
            //        }
            //     }
            //}

            ///---

            foreach (DataRow row in dataTable.Rows)
            {
                if (DataRowState.Deleted == row.RowState) continue;

                for (int nColCount = 0; nColCount < dataTable.Columns.Count; nColCount++)
                {
                    object objValue = row[nColCount];

                    if (objValue.GetType() == typeof(System.DBNull))
                    {
                        Type typeofItem = dataTable.Columns[nColCount].DataType;

                        if (typeofItem == typeof(System.String)) row[nColCount] = string.Empty;
                        else if (typeofItem == typeof(System.Byte[])) row[nColCount] = new byte[0];
                        else if (typeofItem == typeof(System.DateTime)) row[nColCount] = new DateTime(0);

                        else if (typeofItem == typeof(System.Double)) row[nColCount] = (System.Double)0.0;
                        else if (typeofItem == typeof(System.Single)) row[nColCount] = (System.Single)0.0;
                        else if (typeofItem == typeof(System.Int64)) row[nColCount] = (System.Int64)0;
                        else if (typeofItem == typeof(System.Int32)) row[nColCount] = (System.Int32)0;
                        else if (typeofItem == typeof(System.Int16)) row[nColCount] = (System.Int16)0;
                        else if (typeofItem == typeof(System.SByte)) row[nColCount] = (System.SByte)0;
                        else if (typeofItem == typeof(System.UInt64)) row[nColCount] = (System.UInt64)0;
                        else if (typeofItem == typeof(System.UInt32)) row[nColCount] = (System.UInt32)0;
                        else if (typeofItem == typeof(System.UInt16)) row[nColCount] = (System.UInt16)0;
                        else if (typeofItem == typeof(System.Byte)) row[nColCount] = (System.Byte)0;
                        else if (typeofItem == typeof(System.Decimal)) row[nColCount] = (System.Decimal)0;
                    }
                }
            }

            dataTable.AcceptChanges();
        }

        public DataTable ScanValueUpdate(DataTable dataTable)
        {
            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (DataRowState.Deleted == row.RowState ||
                        DataRowState.Unchanged == row.RowState) continue;

                    for (int nColCount = 0; nColCount < dataTable.Columns.Count; nColCount++)
                    {
                        object objValue = row[nColCount];

                        if (objValue.GetType() != typeof(System.DBNull))
                        {
                            Type typeofItem = dataTable.Columns[nColCount].DataType;

                            if (typeofItem == typeof(System.DateTime))
                            {
                                if (DxDate.DateTimeIsEmpty((DateTime)objValue))
                                    row[nColCount] = DBNull.Value;
                            }
                            else if (typeofItem == typeof(System.String))
                            {
                                if (string.IsNullOrEmpty(objValue.ToString()))
                                {
                                    row[nColCount] = null;
                                }
                            }
                            else if (typeofItem == typeof(System.Byte[]))
                            {
                                if (((Byte[])objValue).Length == 0)
                                {
                                    row[nColCount] = DBNull.Value; //Supamit
                                }

                            }
                        }
                    }
                }
                return dataTable;
            }
            catch (System.Exception eConvertEx)
            {
                throw new System.Exception("", eConvertEx);
            }
        }

        public DataRow ScanValueUpdate(DataRow row)
        {
            try
            {
                if (DataRowState.Deleted == row.RowState ||
                    DataRowState.Unchanged == row.RowState) return row;

                for (int nColCount = 0; nColCount < row.Table.Columns.Count; nColCount++)
                {
                    object objValue = row[nColCount];

                    if (objValue.GetType() != typeof(System.DBNull))
                    {
                        Type typeofItem = row.Table.Columns[nColCount].DataType;

                        if (typeofItem == typeof(System.DateTime))
                        {
                            if (DxDate.DateTimeIsEmpty((DateTime)objValue))
                                row[nColCount] = DBNull.Value;
                        }

                        if (typeofItem == typeof(System.String))
                        {
                            if (string.IsNullOrEmpty(objValue.ToString()))
                            {
                                row[nColCount] = null;
                            }
                        }
                    }
                }
                return row;
            }
            catch (System.Exception eConvertEx)
            {
                throw new System.Exception("", eConvertEx);
            }
        }

        public object GetValueData(string strTableName, string strFieldName, string strCondition, string strOrderByColumn, bool bDesc)
        {
            object objvalue = null;

            if (string.Empty == strTableName
                || string.Empty == strFieldName
                || string.Empty == strCondition)
            {
                return objvalue;
            }

            strCondition = strCondition.Replace("0001-01-01T", "9999-01-01T");
            SqlConnection DBCon = new SqlConnection(x_ConnString);
            SqlCommand Command = new SqlCommand();
            SqlDataReader OrderReader;
            try
            {
                string strCommandText = string.Format(" SELECT TOP(1) {0} FROM {1} ", strFieldName, strTableName);

                if (!string.IsNullOrEmpty(strCondition))
                {
                    strCommandText += " WHERE " + strCondition;
                }

                if (!string.IsNullOrEmpty(strOrderByColumn))
                {
                    strCommandText += " ORDER BY " + strOrderByColumn;
                    if (bDesc) strCommandText += " DESC ";
                }

                Command.CommandText = strCommandText;
                Command.CommandType = CommandType.Text;
                Command.Connection = DBCon;

                DBCon.Open();

                OrderReader = Command.ExecuteReader();
                while (OrderReader.Read())
                {
                    //str += String.Format("{0}", OrderReader[0]);
                    if (DBNull.Value != OrderReader[0])
                    {
                        objvalue = OrderReader[0];
                    }

                    break;
                }

            }
            catch (SqlException SQLEx)
            {
                //strError += string.Format("(105) Table {0} Error: {1}", strTableName, SQLEx.Message.ToString());
            }
            catch (Exception Ex)
            {
                //strError += string.Format("(105) Table {0} Error: {1}", strTableName, Ex.Message.ToString());
            }
            finally
            {
                DBCon.Close();
                DBCon.Dispose();
            }

            return objvalue;

        }


        #endregion [Method Get Data]

        #region [Method Update Data]

        public bool Transaction_UpdateDataSet(DataSet xDataSet, out string strError)
        {
            int iCountDataErrorAll = 0;
            strError = string.Empty;
            using (SqlConnection sqlConnection = new SqlConnection(x_ConnString))
            {
                sqlConnection.Open();
                using (SqlTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        foreach (DataTable xDataTableUpdate in xDataSet.Tables)
                        {
                            if (false == Transaction_UpdateDataTable(sqlConnection, trans, xDataTableUpdate, out strError))
                            {
                                iCountDataErrorAll++;
                            }
                        }
                    }
                    catch (InvalidOperationException Ex)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, Ex.Message.ToString());
                        throw Ex;
                    }
                    catch (SqlException SQLEx)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, SQLEx.Message.ToString());

                        throw SQLEx;
                    }
                    catch (Exception Ex)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, Ex.Message.ToString());

                        throw Ex;
                    }
                    finally
                    {
                        if (0 == iCountDataErrorAll)
                        {
                            trans.Commit();
                        }
                        sqlConnection.Close();
                        sqlConnection.Dispose();
                    }
                }
            }

            return (0 == iCountDataErrorAll);

        }

        public bool Transaction_UpdateDataSet(SqlConnection sqlConnection, SqlTransaction trans, DataSet xDataSet, out string strError)
        {
            int iCountDataAll = 0;
            int iCountDataCompleteAll = 0;
            int iCountDataErrorAll = 0;

            strError = string.Empty;

            foreach (DataTable xDataTableUpdate in xDataSet.Tables)
            {
                //ลบรายการที่เป็น delete ก่อน
                DeleteData(xDataTableUpdate, out strError);
                if (string.Empty != strError) return false;
                //xDataTableUpdate = ScanValueUpdate(xDataTableUpdate);

                ScanValueUpdate(xDataTableUpdate);

                //using (SqlConnection sqlConnection = new SqlConnection(x_ConnString))
                {
                    //sqlConnection.Open();
                    //using (SqlTransaction trans = sqlConnection.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand Command = new SqlCommand();
                            Command.Transaction = trans;
                            int iResult = 0;

                            foreach (DataRow xRowUpdate in xDataTableUpdate.Rows)
                            {
                                iCountDataAll++;
                                iResult = 0;
                                if (DataRowState.Added == xRowUpdate.RowState)
                                {
                                    Command.CommandText = CreateStringInsert(xRowUpdate);
                                    //Command.CommandTimeout =  = xTimeOut_Sec
                                }
                                else if (DataRowState.Modified == xRowUpdate.RowState)
                                {
                                    Command.CommandText = CreateStringUpdate(xRowUpdate, out strError);
                                    if (string.Empty != strError)
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                Command.CommandType = CommandType.Text;
                                Command.Connection = sqlConnection;

                                if (sqlConnection.State == ConnectionState.Closed) sqlConnection.Open();

                                iResult = Command.ExecuteNonQuery();

                                if (1 == iResult)
                                {
                                    iCountDataCompleteAll++;
                                }
                                else
                                {
                                    iCountDataErrorAll++;
                                }
                            }
                        }
                        catch (InvalidOperationException Ex)
                        {
                            //trans.Rollback();
                            iCountDataErrorAll++;
                            strError += string.Format("(202) Table {0} Error: {1}", xDataTableUpdate.TableName, Ex.Message.ToString());
                            throw Ex;
                        }
                        catch (SqlException SQLEx)
                        {
                            //trans.Rollback();
                            iCountDataErrorAll++;
                            strError += string.Format("(202) Table {0} Error: {1}", xDataTableUpdate.TableName, SQLEx.Message.ToString());

                            throw SQLEx;
                        }
                        catch (Exception Ex)
                        {
                            //trans.Rollback();
                            iCountDataErrorAll++;
                            strError += string.Format("(202) Table {0} Error: {1}", xDataTableUpdate.TableName, Ex.Message.ToString());

                            throw Ex;
                        }
                        finally
                        {
                            //if (0 == iCountDataErrorAll)
                            //{
                            //    trans.Commit();
                            //}
                            //sqlConnection.Close();
                            //sqlConnection.Dispose();
                        }
                    }
                }
            }
           
            return (string.Empty == strError);
        }

        public bool Transaction_UpdateDataTable(SqlConnection sqlConnection , SqlTransaction trans, DataTable xDataTableUpdate, out string strError)
        {
            int iCountDataAll = 0;
            int iCountDataCompleteAll = 0;
            int iCountDataErrorAll = 0;

            strError = string.Empty;
            //ลบรายการที่เป็น delete ก่อน
            DeleteData(xDataTableUpdate, out strError);
            if (string.Empty != strError) return false;
            xDataTableUpdate = ScanValueUpdate(xDataTableUpdate);

            //using (SqlConnection sqlConnection = new SqlConnection(x_ConnString))
            {
                //sqlConnection.Open();
                //using (SqlTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand Command = new SqlCommand();
                        Command.Transaction = trans;
                        int iResult = 0;

                        foreach (DataRow xRowUpdate in xDataTableUpdate.Rows)
                        {
                            iCountDataAll++;
                            iResult = 0;
                            if (DataRowState.Added == xRowUpdate.RowState)
                            {
                                Command.CommandText = CreateStringInsert(xRowUpdate);
                                //Command.CommandTimeout =  = xTimeOut_Sec
                            }
                            else if (DataRowState.Modified == xRowUpdate.RowState)
                            {
                                //Command.CommandText = CreateStringUpdate(xRowUpdate);
                                Command.CommandText = CreateStringUpdate(xRowUpdate, out strError);
                                if (string.Empty != strError)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            Command.CommandType = CommandType.Text;
                            Command.Connection = sqlConnection;

                            if (sqlConnection.State == ConnectionState.Closed) sqlConnection.Open();

                            iResult = Command.ExecuteNonQuery();
                            if (1 == iResult)
                            {
                                iCountDataCompleteAll++;
                            }
                            else
                            {
                                iCountDataErrorAll++;
                            }
                        }
                    }
                    catch (InvalidOperationException Ex)
                    {
                        //trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(202) Table {0} Error: {1}", xDataTableUpdate.TableName, Ex.Message.ToString());
                        throw Ex;
                    }
                    catch (SqlException SQLEx)
                    {
                        //trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(202) Table {0} Error: {1}", xDataTableUpdate.TableName, SQLEx.Message.ToString());

                        throw SQLEx;
                    }
                    catch (Exception Ex)
                    {
                        //trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(202) Table {0} Error: {1}", xDataTableUpdate.TableName, Ex.Message.ToString());

                        throw Ex;
                    }
                    finally
                    {
                        //if (0 == iCountDataErrorAll)
                        //{
                        //    trans.Commit();
                        //}
                        //sqlConnection.Close();
                        //sqlConnection.Dispose();
                   }
                }
            }
            return (string.Empty == strError);
        }

        public bool UpdateDataTable(DataRow xRowUpdate, out string strError)
        {
            strError = string.Empty;

            SqlConnection DBCon = new SqlConnection(x_ConnString);
            int iResult = 0;
            xRowUpdate = ScanValueUpdate(xRowUpdate);

            try
            {
                //if (Class_Conn.MSSQLVerType.MSSQL2000 == x_MSSQLVerType)
                //{
                //    SqlCommand Command = new SqlCommand();

                //    //foreach (DataColumn Colmn in xRowUpdate.Table.Columns)
                //    //{
                //    //    if ("System.DateTime" == xRowUpdate[Colmn.ColumnName].GetType().FullName)
                //    //    {
                //    //        DateTime dtEntry = (DateTime)xRowUpdate[Colmn.ColumnName];
                //    //        if (IsDateTimeEmpty(dtEntry))
                //    //        {
                //    //            xRowUpdate[Colmn.ColumnName] = System.DBNull.Value;
                //    //        }
                //    //    }
                //    //}

                //    if (DataRowState.Added == xRowUpdate.RowState)
                //    {
                //        Command.CommandText = CreateStringInsert(xRowUpdate);
                //    }
                //    else if (DataRowState.Modified == xRowUpdate.RowState)
                //    {
                //        //Command.CommandText = CreateStringUpdate(xRowUpdate);
                //        Command.CommandText = CreateStringUpdate(xRowUpdate, out strError);
                //        if (string.Empty != strError)
                //        {
                //            return false;
                //        }
                //    }
                //    else
                //    {
                //        return false;
                //    }

                //    Command.CommandType = CommandType.Text;
                //    Command.Connection = DBCon;
                //    DBCon.Open();
                //    iResult = Command.ExecuteNonQuery();
                //}
                //else
                {

                    SqlCommand Command = new SqlCommand();

                    IAsyncResult AsyncResult;
                    System.Threading.WaitHandle WHandle;
                    foreach (DataColumn Colmn in xRowUpdate.Table.Columns)
                    {
                        if ("System.DateTime" == xRowUpdate[Colmn.ColumnName].GetType().FullName)
                        {
                            DateTime dtEntry = (DateTime)xRowUpdate[Colmn.ColumnName];
                            if ( DxDate.DateTimeIsEmpty(dtEntry))
                            {
                                xRowUpdate[Colmn.ColumnName] = System.DBNull.Value;
                            }
                        }

                        //if ("System.DBNull" == xRowUpdate[Colmn.ColumnName].GetType().FullName)
                        //{
                        //    if ("System.Byte[]" == (Colmn.DataType).FullName)
                        //    {
                        //        Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.Image).Value = DBNull.Value;
                        //    }
                        //    else if ("System.Int64" == (Colmn.DataType).FullName |
                        //        "System.Int32" == (Colmn.DataType).FullName |
                        //        "System.Int16" == (Colmn.DataType).FullName)
                        //    {
                        //        Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.Int).Value = DBNull.Value;
                        //    }
                        //    else if ("System.Byte" == (Colmn.DataType).FullName)
                        //    {
                        //        Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.TinyInt).Value = DBNull.Value;
                        //    }
                        //    else if ("System.Double" == (Colmn.DataType).FullName)
                        //    {
                        //        Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.Float).Value = DBNull.Value;
                        //    }
                        //    else if ("System.DateTime" == (Colmn.DataType).FullName)
                        //    {
                        //        Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.DateTime).Value = DBNull.Value;
                        //    }
                        //}
                        //else
                        {
                            Command.Parameters.AddWithValue(string.Format("@{0}", Colmn.ColumnName), xRowUpdate[Colmn.ColumnName]);
                        }
                    }
                    Command.Parameters.AddWithValue("@ErrorText", string.Empty);
                    Command.Parameters["@ErrorText"].Direction = ParameterDirection.Output;

                    if (DataRowState.Added == xRowUpdate.RowState)
                    {
                        Command.CommandText = "SP_INSERT_" + xRowUpdate.Table.TableName;
                    }
                    else if (DataRowState.Modified == xRowUpdate.RowState)
                    {
                        Command.CommandText = "SP_UPDATE_" + xRowUpdate.Table.TableName;
                    }
                    else
                    {
                        return false;
                    }

                    Command.CommandType = CommandType.StoredProcedure;
                    Command.Connection = DBCon;

                    DBCon.Open();

                    //if (Class_Conn.MSSQLVerType.MSSQL2000 == x_MSSQLVerType)
                    //{
                    //    iResult = Command.ExecuteNonQuery();
                    //}
                    //else
                    {
                        AsyncResult = Command.BeginExecuteNonQuery();
                        WHandle = AsyncResult.AsyncWaitHandle;

                        if (WHandle.WaitOne() == true)
                        {
                            iResult = Command.EndExecuteNonQuery(AsyncResult);
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (SqlException SQLEx)
            {
                strError += string.Format("(203) Table {0} Error: {1}", xRowUpdate.Table.TableName, SQLEx.Message.ToString());
            }
            catch (Exception Ex)
            {
                strError += string.Format("(203) Table {0} Error: {1}", xRowUpdate.Table.TableName, Ex.Message.ToString());
            }
            finally
            {
                DBCon.Close();
                DBCon.Dispose();
            }

            return (string.Empty == strError);
        }

        public bool UpdateDataTable(DataTable xDataTableUpdate, out string strError)
        {
            int iCountDataAll = 0;
            int iCountDataCompleteAll = 0;
            int iCountDataErrorAll = 0;
            //bool bUpdateSP = false;

            //foreach (DataColumn xColmns in xDataTableUpdate.Columns)
            //{
            //    if ("System.Byte[]" == (xColmns.DataType).FullName)
            //    {
            //        bUpdateSP = true;
            //        break;
            //    }
            //}
            //if (true == bUpdateSP)
            //{
            //    return UpdateDataTableABC(xDataTableUpdate, out strError, out iCountDataAll, out iCountDataCompleteAll, out iCountDataErrorAll);
            //}
            //else
            //{
                return UpdateDataTable(xDataTableUpdate, out strError, out iCountDataAll, out iCountDataCompleteAll, out iCountDataErrorAll);
            //}
        }
         
        public bool UpdateDataTable(DataTable xDataTableUpdate, out string strError, out int iCountDataAll, out int iCountDataCompleteAll, out int iCountDataErrorAll)
        {
            iCountDataAll = 0;
            iCountDataCompleteAll = 0;
            iCountDataErrorAll = 0;

            strError = string.Empty;
            //ลบรายการที่เป็น delete ก่อน
            DeleteData(xDataTableUpdate, out strError);
            if (string.Empty != strError) return false;
            xDataTableUpdate = ScanValueUpdate(xDataTableUpdate);

            using (SqlConnection sqlConnection = new SqlConnection(x_ConnString))
            {
                sqlConnection.Open();
                using (SqlTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand Command = new SqlCommand();
                        Command.Transaction = trans;
                        int iResult = 0;

                        foreach (DataRow xRowUpdate in xDataTableUpdate.Rows)
                        {
                            iCountDataAll++;
                            iResult = 0;
                            if (DataRowState.Added == xRowUpdate.RowState)
                            {
                                Command.CommandText = CreateStringInsert(xRowUpdate);
                                //Command.CommandTimeout =  = xTimeOut_Sec
                            }
                            else if (DataRowState.Modified == xRowUpdate.RowState)
                            {
                                //Command.CommandText = CreateStringUpdate(xRowUpdate);
                                Command.CommandText = CreateStringUpdate(xRowUpdate, out strError);
                                if (string.Empty != strError)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                continue;
                            }

                            Command.CommandType = CommandType.Text;
                            Command.Connection = sqlConnection;

                            if (sqlConnection.State == ConnectionState.Closed) sqlConnection.Open();

                            iResult = Command.ExecuteNonQuery();
                            if (1 == iResult)
                            {
                                iCountDataCompleteAll++;
                            }
                            else
                            {
                                iCountDataErrorAll++;
                            }
                        }
                    }
                    catch (InvalidOperationException Ex)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(204) Table {0} Error: {1}", xDataTableUpdate.TableName, Ex.Message.ToString());
                    }
                    catch (SqlException SQLEx)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(204) Table {0} Error: {1}", xDataTableUpdate.TableName, SQLEx.Message.ToString());
                    }
                    catch (Exception Ex)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(204) Table {0} Error: {1}", xDataTableUpdate.TableName, Ex.Message.ToString());
                    }
                    finally
                    {
                        if (0 == iCountDataErrorAll)
                        {
                            trans.Commit();
                        }
                        sqlConnection.Close();
                        sqlConnection.Dispose();
                   }
                }
            }
            return (string.Empty == strError);
        }

        public bool UpdateDataTableABC(DataTable xDataTableUpdate, out string strError, out int iCountDataAll, out int iCountDataCompleteAll, out int iCountDataErrorAll)
        {
            iCountDataAll = 0;
            iCountDataCompleteAll = 0;
            iCountDataErrorAll = 0;

            strError = string.Empty;
            //ลบรายการที่เป็น delete ก่อน
            DeleteData(xDataTableUpdate, out strError);
            if (string.Empty != strError) return false;

            using (SqlConnection sqlConnection = new SqlConnection(x_ConnString))
            {
                sqlConnection.Open();
                using (SqlTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand Command = new SqlCommand();
                        Command.Transaction = trans;
                        int iResult = 0;

                        foreach (DataRow xRowUpdate in xDataTableUpdate.Rows)
                        {
                            iCountDataAll++;
                            foreach (DataColumn xColmns in xDataTableUpdate.Columns)
                            {
                                if (xColmns.DataType == typeof(System.String) & xRowUpdate[xColmns.ColumnName] is DBNull)
                                {
                                    xRowUpdate[xColmns.ColumnName] = string.Empty;
                                }
                            }
                            #region MSSQL2000
                            //if (Class_Conn.MSSQLVerType.MSSQL2000 == x_MSSQLVerType)
                            //{
                            //    iResult = 0;
                            //    if (DataRowState.Added == xRowUpdate.RowState)
                            //    {
                            //        Command.CommandText = CreateStringInsert(xRowUpdate);
                            //    }
                            //    else if (DataRowState.Modified == xRowUpdate.RowState)
                            //    {
                            //        //Command.CommandText = CreateStringUpdate(xRowUpdate);
                            //        Command.CommandText = CreateStringUpdate(xRowUpdate, out strError);
                            //        if (string.Empty != strError)
                            //        {
                            //            return false;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        continue;
                            //    }

                            //    Command.CommandType = CommandType.Text;
                            //    Command.Connection = sqlConnection;

                            //    if (sqlConnection.State == ConnectionState.Closed) sqlConnection.Open();

                            //    iResult = Command.ExecuteNonQuery();
                            //    if (1 == iResult)
                            //    {
                            //        iCountDataCompleteAll++;
                            //    }
                            //    else
                            //    {
                            //        iCountDataErrorAll++;
                            //    }
                            //}
                            #endregion

                            #region ELSE
                            //else
                            {
                                IAsyncResult AsyncResult;
                                System.Threading.WaitHandle WHandle;
                                #region LoopDataColumn
                                foreach (DataColumn Colmn in xRowUpdate.Table.Columns)
                                {
                                    if ("System.DateTime" == xRowUpdate[Colmn.ColumnName].GetType().FullName)
                                    {
                                        DateTime dtEntry = (DateTime)xRowUpdate[Colmn.ColumnName];
                                        if (DxDate.DateTimeIsEmpty(dtEntry))
                                        {
                                            xRowUpdate[Colmn.ColumnName] = System.DBNull.Value;
                                        }
                                    }

                                    if ("System.DBNull" == xRowUpdate[Colmn.ColumnName].GetType().FullName)
                                    {
                                        if ("System.Byte[]" == (Colmn.DataType).FullName)
                                        {
                                            Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.Image).Value = DBNull.Value;
                                        }
                                        else if ("System.Int64" == (Colmn.DataType).FullName |
                                            "System.Int32" == (Colmn.DataType).FullName |
                                            "System.Int16" == (Colmn.DataType).FullName)
                                        {
                                            Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.Int).Value = DBNull.Value;
                                        }
                                        else if ("System.Byte" == (Colmn.DataType).FullName)
                                        {
                                            Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.TinyInt).Value = DBNull.Value;
                                        }
                                        else if ("System.Double" == (Colmn.DataType).FullName)
                                        {
                                            Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.Float).Value = DBNull.Value;
                                        }
                                        else if ("System.DateTime" == (Colmn.DataType).FullName)
                                        {
                                            Command.Parameters.Add(string.Format("@{0}", Colmn.ColumnName), SqlDbType.DateTime).Value = DBNull.Value;
                                        }
                                    }
                                    else
                                    {
                                        Command.Parameters.AddWithValue(string.Format("@{0}", Colmn.ColumnName), xRowUpdate[Colmn.ColumnName]);
                                    }
                                }
                                #endregion
                                Command.Parameters.AddWithValue("@ErrorText", string.Empty);
                                Command.Parameters["@ErrorText"].Direction = ParameterDirection.Output;

                                if (DataRowState.Added == xRowUpdate.RowState)
                                {
                                    Command.CommandText = "SP_INSERT_" + xRowUpdate.Table.TableName;
                                }
                                else if (DataRowState.Modified == xRowUpdate.RowState)
                                {
                                    Command.CommandText = "SP_UPDATE_" + xRowUpdate.Table.TableName;
                                }
                                else
                                {
                                    continue;
                                }

                                Command.CommandType = CommandType.StoredProcedure;
                                Command.Connection = sqlConnection;
                                if (sqlConnection.State == ConnectionState.Closed) sqlConnection.Open();

                                AsyncResult = Command.BeginExecuteNonQuery();
                                WHandle = AsyncResult.AsyncWaitHandle;

                                if (WHandle.WaitOne() == true)
                                {
                                    iResult = Command.EndExecuteNonQuery(AsyncResult);
                                    if (1 == iResult)
                                    {
                                        iCountDataCompleteAll++;
                                    }
                                    else
                                    {
                                        iCountDataErrorAll++;
                                    }
                                }
                            }
                            #endregion
                        }

                    }
                    catch (SqlException SQLEx)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(205) Table {0} Error: {1}", xDataTableUpdate.TableName, SQLEx.Message.ToString());

                    }
                    catch (Exception Ex)
                    {
                        iCountDataErrorAll++;
                        strError += string.Format("(205) Table {0} Error: {1}", xDataTableUpdate.TableName, Ex.Message.ToString());

                    }
                    finally
                    {
                        trans.Commit();
                        sqlConnection.Close();
                        sqlConnection.Dispose();
                    }
                }
            }
           
            return (string.Empty == strError);
        }
        
        public bool UpdateDataTableNonSP(DataTable xDataTableUpdate, out string strError)
        {
            strError = string.Empty;

            //ลบรายการที่เป็น delete ก่อน
            SqlCommand Command = new SqlCommand();
            DeleteData(xDataTableUpdate, out strError);
            if (string.Empty != strError) return false;
            xDataTableUpdate = ScanValueUpdate(xDataTableUpdate);

            SqlConnection DBCon = new SqlConnection(x_ConnString);
            try
            {
                foreach (DataRow xRowUpdate in xDataTableUpdate.Rows)
                {
                    foreach (DataColumn xColmns in xDataTableUpdate.Columns)
                    {
                        if (xColmns.DataType == typeof(System.String) & xRowUpdate[xColmns.ColumnName] is DBNull)
                        {
                            xRowUpdate[xColmns.ColumnName] = string.Empty;
                        }
                    }

                    if (DataRowState.Added == xRowUpdate.RowState)
                    {
                        Command.CommandText = CreateStringInsert(xRowUpdate);
                    }
                    else if (DataRowState.Modified == xRowUpdate.RowState)
                    {
                        //Command.CommandText = CreateStringUpdate(xRowUpdate);
                        Command.CommandText = CreateStringUpdate(xRowUpdate, out strError);
                        if (string.Empty != strError)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    Command.CommandType = CommandType.Text;
                    Command.Connection = DBCon;
                    if (DBCon.State == ConnectionState.Closed)
                        DBCon.Open();
                    Command.ExecuteNonQuery();
                }
            }
            catch (SqlException SQLEx)
            {
                strError += string.Format("(206) Table {0} Error: {1}", xDataTableUpdate.TableName, SQLEx.Message.ToString());
            }
            catch (Exception Ex)
            {
                strError += string.Format("(206) Table {0} Error: {1}", xDataTableUpdate.TableName, Ex.Message.ToString());
            }
            finally
            {
                DBCon.Close();
                DBCon.Dispose();
            }

            return (string.Empty == strError);
        }
        
        private string CreateStringInsert(DataRow xRow)
        {
            string strTableName = xRow.Table.TableName;
            DataTable xDataTableUpdate = xRow.Table.Clone().Copy();
            string strReturn = string.Empty;
            
            //INSERT INTO tablename (Colname1,calname2)
            //VALUES ('xxxxv1','xxxxv2')

            strReturn = string.Format("INSERT INTO {0} (", strTableName);
            foreach (DataColumn xColmns in xDataTableUpdate.Columns)
            {
                strReturn += string.Format("{0},", xColmns.ColumnName);
            }
            strReturn = strReturn.Substring(0, strReturn.Length - 1);
            strReturn += string.Format(") ");

            strReturn += string.Format("VALUES (");
            foreach (DataColumn xColmns in xDataTableUpdate.Columns)
            {
                if (xColmns.AutoIncrement) continue;//TOM

                if ("System.DateTime" == xRow[xColmns.ColumnName].GetType().FullName)
                {
                    DateTime dtEntry = (DateTime)xRow[xColmns.ColumnName];
                    if (DxDate.DateTimeIsEmpty(dtEntry))
                    {
                        xRow[xColmns.ColumnName] = System.DBNull.Value;
                        strReturn += string.Format("{0},", "NULL");
                    }
                    else
                    {
                        strReturn += string.Format("'{0}',", GetDateTimeToStringDB((DateTime)xRow[xColmns.ColumnName]));
                    }
                }
                else if ("System.Byte[]" == xRow[xColmns.ColumnName].GetType().FullName)
                {
                    if (DBNull.Value == xRow[xColmns.ColumnName])
                    {
                        xRow[xColmns.ColumnName] = System.DBNull.Value;
                        strReturn += string.Format("{0},", "NULL");
                    }
                    else
                    {
                        byte[] arraytoinsert = (byte[])xRow[xColmns.ColumnName];
                        string strByte = "0x" + BitConverter.ToString(arraytoinsert).Replace("-", "");
                        strReturn += string.Format("{0},", strByte);
                    }
                }
                else
                {
                    if (typeof(System.DBNull) == xRow[xColmns.ColumnName].GetType())
                    {
                        strReturn += string.Format("{0},", "NULL");
                    }
                    else
                    {
                        string strValue = xRow[xColmns.ColumnName].ToString();
                        strValue = strValue.Replace("'", "^");
                        strValue = strValue.Replace('"', '^');

                        strReturn += string.Format("N'{0}',", strValue);
                    }
                }
            }

            strReturn = strReturn.Substring(0, strReturn.Length - 1);
            strReturn += string.Format(")");

            return strReturn;
        }

        private string CreateStringUpdate(DataRow xRow, out string strError)
        {
            strError = string.Empty;

            string strTableName = xRow.Table.TableName;
            DataTable xDataTableUpdate = xRow.Table.Clone().Copy();
            string strReturn = string.Empty;
            List<string> xListOfSPColumn = new List<string>();

            //UPDATE tablename
            //SET 
            //Colmns1 = @Values1,
            //Colmns2 = @Values2,
            //Colmns3 = @Values3,
            //WHERE PKColn1 = '@PKvalue1' AND PKColn2 = '@PKvalue2'

            strReturn = string.Format("UPDATE {0} SET ", strTableName);                       
            foreach (DataColumn xColmns in xDataTableUpdate.Columns)
            {
                if (xColmns.AutoIncrement) continue;//TOM

                if (typeof(System.DBNull) == xRow[xColmns.ColumnName].GetType())
                {
                    strReturn += string.Format("{0} = {1},", xColmns.ColumnName, "NULL");
                }
                else
                {
                    if ("System.DateTime" == xRow[xColmns.ColumnName].GetType().FullName)
                    {
                        DateTime dtEntry = (DateTime)xRow[xColmns.ColumnName];
                        if (DxDate.DateTimeIsEmpty(dtEntry))
                        {
                            xRow[xColmns.ColumnName] = System.DBNull.Value;
                            strReturn += string.Format("{0} = {1},", xColmns.ColumnName, "NULL");
                        }
                        else
                        {
                            strReturn += string.Format("{0} = '{1}',", xColmns.ColumnName, GetDateTimeToStringDB((DateTime)xRow[xColmns.ColumnName]));
                        }
                    }
                    else if ("System.Byte[]" == xRow[xColmns.ColumnName].GetType().FullName)
                    {
                        if (DBNull.Value == xRow[xColmns.ColumnName])
                        {
                            xRow[xColmns.ColumnName] = System.DBNull.Value;
                            strReturn += string.Format("{0} = {1},", xColmns.ColumnName, "NULL");
                        }
                        else
                        {
                            byte[] arraytoinsert = (byte[])xRow[xColmns.ColumnName];
                            string strByte = "0x" + BitConverter.ToString(arraytoinsert).Replace("-", "");
                            strReturn += string.Format("{0} = {1},", xColmns.ColumnName, strByte);
                        }                        
                    }
                    else
                    {
                        string strValue = xRow[xColmns.ColumnName].ToString();
                        strValue = strValue.Replace("'", "^");
                        strValue = strValue.Replace('"', '^');
                        strReturn += string.Format("{0} = N'{1}',", xColmns.ColumnName, strValue);
                    }
                }
            }

            strReturn = strReturn.Substring(0, strReturn.Length - 1);

            //string strError = string.Empty;
            //DataTable xDataTable_Info = GetTableInfoSchema(strTableName, out strError);
            //StringCollection strPKColl = new StringCollection();
            //string strFilter = "IS_NULLABLE = 'NO'";
            //foreach (DataRow xRowInfo in xDataTable_Info.Select(strFilter))
            //{
            //    strPKColl.Add((string)xRowInfo["COLUMN_NAME"]);
            //}
            DataTable xDataTable_Key = GetTableInfoSchemaKey(strTableName, out strError);
            StringCollection strPKColl = new StringCollection();
            foreach (DataRow xRowInfo in xDataTable_Key.Rows)
            {
                strPKColl.Add((string)xRowInfo["COLUMN_NAME"]);
            }

            if (strPKColl.Count == 0)
            {
                strError = "PK not found table " + strTableName;
                return string.Empty;
            }

            strReturn += " WHERE ";

            foreach (string strColmns in strPKColl)
            {
                if ("System.DateTime" == xRow[strColmns].GetType().FullName)
                {
                    strReturn += string.Format(" {0} = '{1}' AND", strColmns, GetDateTimeToStringDB((DateTime)xRow[strColmns]));
                }
                else
                {
                    string strValue = xRow[strColmns, DataRowVersion.Original].ToString();
                    strValue = strValue.Replace("'", "^");
                    strValue = strValue.Replace('"', '^');

                    strReturn += string.Format(" {0} = '{1}' AND", strColmns, strValue);
                }
            }
            strReturn = strReturn.Substring(0, strReturn.Length - 3);

            //if (0 != xListOfSPColumn.Count)
            //{
            //    foreach (string strSPColumnName in xListOfSPColumn)
            //    {
            //        StoreProcedure_Update(xDataRow, strSPColumnName, out strErrorMsg);
            //    }
            //}
            
            return strReturn;
        }

        public bool UpdateDataSetBulk(DataSet xDataSet, out string strError)
        {
            bool bForceDeleteOldData = false;
            return UpdateDataSetBulk(xDataSet, bForceDeleteOldData, out strError);
        }

        public bool UpdateDataSetBulk(DataSet xDataSet, bool bForceDeleteOldData, out string strError)
        {
            int iCountDataErrorAll = 0;
            strError = string.Empty;
            using (SqlConnection sqlConnection = new SqlConnection(x_ConnString))
            {
                sqlConnection.Open();
                using (SqlTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        foreach (DataTable xDataTableUpdate in xDataSet.Tables)
                        {
                            if (xDataTableUpdate.Rows.Count == 0)
                                continue;

                            if (false == UpdateDataTableBulk(sqlConnection, trans, ForceScanValueUpdate(xDataTableUpdate), bForceDeleteOldData, out strError))
                            {
                                iCountDataErrorAll++;
                            }
                        }
                    }
                    catch (InvalidOperationException Ex)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, Ex.Message.ToString());
                        throw Ex;
                    }
                    catch (SqlException SQLEx)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, SQLEx.Message.ToString());

                        throw SQLEx;
                    }
                    catch (Exception Ex)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, Ex.Message.ToString());

                        throw Ex;
                    }
                    finally
                    {
                        if (0 == iCountDataErrorAll)
                        {
                            trans.Commit();
                        }
                        sqlConnection.Close();
                        sqlConnection.Dispose();
                    }
                }
            }

            return (0 == iCountDataErrorAll);

        }

        public bool UpdateDataSetBulk(SqlConnection destinationConnection, SqlTransaction trans, DataSet xDataSet, bool bForceDeleteOldData, out string strError)
        {
            int iCountDataErrorAll = 0;
            strError = string.Empty;
            //using (SqlConnection sqlConnection = new SqlConnection(x_ConnString))
            {
                //sqlConnection.Open();
                //using (SqlTransaction trans = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        foreach (DataTable xDataTableUpdate in xDataSet.Tables)
                        {
                            if (xDataTableUpdate.Rows.Count == 0)
                                continue;

                            if (false == UpdateDataTableBulk(destinationConnection, trans, ForceScanValueUpdate(xDataTableUpdate), bForceDeleteOldData, out strError))
                            {
                                iCountDataErrorAll++;
                            }
                        }
                    }
                    catch (InvalidOperationException Ex)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, Ex.Message.ToString());
                        throw Ex;
                    }
                    catch (SqlException SQLEx)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, SQLEx.Message.ToString());

                        throw SQLEx;
                    }
                    catch (Exception Ex)
                    {
                        trans.Rollback();
                        iCountDataErrorAll++;
                        strError += string.Format("(201) DataSet {0} Error: {1}", xDataSet.DataSetName, Ex.Message.ToString());

                        throw Ex;
                    }
                    finally
                    {
                        //if (0 == iCountDataErrorAll)
                        //{
                        //    trans.Commit();
                        //}
                        //sqlConnection.Close();
                        //sqlConnection.Dispose();
                    }
                }
            }

            return (0 == iCountDataErrorAll);

        }


        private bool UpdateDataTableBulk(SqlConnection destinationConnection, SqlTransaction trans, DataTable xDataTableUpdate, out string strError)
        {
            bool forceDeleteOldData = false;
            return UpdateDataTableBulk(destinationConnection, trans, xDataTableUpdate, forceDeleteOldData, out strError);
        }

        private bool UpdateDataTableBulk(SqlConnection destinationConnection, SqlTransaction trans, DataTable xDataTableUpdate, bool forceDeleteOldData, out string strError)
        {
            strError = string.Empty;
            //using (SqlConnection destinationConnection = new SqlConnection(x_ConnString))
            {
                //destinationConnection.Open();                

                if(forceDeleteOldData)
                {
                    bool bOK = ForceDeleteDataTableWithTransaction(destinationConnection, trans, xDataTableUpdate, out strError);
                    if (!bOK) return bOK;
                }

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection, SqlBulkCopyOptions.Default, trans))
                {
                    bulkCopy.DestinationTableName = xDataTableUpdate.TableName;

                    try
                    {
                        bulkCopy.WriteToServer(xDataTableUpdate);
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;//Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        //reader.Close();
                    }
                }
            }

            return (string.Empty == strError);

        }

        public DataTable ForceScanValueUpdate(DataTable dataTable)
        {
            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (DataRowState.Deleted == row.RowState ||
                        DataRowState.Detached == row.RowState) continue;

                    for (int nColCount = 0; nColCount < dataTable.Columns.Count; nColCount++)
                    {
                        object objValue = row[nColCount];

                        if (objValue.GetType() != typeof(System.DBNull))
                        {
                            Type typeofItem = dataTable.Columns[nColCount].DataType;

                            if (typeofItem == typeof(System.DateTime))
                            {
                                if (DxDate.DateTimeIsEmpty((DateTime)objValue))
                                    row[nColCount] = DBNull.Value;
                            }
                            else if (typeofItem == typeof(System.String))
                            {
                                if (string.IsNullOrEmpty(objValue.ToString()))
                                {
                                    row[nColCount] = null;
                                }
                            }
                            else if (typeofItem == typeof(System.Byte[]))
                            {
                                if (((Byte[])objValue).Length == 0)
                                {
                                    row[nColCount] = DBNull.Value; //Supamit
                                }

                            }
                        }
                    }
                }
                return dataTable;
            }
            catch (System.Exception eConvertEx)
            {
                throw new System.Exception("", eConvertEx);
            }
        }
        #endregion [Method Update Data]

        #region [Method Delete Data]
         
        public bool DeleteData(DataTable tableDelete, out string strError)
        {
            bool bResult = true;
            strError = string.Empty;
            StringCollection xColPKArr = new StringCollection();

            DataTable xDataTable_Key = GetTableInfoSchemaKey(tableDelete.TableName, out strError);
            StringCollection strPKColl = new StringCollection();
            foreach (DataRow xRowInfo in xDataTable_Key.Rows)
            {
                xColPKArr.Add((string)xRowInfo["COLUMN_NAME"]);
            }

            DataTable xDataDelete = tableDelete.Clone();
            foreach (DataRow xRowDel in tableDelete.Rows)
            {
                if (DataRowState.Deleted != xRowDel.RowState) continue;
                xDataDelete.ImportRow(xRowDel);
            }

            foreach (DataRow xRowDel in xDataDelete.Rows)
            {
                if (DataRowState.Deleted != xRowDel.RowState) continue;
                ///
                xRowDel.RejectChanges();
                string SQLStmn = string.Empty;
                foreach (string ColumnName in xColPKArr)
                {

                    if ("System.DateTime" == xRowDel[ColumnName].GetType().FullName)
                    {
                        //2010-03-08 23:37:29.827
                        DateTime dt = (DateTime)xRowDel[ColumnName];
                        string strDate = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}.{6:D3}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                        SQLStmn += string.Format(" {0} = '{1}' AND", ColumnName, strDate);
                    }
                    else
                    {
                        SQLStmn += string.Format(" {0} = '{1}' AND", ColumnName, xRowDel[ColumnName]);
                    }
                }
                if (string.Empty != SQLStmn)
                {
                    if (SQLStmn.Length > 3)
                    {
                        //ตัด AND ตัวสุดท้าย
                        SQLStmn = SQLStmn.Substring(0, SQLStmn.Length - 3);
                    }

                    bResult = DeleteData(xDataDelete.TableName, SQLStmn, out strError);
                }
            }
            return bResult;
        }

        public bool DeleteData(string strTableName, string strCondition, out string strError)
        {
            int iResult = 0;
            strError = string.Empty;
            if (string.Empty == strTableName)
            {
                strError = "Table Name Can't Empty.";
                return false;
            }

            if (string.Empty == strCondition)
            {
                strError = "strCondition Can't Empty.";
                return false;
            }

            SqlConnection DBCon = new SqlConnection(x_ConnString);
            SqlCommand Command = new SqlCommand();
            IAsyncResult AsyncResult;
            System.Threading.WaitHandle WHandle;

            try
            {
                string strCommandText = string.Format("DELETE FROM {0} ", strTableName);
                if (string.Empty != strCondition)
                {
                    strCommandText += " WHERE " + strCondition;
                }

                DBCon = new SqlConnection(x_ConnString);
                Command.CommandText = strCommandText;
                Command.CommandType = CommandType.Text;
                Command.Connection = DBCon;

                DBCon.Open();

                //if (Class_Conn.MSSQLVerType.MSSQL2000 == x_MSSQLVerType)
                //{
                //    iResult = Command.ExecuteNonQuery();
                //}
                //else
                {
                    AsyncResult = Command.BeginExecuteNonQuery();
                    WHandle = AsyncResult.AsyncWaitHandle;

                    if (WHandle.WaitOne() == true)
                    {
                        iResult = Command.EndExecuteNonQuery(AsyncResult);
                    }
                    else
                    {

                    }
                }
            }
            catch (SqlException SQLEx)
            {
                strError += string.Format("(301) Table {0} Error: {1}", strTableName, SQLEx.Message.ToString());
            }
            catch (Exception Ex)
            {
                strError += string.Format("(301) Table {0} Error: {1}", strTableName, Ex.Message.ToString());
            }
            finally
            {
                DBCon.Close();
                DBCon.Dispose();
            }

            //return (iResult > 0);
            return (strError == string.Empty);

        }


        public bool ForceDeleteDataTableWithTransaction(SqlConnection sqlConnection, SqlTransaction transaction, DataTable tableDelete, out string strError)
        {
            bool bResult = true;
            strError = string.Empty;
            StringCollection xColPKArr = new StringCollection();

            DataTable tbDelete = tableDelete.Copy();
            tbDelete.AcceptChanges();

            DataTable xDataTable_Key = GetTableInfoSchemaKey(tbDelete.TableName, out strError);
            StringCollection strPKColl = new StringCollection();
            foreach (DataRow xRowInfo in xDataTable_Key.Rows)
            {
                xColPKArr.Add((string)xRowInfo["COLUMN_NAME"]);
            }

            string SQLStmn = string.Empty;
            foreach (DataRow xRowDel in tbDelete.Rows)
            {
                ///
                xRowDel.RejectChanges();                
                foreach (string ColumnName in xColPKArr)
                {

                    if ("System.DateTime" == xRowDel[ColumnName].GetType().FullName)
                    {
                        //2010-03-08 23:37:29.827
                        DateTime dt = (DateTime)xRowDel[ColumnName];
                        string strDate = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}.{6:D3}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                        SQLStmn += string.Format(" {0} = '{1}' AND", ColumnName, strDate);
                    }
                    else
                    {
                        SQLStmn += string.Format(" {0} = '{1}' AND", ColumnName, xRowDel[ColumnName]);
                    }
                }

                if (string.Empty != SQLStmn)
                {
                    if (SQLStmn.Length > 3)
                    {
                        //ตัด AND ตัวสุดท้าย
                        SQLStmn = SQLStmn.Substring(0, SQLStmn.Length - 3);
                    }

                    SQLStmn += string.Format(" OR");

                }
            }

            if (SQLStmn.Length > 2)
            {
                //ตัด OR ตัวสุดท้าย
                SQLStmn = SQLStmn.Substring(0, SQLStmn.Length - 2);
            }

            bResult = DeleteDataWithTransaction(sqlConnection, transaction, tableDelete.TableName, SQLStmn, out strError);

            return bResult;
        }


        public bool DeleteDataWithTransaction(SqlConnection sqlConnection,SqlTransaction transaction, DataTable tableDelete, out string strError)
        {
            bool bResult = true;
            strError = string.Empty;
            StringCollection xColPKArr = new StringCollection();

            DataTable xDataTable_Key = GetTableInfoSchemaKey(tableDelete.TableName, out strError);
            StringCollection strPKColl = new StringCollection();
            foreach (DataRow xRowInfo in xDataTable_Key.Rows)
            {
                xColPKArr.Add((string)xRowInfo["COLUMN_NAME"]);
            }

            DataTable xDataDelete = tableDelete.Clone();
            foreach (DataRow xRowDel in tableDelete.Rows)
            {
                if (DataRowState.Deleted != xRowDel.RowState) continue;
                xDataDelete.ImportRow(xRowDel);
            }

            foreach (DataRow xRowDel in xDataDelete.Rows)
            {
                if (DataRowState.Deleted != xRowDel.RowState) continue;
                ///
                xRowDel.RejectChanges();
                string SQLStmn = string.Empty;
                foreach (string ColumnName in xColPKArr)
                {

                    if ("System.DateTime" == xRowDel[ColumnName].GetType().FullName)
                    {
                        //2010-03-08 23:37:29.827
                        DateTime dt = (DateTime)xRowDel[ColumnName];
                        string strDate = string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}.{6:D3}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                        SQLStmn += string.Format(" {0} = '{1}' AND", ColumnName, strDate);
                    }
                    else
                    {
                        SQLStmn += string.Format(" {0} = '{1}' AND", ColumnName, xRowDel[ColumnName]);
                    }
                }
                if (string.Empty != SQLStmn)
                {
                    if (SQLStmn.Length > 3)
                    {
                        //ตัด AND ตัวสุดท้าย
                        SQLStmn = SQLStmn.Substring(0, SQLStmn.Length - 3);
                    }

                    bResult = DeleteDataWithTransaction(sqlConnection, transaction, xDataDelete.TableName, SQLStmn, out strError);
                }
            }
            return bResult;
        }

        public bool DeleteDataWithTransaction(SqlConnection sqlConnection,SqlTransaction transaction, string strTableName, string strCondition, out string strError)
        {
            int iResult = 0;
            strError = string.Empty;
            if (string.Empty == strTableName)
            {
                strError = "Table Name Can't Empty.";
                return false;
            }

            if (string.Empty == strCondition)
            {
                strError = "strCondition Can't Empty.";
                return false;
            }

            //SqlConnection DBCon = new SqlConnection(x_ConnString);
            SqlCommand Command = new SqlCommand();
            IAsyncResult AsyncResult;
            System.Threading.WaitHandle WHandle;

            try
            {
                string strCommandText = string.Format("DELETE FROM {0} ", strTableName);
                if (string.Empty != strCondition)
                {
                    strCommandText += " WHERE " + strCondition;
                }

                //DBCon = new SqlConnection(x_ConnString);
                Command.CommandText = strCommandText;
                Command.CommandType = CommandType.Text;
                Command.Connection = sqlConnection;

                if (null != transaction)//<@>
                {
                    Command.Transaction = transaction;
                }

                //sqlConnection.Open();

                //if (Class_Conn.MSSQLVerType.MSSQL2000 == x_MSSQLVerType)
                //{
                //    iResult = Command.ExecuteNonQuery();
                //}
                //else
                {
                    AsyncResult = Command.BeginExecuteNonQuery();
                    WHandle = AsyncResult.AsyncWaitHandle;

                    if (WHandle.WaitOne() == true)
                    {
                        iResult = Command.EndExecuteNonQuery(AsyncResult);
                    }
                    else
                    {

                    }
                }
            }
            catch (SqlException SQLEx)
            {
                strError += string.Format("(301) Table {0} Error: {1}", strTableName, SQLEx.Message.ToString());
            }
            catch (Exception Ex)
            {
                strError += string.Format("(301) Table {0} Error: {1}", strTableName, Ex.Message.ToString());
            }
            finally
            {
                //sqlConnection.Close();
                //sqlConnection.Dispose();
            }

            //return (iResult > 0);
            return (strError == string.Empty);
        }



        #endregion [Method Delete Data]

        #region [Method General]

        public StringCollection GetColumnCollSchemaTable(string strTBName)
        {
            StringCollection strColumnColl = new StringCollection();
            if (string.Empty == strTBName) return strColumnColl;

            string strError = string.Empty;
            DataTable DataTableTableInfoSchema = GetTableInfoSchema(strTBName, out strError);
            foreach(DataRow row in DataTableTableInfoSchema.Rows)
            {
                strColumnColl.Add((string)row["COLUMN_NAME"].ToString().ToUpper());
            }
            
            return strColumnColl;
        }

        public DataTable GetSchemaTable(string strTBName)
        {
            return GetSchemaTable(strTBName, false);
        }
        public DataTable GetSchemaTable(string strTBName, bool bWithPK)
        {
            DataTable dtResult = null;
            if (string.Empty == strTBName) return dtResult;

            SqlConnection DBCon = new SqlConnection(x_ConnString);
            SqlCommand xDBCommand = new SqlCommand();

            String strSQL = string.Format("SELECT * FROM {0}", strTBName);

            xDBCommand.CommandType = CommandType.Text;
            xDBCommand.CommandText = strSQL;
            xDBCommand.Connection = DBCon;

            DBCon.Close();
            DBCon.Open();
            SqlDataReader reader = null;

            try
            {
                DataTable schemaTable;
                    
                reader = ((SqlCommand)xDBCommand).ExecuteReader(CommandBehavior.SchemaOnly);
                schemaTable = reader.GetSchemaTable();
                dtResult = new DataTable(strTBName);

                if (null != schemaTable)
                {
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        DataColumn xDataColumn = new DataColumn((string)row["ColumnName"].ToString().ToUpper(), (Type)row["DataType"]);

                        if (typeof(System.DateTime) == (Type)row["DataType"]) xDataColumn.DateTimeMode = DataSetDateTime.Unspecified;
                        else if (typeof(System.String) == (Type)row["DataType"]) xDataColumn.MaxLength = (int)row["ColumnSize"];

                        xDataColumn.ExtendedProperties.Add("SqlDbType", (System.Data.SqlDbType)(int)row["ProviderType"]);

                        dtResult.Columns.Add(xDataColumn);
                    }
                }
                else
                {
                    dtResult.Columns.Add("RowsAffected", Type.GetType("System.Int32"));

                    DataRow row = dtResult.NewRow();

                    row[0] = reader.RecordsAffected;
                    dtResult.Rows.Add(row);
                }

                if (bWithPK)
                {
                    string strError = string.Empty;
                    DataTable xDataTable_Key = GetTableInfoSchemaKey(strTBName, out strError);
                    StringCollection strPKColl = new StringCollection();

                    List<DataColumn> ListOfPK = new List<DataColumn>();
                    foreach (DataRow xRowInfo in xDataTable_Key.Rows)
                    {
                        ListOfPK.Add(dtResult.Columns[(string)xRowInfo["COLUMN_NAME"]]);
                    }

                    dtResult.PrimaryKey = ListOfPK.ToArray();
                }
            }
            catch (SqlException)
            {
                  
            }
            catch (Exception)
            {
                    
            }
            finally
            {
                if (null != reader) reader.Close();
                xDBCommand.Dispose();
                DBCon.Close();
                DBCon.Dispose();
            }
           
            dtResult.RemotingFormat = SerializationFormat.Binary;
            dtResult.AcceptChanges();
            return dtResult;
        }

        private DataTable GetTable(SqlDataReader sqlReader)
        {
            DataTable schemaTable = sqlReader.GetSchemaTable();
            DataTable outputTable = new DataTable();
            DataColumn dcColumn;
            DataRow drRow;

            for (int i = 0; i < schemaTable.Rows.Count; i++)
            {
                dcColumn = new DataColumn();
                if (!outputTable.Columns.Contains(schemaTable.Rows[i]["ColumnName"].ToString()))
                {
                    dcColumn.ColumnName = schemaTable.Rows[i]["ColumnName"].ToString();
                    dcColumn.Unique = Convert.ToBoolean(schemaTable.Rows[i]["IsUnique"]);
                    dcColumn.AllowDBNull = Convert.ToBoolean(schemaTable.Rows[i]["AllowDBNull"]);
                    dcColumn.ReadOnly = Convert.ToBoolean(schemaTable.Rows[i]["IsReadOnly"]);
                    outputTable.Columns.Add(dcColumn);
                }
            }
            while (sqlReader.Read())
            {

                drRow = outputTable.NewRow();
                for (int i = 0; i < sqlReader.FieldCount; i++)
                {
                    drRow[i] = sqlReader.GetValue(i);
                }
                outputTable.Rows.Add(drRow);
            }

            return outputTable;
        }

        public StringCollection GetAllTableNameInDB(out string strError)
        {
            StringCollection strTableNameColl = new StringCollection();
            strError = string.Empty;
            DataTable tb = new DataTable("INFO");
            SqlConnection objConn = new SqlConnection(x_ConnString);
            SqlDataReader dtReader = null;
            SqlCommand objCmd = new SqlCommand();

            try
            {
                objConn = new SqlConnection(x_ConnString);
                objConn.Open();

                String strSQL = string.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES");
                objCmd = new SqlCommand(strSQL, objConn);
                dtReader = objCmd.ExecuteReader();

                tb = GetTable(dtReader);
                tb.TableName = "INFO";

                foreach (DataRow xRow in tb.Select())
                {
                    strTableNameColl.Add((string)xRow["Table_Name"]);
                }
            }
            catch (SqlException Sqlex)
            {
                strError += Sqlex.Message.ToString();
            }
            catch (Exception ex)
            {
                strError += ex.Message.ToString();
            }
            finally
            {
                if (null != dtReader) dtReader.Close();
                objCmd.Dispose();
                objConn.Close();
                objConn.Dispose();
            }
            return strTableNameColl;
        }

        public DataTable GetTableInfoSchema(string strTBName, out string strError)
        {
            strError = string.Empty;
            DataTable tb = new DataTable(strTBName);
            SqlConnection objConn = new SqlConnection(x_ConnString);
            SqlDataReader dtReader = null;
            SqlCommand objCmd = new SqlCommand();

            try
            {
                objConn = new SqlConnection(x_ConnString);
                objConn.Open();

                String strSQL = string.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", strTBName);
                objCmd = new SqlCommand(strSQL, objConn);
                dtReader = objCmd.ExecuteReader();

                tb = GetTable(dtReader);
                tb.TableName = strTBName.ToUpper();
            }
            catch (SqlException Sqlex)
            {
                strError += string.Format("(401) Table {0} Error: {1}", strTBName, Sqlex.Message.ToString());
            }
            catch (Exception ex)
            {
                strError += string.Format("(401) Table {0} Error: {1}", strTBName, ex.Message.ToString());
            }
            finally
            {
                if (null != dtReader) dtReader.Close();
                objCmd.Dispose();
                objConn.Close();
                objConn.Dispose();
            }
            return tb;
        }

        public DataTable GetTableInfoSchemaKey(string strTBName, out string strError)
        {
            strError = string.Empty;
            DataTable tb = new DataTable(strTBName);
            SqlConnection objConn = new SqlConnection(x_ConnString);
            SqlDataReader dtReader = null;
            SqlCommand objCmd = new SqlCommand();

            try
            {
                objConn.Open();

                String strSQL = string.Format("SELECT * FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{0}' ORDER BY ORDINAL_POSITION", strTBName);
                objCmd = new SqlCommand(strSQL, objConn);
                dtReader = objCmd.ExecuteReader();

                tb = GetTable(dtReader);
                tb.TableName = strTBName.ToUpper();

                dtReader.Close();
                dtReader.Dispose();
            }
            catch (SqlException Sqlex)
            {
                strError += string.Format("(403) Table {0} Error: {1}", strTBName, Sqlex.Message.ToString());
            }
            catch (Exception ex)
            {
                strError += string.Format("(403) Table {0} Error: {1}", strTBName, ex.Message.ToString());
            }
            finally
            {
                if (null != dtReader) dtReader.Close();
                objCmd.Dispose();
                objConn.Close();
                objConn.Dispose();
            }
            return tb;
        }
        //"yyyy-MM-ddT00:00:00"
        public string GetDateTimeToStringDB(DateTime dtToConvert)
        {
            string strYear = dtToConvert.Year.ToString();
            string sMonth = dtToConvert.Month.ToString();
            string sDay = dtToConvert.Day.ToString();
            string sHour = dtToConvert.Hour.ToString();
            string sMinute = dtToConvert.Minute.ToString();
            string sSecond = dtToConvert.Second.ToString();
            string sMillisecond = dtToConvert.Millisecond.ToString();
            //'2010-02-16 10:10:34.923'
            return string.Format("{0:D2}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}:{6:D2}", strYear, sMonth,sDay ,sHour,sMinute,sSecond,sMillisecond);
        }

        //public bool IsDateTimeEmpty(DateTime DateTimeEntry)
        //{
        //    if (null == DateTimeEntry) return true;
        //    if (DateTimeEntry == new DateTime()) return true;
        //    if (DateTimeEntry == new DateTime(0001, 1, 1)) return true;
        //    if (1900 == DateTimeEntry.Year && 1 == DateTimeEntry.Month && 1 == DateTimeEntry.Day) return true;
        //    if (0544 == DateTimeEntry.Year && 1 == DateTimeEntry.Month && 1 == DateTimeEntry.Day) return true;
        //    if (1 == DateTimeEntry.Year && 1 == DateTimeEntry.Month && 1 == DateTimeEntry.Day) return true;
        //    return false;
        //}

        public bool CommandDBWithTransaction(SqlConnection sqlConnection, SqlTransaction transaction, string strCommandText, out string strError)
        {
            int iResult = 0;
            strError = string.Empty;
            
            //SqlConnection DBCon = new SqlConnection(x_ConnString);
            SqlCommand Command = new SqlCommand();
            IAsyncResult AsyncResult;
            System.Threading.WaitHandle WHandle;

            try
            {
                //DBCon = new SqlConnection(x_ConnString);
                Command.CommandText = strCommandText;
                Command.CommandType = CommandType.Text;
                Command.Connection = sqlConnection;

                if (null != transaction)//<@>
                {
                    Command.Transaction = transaction;
                }

                //sqlConnection.Open();

                //if (Class_Conn.MSSQLVerType.MSSQL2000 == x_MSSQLVerType)
                //{
                //    iResult = Command.ExecuteNonQuery();
                //}
                //else
                {
                    AsyncResult = Command.BeginExecuteNonQuery();
                    WHandle = AsyncResult.AsyncWaitHandle;

                    if (WHandle.WaitOne() == true)
                    {
                        iResult = Command.EndExecuteNonQuery(AsyncResult);
                    }
                    else
                    {

                    }
                }
            }
            catch (SqlException SQLEx)
            {
                strError += string.Format("(301)  Error: {0}", SQLEx.Message.ToString());
            }
            catch (Exception Ex)
            {
                strError += string.Format("(301) Error: {0}", Ex.Message.ToString());
            }
            finally
            {
                //sqlConnection.Close();
                //sqlConnection.Dispose();
            }

            //return (iResult > 0);

            return (strError == string.Empty);
        }

        public bool CheckTableContainInDB(string strConnnection , string strTBName, out string strError)
        {
            strError = string.Empty;
            DataTable tb = new DataTable(strTBName);
            SqlConnection objConn = new SqlConnection(strConnnection);
            SqlDataReader dtReader = null;
            SqlCommand objCmd = new SqlCommand();
            int i = 0;
            try
            {
                objConn = new SqlConnection(strConnnection);
                objConn.Open();

                String strSQL = string.Format("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", strTBName);
                objCmd = new SqlCommand(strSQL, objConn);
                i = (Int32)objCmd.ExecuteScalar();
            }
            catch (SqlException Sqlex)
            {
                strError += string.Format("(401) Table {0} Error: {1}", strTBName, Sqlex.Message.ToString());
            }
            catch (Exception ex)
            {
                strError += string.Format("(401) Table {0} Error: {1}", strTBName, ex.Message.ToString());
            }
            finally
            {
                if (null != dtReader) dtReader.Close();
                objCmd.Dispose();
                objConn.Close();
                objConn.Dispose();
            }
            return (i > 0);
        }


        #endregion [Method General]

        #region [ClassParam]

        public class GetDataOptionParam
        {
            private string              x_TableName;
            private string              x_Condition;
            private StringCollection    x_ColumnsSelectedColl;
            private bool                x_ScanFillUp;
            private bool                x_LimitMaxLenght;

            public GetDataOptionParam()
            {
                x_TableName = string.Empty;
                x_Condition = string.Empty;
                x_ColumnsSelectedColl = new StringCollection();
                x_ScanFillUp = false;
                x_LimitMaxLenght = false;
             }

            public string TableName { get { return x_TableName; } set { x_TableName = value; } }
            public string Condition { get { return x_Condition; } set { x_Condition = value; } }
            public StringCollection ColumnsSelectedColl { get { return x_ColumnsSelectedColl; } set { x_ColumnsSelectedColl = value; } }
            public bool ScanFillUp { get { return x_ScanFillUp; } set { x_ScanFillUp = value; } }
            public bool LimitMaxLenght { get { return x_LimitMaxLenght; } set { x_LimitMaxLenght = value; } }

        }


        #endregion
    }
}
