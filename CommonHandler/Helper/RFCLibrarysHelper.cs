using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonHandler.Model;
using CommonHandler.Log;
using System.Reflection;
using ERPConnect;
using System.Data; 
using CommonHandler.Data;
using Oracle.ManagedDataAccess.Client;

namespace CommonHandler.Helper
{
    public class RFCLibrarysHelper
    {
        //日志类
        private LogHandler logHandler;
        private OracleHandler dbHandler;
        public RFCLibrarysHelper()
        {
            this.logHandler = new LogHandler(this.GetType().Name);
        }

        public RFCLibrarysHelper(string connectionString)
        {
            dbHandler = new OracleHandler(connectionString);
            this.logHandler = new LogHandler(this.GetType().Name);
        }
        #region 根据RFC配置信息，打开RFC链接
        /// <summary>
        /// 根据RFC配置信息，打开RFC链接
        /// </summary>
        /// <param name="Connection"></param>
        /// <returns></returns>
        public ReturnResult ERGetPConnection(ERPGetConnection Connection)
        {
            ReturnResult consap = new ReturnResult();
            R3Connection ConnectionSAP = new R3Connection();
            try
            {
                string connString = BuildConneciton(Connection);
                ConnectionSAP = new R3Connection(connString);

                if (!ConnectionSAP.Ping())
                {
                    ConnectionSAP.Open();
                    consap.Status = true;
                }
            }
            catch (Exception ex)
            {

                ConnectionSAP.Close();
                consap.Message = ex.Message;
                consap.Status = false;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call ERGetPConnection Fail: " + ex.Message);
            }
            consap.Anything = ConnectionSAP;
            return consap;
        }
        #endregion
        #region 获取需要的RFC配置信息
        /// <summary>
        /// 获取需要的RFC配置信息
        /// </summary>
        /// <returns></returns>
        private ERPGetConnection Get_RFC_ConfigInfo(string prg_Name)
        {
            ERPGetConnection vRFCName = new ERPGetConnection();
            vRFCName.USER = GetMWebS("RFC", "ERPCONN", "USER", prg_Name);
            vRFCName.PASSWD = GetMWebS("RFC", "ERPCONN", "PASSWD", prg_Name);
            vRFCName.LANG = GetMWebS("RFC", "ERPCONN", "LANG", prg_Name);
            vRFCName.CLIENT = GetMWebS("RFC", "ERPCONN", "CLIENT", prg_Name);
            vRFCName.ASHOST = GetMWebS("RFC", "ERPCONN", "ASHOST", prg_Name);//IP地址
            vRFCName.SYSNR = GetMWebS("RFC", "ERPCONN", "SYSNR", prg_Name);
            vRFCName.GROUP = GetMWebS("RFC", "ERPCONN", "GROUP", prg_Name);
            vRFCName.R3NAME = GetMWebS("RFC", "ERPCONN", "R3NAME", prg_Name);
            vRFCName.MSHOST = GetMWebS("RFC", "ERPCONN", "MSHOST", prg_Name);
            return vRFCName;
        }
        /// <summary>
        /// 根据关键字，查询配置关键信息
        /// </summary>
        /// <param name="VR_CLASS"></param>
        /// <param name="VR_ITEM"></param>
        /// <param name="VR_NAME"></param>
        /// <returns></returns>
        public string GetMWebS(string VR_CLASS, string VR_ITEM, string VR_NAME, string PRG_NAME)
        {
            string Value = "";
            ReturnResult execRes = new ReturnResult();
            execRes = Get_RFC_ConfigInfo_ByKey(VR_CLASS, VR_ITEM, VR_NAME, PRG_NAME);
            try
            {
                DataSet ds = (DataSet)execRes.Anything;
                Value = ds.Tables[0].Rows[0]["VR_VALUE"].ToString();
            }
            catch
            {
                Value = "";
            }
            return Value;
        }
        #endregion

        #region 获取RFC配置的【用户，IP等相关信息】
        /// <summary>
        /// 获取RFC配置的【用户，IP等相关信息】
        /// </summary>
        /// <param name="VR_CLASS"></param>
        /// <param name="VR_ITEM"></param>
        /// <param name="VR_NAME"></param>
        /// <returns></returns>
        public ReturnResult Get_RFC_ConfigInfo_ByKey(string VR_CLASS, string VR_ITEM, string VR_NAME, string PRG_NAME)
        {
            ReturnResult result = new ReturnResult();
            DBParameter dbParams = new DBParameter();

            try
            {
                dbParams.Clear();
                string sql = @"SELECT PI.VR_VALUE FROM SFIS1.C_PARAMETER_INI PI 
                           WHERE PI.PRG_NAME =:PRG_NAME AND PI.VR_CLASS = :VR_CLASS AND PI.VR_ITEM = :VR_ITEM AND PI.VR_NAME = :VR_NAME";

                dbParams.Add(":VR_CLASS", OracleDbType.Varchar2, VR_CLASS);
                dbParams.Add(":VR_ITEM", OracleDbType.Varchar2, VR_ITEM);
                dbParams.Add(":VR_NAME", OracleDbType.Varchar2, VR_NAME);
                dbParams.Add(":PRG_NAME", OracleDbType.Varchar2, PRG_NAME);
                result.Anything = dbHandler.ExecuteAsScalar<string>(sql, CommandType.Text, dbParams.GetParameters());
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_RFC_ConfigInfo_ByKey Fail: " + ex.Message);
            }
            return result;
        }
        #endregion

