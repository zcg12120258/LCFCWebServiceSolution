using CommonHandler.Log;
using CommonHandler.Setting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using CommonHandler.Model;
using CommonHandler.Helper;
using Oracle.ManagedDataAccess.Client;

namespace CommonHandler.Data
{
    public class OracleHandler
    {
        #region 私有变量
        //数据库链接字符串
        private string connectionString = SystemSetting.ConnectionString;
        //日志类对象
        private LogHandler logHandler;
        #endregion

        #region 构造函数
        public OracleHandler()
        {
            this.logHandler = new LogHandler(this.GetType().Name);
        }
        public OracleHandler(string connectionString)
        {
            this.logHandler = new LogHandler(this.GetType().Name);
            this.connectionString = connectionString;
        }
        #endregion

        #region 属性

        /// <summary>
        /// 设置或获取连接数据库连接字符串
        /// </summary>
        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        #endregion

        #region 事务
        /// <summary>
        /// 返回事务对象
        /// </summary>
        /// <returns>事务对象</returns>
        public OracleTransaction BeginTransaction()
        {
            OracleConnection Conn = new OracleConnection(this.ConnectionString);
            Conn.Open();
            OracleTransaction oracleTransaction = Conn.BeginTransaction();
            return oracleTransaction;
        }
        /// <summary>
        /// 提交事务到数据库
        /// </summary>
        /// <param name="transaction"></param>
        public void CommitTransaction(OracleTransaction transaction)
        {
            try
            {
                transaction.Commit();
            }
            finally
            {
                if (transaction != null && transaction.Connection != null)
                {
                    transaction.Connection.Close();
                }
            }
        }
        
        /// <summary>
        ///回滚事务
        /// </summary>
        /// <param name="transaction"></param>
        public void RollbackTransaction(OracleTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            finally
            {
                if (transaction != null && transaction.Connection != null)
                {
                    transaction.Connection.Close();
                }
            }
        }
        #endregion

        #region 使用事务的方法
        /// <summary>
        /// 准备Command对象
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="connection">connection</param>
        /// <param name="transaction">Transaction</param>
        /// <param name="commandType">Command type</param>
        /// <param name="commandText">Command text</param>
        public static void PrepareCommand(OracleCommand command, OracleConnection connection, OracleTransaction transaction, CommandType commandType, string commandText)
        {
            //判断连接是否打开
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            command.Connection = connection;
            command.Transaction = transaction;
            command.CommandType = commandType;
            command.CommandText = commandText;
        }

