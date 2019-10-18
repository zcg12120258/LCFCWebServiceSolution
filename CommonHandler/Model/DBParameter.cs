using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace CommonHandler.Model
{
    public class DBParameter
    {
        private List<OracleParameter> oraParams;
        public DBParameter()
        {
            this.oraParams = new List<OracleParameter>();
        }
        /// <summary>
        /// 创建一参数
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="oraType">参数类型</param>
        /// <param name="paramValue">参数值</param>
        public void Add(string paramName,OracleDbType oraType, object paramValue)
        {
            OracleParameter oracleParameter = new OracleParameter();
            oracleParameter.OracleDbType = oraType;
            oracleParameter.ParameterName = paramName;
            oracleParameter.Value = paramValue;
            this.oraParams.Add(oracleParameter);
        }

        /// <summary>
        /// 创建OutPut参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="oraType"></param>
        public void Add(string paramName, OracleDbType oraType)
        {
            OracleParameter oracleParameter = new OracleParameter();
            oracleParameter.OracleDbType = oraType;
            oracleParameter.ParameterName = paramName;
            oracleParameter.Direction = ParameterDirection.Output;
            oracleParameter.Size = 200;
            this.oraParams.Add(oracleParameter);
        }
        /// <summary>
        /// 获取参数列表值
        /// </summary>
        /// <returns></returns>
        public List<OracleParameter> GetParameters()
        {
            return this.oraParams;
        }
        /// <summary>
        /// 清除参数列表
        /// </summary>
        public void Clear()
        {
            this.oraParams.Clear();
        }
    }

    /// <summary>
    /// sql语句和参数列表类
    /// </summary>
    public class OracleSQlParameter
    {
        /// <summary>
        /// sql语句
        /// </summary>
        public string sqlCommandText;
        /// <summary>
        /// Sql语句的参数列表
        /// </summary>
        public List<OracleParameter> oracleParameter;
    }
}