        #region 拼接语句
        /// <summary>
        /// 拼接语句
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string BuildConneciton(ERPGetConnection conn)
        {
            string connstr = string.Format(" USER={0} PASSWD={1} LANG={2} CLIENT={3} ASHOST={4} SYSNR={5} ",
                                             conn.USER, conn.PASSWD, conn.LANG, conn.CLIENT, conn.ASHOST, conn.SYSNR);
            return connstr;
        }
        #endregion

        #region 查询RFC数据（无查询参数）
        /// <summary>
        /// 查询RFC数据（无查询参数）
        /// </summary>
        /// <param name="ConnectionSAP">链接信息</param>
        /// <param name="interfaceFunName">接口名称</param>
        /// <returns></returns>
        public ReturnResult Get_RFC_Info_By_NoParam(ref DataTable dataTable_put, R3Connection ConnectionSAP, string interfaceFunName)
        {
            RFCTable rfcTable_ERR = new RFCTable();
            ReturnResult result = new ReturnResult();
            RFCFunction function = null;
            try
            {
                function = ConnectionSAP.CreateFunction(interfaceFunName);

                function.Execute();
                rfcTable_ERR = function.Tables["T_OUTTAB"];//out table
                dataTable_put = rfcTable_ERR.ToADOTable();
                ConnectionSAP.Close();
                result.Status = true;
                result.Message = "OK";
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_RFC_Info_By_NoParam Fail: " + ex.Message);
            }
            return result;
        }
        #endregion
        #region 根据参数查询RFC数据
        /// <summary>
        /// 根据参数查询RFC数据
        /// </summary>
        /// <param name="refStr">查询返回信息</param>
        /// <param name="ConnectionSAP">链接信息</param>
        /// <param name="interfaceFunName">接口名称</param>
        /// <param name="lnName">料号</param>
        /// <returns></returns>
        public ReturnResult Get_RFC_Info_By_Param(ref string refStr, R3Connection ConnectionSAP, string interfaceFunName, string lnName)
        {
            ReturnResult result = new ReturnResult();
            RFCFunction function = null;
            try
            {
                RFCTableColumnCollection ff = new RFCTableColumnCollection();
                function = ConnectionSAP.CreateFunction(interfaceFunName);
                function.Exports["MATNR"].ParamValue = lnName;//查询条件（料号？）
                function.Execute();
                refStr = function.Imports["NORMT"].ParamValue.ToString().Trim();//返回数据
                ConnectionSAP.Close();

                result.Status = true;
                result.Message = "OK";
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_RFC_Info_By_Param Fail: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 根据参数查询RFC数据
        /// </summary>
        /// <param name="refStr">查询返回信息</param>
        /// <param name="ConnectionSAP">链接信息</param>
        /// <param name="interfaceFunName">接口名称</param>
        /// <param name="tbParam">料号</param>
        /// <returns></returns>
        public ReturnResult Get_RFC_Info_By_Param(ref string rfcRes, R3Connection ConnectionSAP, string interfaceFunName, DataTable tbParam)
        {
            RFCTable rfcTable_ERR = new RFCTable();

            ReturnResult result = new ReturnResult();
            RFCFunction function = null;


            try
            {
                RFCTableColumnCollection ff = new RFCTableColumnCollection();
                function = ConnectionSAP.CreateFunction(interfaceFunName);
                RFCTable info_table_H = function.Tables["IT_ZPPT6017"];//in table
                foreach (DataRow row in tbParam.Rows)
                {
                    RFCStructure rfcStruc_H = info_table_H.AddRow();
                    rfcStruc_H["AUFNR"] = row[0];//MO 编号
                    rfcStruc_H["MO_NO"] = row[1];//MO 排序
                }
                function.Execute();

                //rfcTable_ERR = function.Tables["IT_ZPPT6017"];//out table
                //dataTable_put = rfcTable_ERR.ToADOTable();
                rfcRes = function.Imports["E_FLAG"].ParamValue.ToString().Trim();
                ConnectionSAP.Close();

                result.Status = true;
                result.Message = "OK";
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_RFC_Info_By_Param Fail: " + ex.Message);
            }
            return result;
        }

        #endregion

    }
}