        /// <summary>
        ///  执行ExecuteNonQuery
        /// </summary>
        /// <param name="transaction">transaction</param>
        /// <param name="commandType">Command type</param>
        /// <param name="commandText">Command text</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        /// <returns>ID</returns>
        public ReturnResult ExecuteNonQuery(OracleTransaction transaction, CommandType commandType, string commandText, List<OracleParameter> oraParamsCollection)
        {
            ReturnResult result = new ReturnResult();
            OracleParameter orcleOutputParameter=null;
            OracleCommand cmd = new OracleCommand();
            try
            {
                PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText);
                if (oraParamsCollection != null)
                {
                    foreach (OracleParameter current in oraParamsCollection)
                    {
                        cmd.Parameters.Add(current);
                        if (current.Direction.Equals(ParameterDirection.Output))
                        {
                            orcleOutputParameter = current;
                        }
                    }
                }
                cmd.ExecuteNonQuery();
                if (orcleOutputParameter == null)
                {
                    result.Message = "OK";
                    result.Anything = "OK";
                }
                else
                {
                    result.Message = "OK";
                    result.Anything = orcleOutputParameter.Value.ToString();
                }
                result.Status = true;
            }
            catch(Exception ex)
            {
                result.Message = "Execute:ExecuteNonQuery," + ex.Message;
                result.Status = false;
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteNonQuery Fail:" + ex.Message);
                
            }
            return result;
        }
        /// <summary>
        /// 事务执行多条sql语句
        /// KeyValuePair<int, KeyValuePair<string, List<OracleParameter>>>
        /// int：sql语句的位置，string：sql语句，List<OracleParameter>sql语句对应的参数集
        /// </summary>
        /// <param name="sqlList"></param>
        /// <returns></returns>
        public ReturnResult ExecuteNonQueryBatchSql(List< KeyValuePair<string, List<OracleParameter>>> sqlList)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                    OracleCommand cmd = new OracleCommand();
                    using (OracleTransaction trans = BeginTransaction())
                    {
                        try
                        {
                            //循环  
                            foreach (KeyValuePair<string, List<OracleParameter>> item in sqlList)
                            {
                                string cmdText = item.Key;
                                List<OracleParameter> cmdParms = item.Value;
                                PrepareCommand(cmd, trans.Connection, trans, CommandType.Text, cmdText);
                                if (cmdParms != null)
                                {
                                    foreach (OracleParameter current in cmdParms)
                                    {
                                        cmd.Parameters.Add(current);

                                    }
                                }
                                int val = cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                            CommitTransaction(trans);
                            result.Status=true;
                        }
                        catch (Exception ex)
                        {
                            RollbackTransaction(trans);
                            result.Status = false;
                            result.Message = ex.Message;
                            logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteNonQueryBatchSql Fail:" + ex.Message);
                        }
                    }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteNonQueryBatchSql Fail:" + ex.Message);
            }
            return result;
        }

        /// <summary>  
        /// 执行多条SQL语句，实现数据库事务。  
        /// </summary>  
        /// <param name="sqlLStringList">哈希表（key为哈希表顺序，value是OracleSQlParameter类型，sqlCommandText为sql语句，oracleParameter是该语句的List<OracleSqlParameter>）</param>  
        public ReturnResult ExecuteSqlTran(Hashtable sqlLStringList)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                OracleCommand cmd = new OracleCommand();
                using (OracleTransaction trans = BeginTransaction())
                {
                    try
                    {
                        //循环  
                        foreach (DictionaryEntry myDE in sqlLStringList)
                        {
                            string cmdText = ((OracleSQlParameter)myDE.Value).sqlCommandText;
                            List<OracleParameter> cmdParms = ((OracleSQlParameter)myDE.Value).oracleParameter;
                            PrepareCommand(cmd, trans.Connection, trans, CommandType.Text, cmdText);
                            if (cmdParms != null)
                            {
                                foreach (OracleParameter current in cmdParms)
                                {
                                    cmd.Parameters.Add(current);

                                }
                            }
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        CommitTransaction(trans);
                        result.Status = true;
                    }
                    catch (Exception ex)
                    {
                        RollbackTransaction(trans);
                        result.Status = false;
                        result.Message = ex.Message;
                        logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteSqlTran Fail:" + ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteSqlTran Fail:" + ex.Message);
            }
            return result;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 执行ExecuteNonQuery.
        /// </summary>
        /// <param name="stroredProcedureName">存储过程名称</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        public ReturnResult ExecuteNonQuerySp(string stroredProcedureName, List<OracleParameter> oraParamsCollection)
        {
            ReturnResult result = new ReturnResult();
            OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
            try
            {
                OracleCommand oracleCommand = oraConnection.CreateCommand();
                oracleCommand.CommandText = stroredProcedureName;
                oracleCommand.CommandType = CommandType.StoredProcedure;
                //Loop for Paramets
                foreach (var current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                //End of for loop

                if (oraConnection.State != ConnectionState.Open)
                {
                    oraConnection.Open();
                }
                oracleCommand.ExecuteNonQuery();
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Message = "Execute:ExecuteNonQuerySp," + ex.Message;
                result.Status = false;
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteNonQuerySp Fail:" + ex.Message);
            }
            finally
            {
                oraConnection.Close();
            }
            return result;
        }

        /// <summary>
        /// 执行ExecuteNonQuery.
        /// </summary>
        /// <param name="commandText">sql语句</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        public ReturnResult ExecuteNonQuery(string commandText, List<OracleParameter> oraParamsCollection)
        {
            ReturnResult result = new ReturnResult();
            OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
            try
            {
                OracleCommand oracleCommand = oraConnection.CreateCommand();
                oracleCommand.CommandText = commandText;
                oracleCommand.CommandType = CommandType.Text;
                //Loop for Paramets
                foreach (var current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                //End of for loop

                if (oraConnection.State != ConnectionState.Open)
                {
                    oraConnection.Open();
                }
                oracleCommand.ExecuteNonQuery();
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Message = "Execute:ExecuteNonQuery," + ex.Message;
                result.Status = false;
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteNonQuery Fail:" + ex.Message);
            }
            finally
            {
                oraConnection.Close();
            }
            return result;
        }

        /// <summary>
        /// 执行存储过程返回DataSet
        /// </summary>
        /// <param name="stroredProcedureName">存储过程名称</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        /// <returns></returns>
        public DataSet ExecuteAsDataSetSp(string stroredProcedureName, List<OracleParameter> oraParamsCollection)
        {
            DataSet ds = new DataSet();
            OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
            try
            {
                OracleCommand oracleCommand = oraConnection.CreateCommand();
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter();
                oracleCommand.CommandText = stroredProcedureName;
                oracleCommand.CommandType = CommandType.StoredProcedure;

                //Loop for Paramets
                foreach (OracleParameter current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                //End of for loop

                oracleDataAdapter.SelectCommand = oracleCommand;
                if (oraConnection.State != ConnectionState.Open)
                {
                    oraConnection.Open();
                }
                oracleDataAdapter.Fill(ds);
                oraConnection.Close();

            }
            catch (Exception ex)
            {
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteAsDataSetSp Fail:" + ex.Message);
                throw ex;
            }
            finally
            {
                oraConnection.Close();
            }
            return ds;
        }

        /// <summary>
        /// 执行Sql文本返回DataSet
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        /// <returns></returns>
        public DataSet ExecuteAsDataSet(string commandText, List<OracleParameter> oraParamsCollection)
        {
            DataSet dataSet = new DataSet();
            OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
            try
            {
                if (oraConnection.State != ConnectionState.Open)
                {
                    oraConnection.Open();
                }
                OracleCommand oracleCommand = oraConnection.CreateCommand();
                oracleCommand.CommandType = CommandType.Text;
                oracleCommand.CommandText = commandText;

                foreach (OracleParameter current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(oracleCommand);
                oracleDataAdapter.Fill(dataSet);
                oraConnection.Close();
            }
            catch (Exception ex)
            {
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteAsDataSet Fail:" + ex.Message);
                throw ex;
            }
            finally
            {
                oraConnection.Close();
            }
            return dataSet;
        }

        /// <summary>
        /// 执行存储过程文本返回DataReader
        /// </summary>
        /// <param name="stroredProcedureName">存储过程名称</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        /// <returns></returns>
        public OracleDataReader ExecuteAsDataReaderSp(string stroredProcedureName, List<OracleParameter> oraParamsCollection)
        {
            try
            {
                OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oraConnection;
                oracleCommand.CommandText = stroredProcedureName;
                oracleCommand.CommandType = CommandType.StoredProcedure;
                //Loop for Paramets
                foreach (var current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                //End of for loop
                oraConnection.Open();
                OracleDataReader oracleReader = oracleCommand.ExecuteReader(CommandBehavior.CloseConnection);
                return oracleReader;
            }
            catch (Exception e)
            {
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteAsDataReaderSp Fail:" + e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 执行sql文本返回 DataReader
        /// </summary>
        /// <param name="commandText">SQL语句</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        /// <returns></returns>
        public OracleDataReader ExecuteAsDataReader(string commandText, List<OracleParameter> oraParamsCollection)
        {
            try
            {
                OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
                OracleCommand oracleCommand = new OracleCommand();
                oracleCommand.Connection = oraConnection;
                oracleCommand.CommandText = commandText;
                oracleCommand.CommandType = CommandType.Text;
                //Loop for Paramets
                foreach (var current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                //End of for loop
                oraConnection.Open();
                OracleDataReader oracleReader = oracleCommand.ExecuteReader(CommandBehavior.CloseConnection);
                return oracleReader;
            }
            catch (Exception e)
            {
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteAsDataReader Fail:" + e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 执行Sql语句或存储过程返回一个对象
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="commandText">存储过程或Sql语句</param>
        /// <param name="commdType">CommandType</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        /// <returns></returns>
        public T ExecuteAsObject<T>(string commandText,CommandType commdType, List<OracleParameter> oraParamsCollection)
        {
            OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
            try
            {

                OracleDataReader oracleReader;
             
                OracleCommand oracleCommand = oraConnection.CreateCommand();
                oracleCommand.CommandText = commandText;
                oracleCommand.CommandType = commdType;
                //Loop for Parameters
                foreach (var current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                //End of for loop
                 if (oraConnection.State != ConnectionState.Open)
                {
                    oraConnection.Open();
                }
                 oracleReader = oracleCommand.ExecuteReader(CommandBehavior.CloseConnection);
                 ArrayList arrColl = DataSourceConvertHelper.FillCollection(oracleReader, typeof(T));
                oraConnection.Close();
                if (oracleReader != null)
                {
                    oracleReader.Close();
                }
                if (arrColl != null && arrColl.Count > 0)
                {
                    return (T)arrColl[0];
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception e)
            {
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteAsObject<T> Fail:" + e.Message);
                throw e;
            }
            finally
            {
                oraConnection.Close();
            }
        }

        /// <summary>
        /// 执行Sql语句或存储过程反应一个对象列表
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="commandText">存储过程或Sql语句</param>
        /// <param name="commdType">CommandType</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        /// <returns>返回对象列表.</returns>
        public List<T> ExecuteAsList<T>(string commandText,CommandType commdType, List<OracleParameter> oraParamsCollection)
        {
            OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
            try
            {
                OracleDataReader oracleReader;
                OracleCommand oracleCommand = oraConnection.CreateCommand();
                oracleCommand.CommandText = commandText;
                oracleCommand.CommandType = commdType;
                //Loop for Paramets
                foreach (var current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                //End of for loop
                if (oraConnection.State != ConnectionState.Open)
                {
                    oraConnection.Open();
                }
                oracleReader = oracleCommand.ExecuteReader(CommandBehavior.CloseConnection); //datareader automatically closes the SQL connection
                List<T> mList = new List<T>();
                mList = DataSourceConvertHelper.FillCollection<T>(oracleReader);
                if (oracleReader != null)
                {
                    oracleReader.Close();
                }
                oraConnection.Close();
                return mList;
            }

            catch (Exception e)
            {
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteAsList<T> Fail:" + e.Message);
                throw e;
            }
            finally
            {
                oraConnection.Close();
            }
        }

        /// <summary>
        /// 执行Executescalar .
        /// </summary>
        /// <typeparam name="T"> 泛型</typeparam>
        /// <param name="commandText">存储过程或者SQL语句</param>
        /// <param name="commandType">CommandType</param>
        /// <param name="oraParamsCollection">键值对的参数列表</param>
        /// <returns></returns>
        public T ExecuteAsScalar<T>(string commandText, CommandType commandType, List<OracleParameter> oraParamsCollection)
        {
            OracleConnection oraConnection = new OracleConnection(this.ConnectionString);
            try
            {
                OracleCommand oracleCommand = oraConnection.CreateCommand();
                oracleCommand.CommandText = commandText;
                oracleCommand.CommandType = commandType;
                //Loop for Paramets
                foreach (var current in oraParamsCollection)
                {
                    oracleCommand.Parameters.Add(current);
                }
                //End of for loop
                if (oraConnection.State != ConnectionState.Open)
                {
                    oraConnection.Open();
                }
                return (T)oracleCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                logHandler.Error(MethodBase.GetCurrentMethod().Name + "Call ExecuteAsScalar<T> Fail:" + e.Message);
                throw e;
            }
            finally
            {
                oraConnection.Close();
            }
        }

        #endregion
    }
}
