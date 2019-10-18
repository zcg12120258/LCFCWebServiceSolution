using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommonHandler.Model;
using System.Reflection;
using System.Data;
using KittingToLineWebService.Beans;
using CommonHandler.Data;
using CommonHandler.Log;
using Oracle.ManagedDataAccess.Client;

namespace KittingToLineWebService.DataGateway
{
    public class DataProviderService
    {
        private OracleHandler dbHander;
        private LogHandler logHandler;

        public DataProviderService(string connectionString)
        {
            dbHander = new OracleHandler(connectionString);
            logHandler = new LogHandler(this.GetType().Name);
        }

        #region 从SAP获取Lot号，更新R_WIP_LOT_MO_LOG_T表的Lot号、插入Lot号和Lot_Seq数据到r_wip_lot_tracking_t表
       
        /// <summary>
        /// 根据工单号获取Log表数据
        /// </summary>
        /// <param name="moorders"></param>
        /// <returns></returns>
        public List<LotMoEntity> GetMoEntityListByMoOrders(string moOrders)
        {
            List<LotMoEntity> lotMoEntityList = new List<LotMoEntity>();
            string strSql = string.Format(@"SELECT LM.LOT_NUMBER LOT_NO,LM.LINE_NAME,LM.MO MO_ORDER,LM.MO_SEQ MO_NO,TO_CHAR(LM.MO_START_TIME,'YYYY/MM/dd') CREATE_TIME,LM.SHIFT, '0' LOT_SEQ,JOB_STATUS FROM SFISM4.R_WIP_LOT_MO_LOG_T LM WHERE LM.MO IN ('{0}')", moOrders);
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();
            try
            {

                lotMoEntityList = dbHander.ExecuteAsList<LotMoEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call GetMoEntityListByMoOrders Fail: " + ex.Message);
            }
            return lotMoEntityList;
        }

        /// <summary>
        /// 批量插入和更新
        /// </summary>
        /// <param name="lotEntityList"></param>
        /// <param name="insertlotEntityList"></param>
        /// <returns></returns>
        public ReturnResult UpdateLogAndInsertTracking(List<LotMoEntity> lotEntityList, List<LotMoEntity> insertlotEntityList)
        {
            ReturnResult result = new ReturnResult();// {  Status=false};
            result = DeleteTracking(lotEntityList);
            if (result.Status)
            {
                List<KeyValuePair<string, List<OracleParameter>>> strSqlList = new List<KeyValuePair<string, List<OracleParameter>>>();
                string strUpdate = string.Format(@"UPDATE SFISM4.R_WIP_LOT_MO_LOG_T SET LOT_NUMBER=:LOT_NO,JOB_STATUS='2' WHERE MO=:MO_ORDER");
                foreach (var entity in lotEntityList)
                {
                    DBParameter dbParameter = new DBParameter();
                    dbParameter.Clear();
                    dbParameter.Add(":LOT_NO", OracleDbType.Varchar2, entity.LOT_NO);
                    dbParameter.Add(":MO_ORDER", OracleDbType.Varchar2, entity.MO_ORDER);
                   
                    strSqlList.Add(new KeyValuePair<string, List<OracleParameter>>(strUpdate, dbParameter.GetParameters()));
                }

                string strInsert = string.Format(@"INSERT INTO  SFISM4.R_WIP_LOT_TRACKING_T 
                (LOT_NUMBER,
                LINE_NAME,
                STATION_NAME,
                IN_STATION_TIME ,
                MATERIAL_STATION_NAME,
                MATERIAL_IN_STATION_TIME ,
                LOT_SEQ,
                CREATE_TIME,
                SHIFT)
                VALUES
                (:LOT_NUMBER,
                :LINE_NAME,
                :STATION_NAME,
                SYSDATE,
                :MATERIAL_STATION_NAME,
                SYSDATE ,
                :LOT_SEQ,
                TO_DATE(:CREATE_TIME,'YYYY/MM/DD'),
                :SHIFT)");

                foreach (var insertEntity in insertlotEntityList)
                {
                    DBParameter udbParameter = new DBParameter();
                    udbParameter.Clear();
                    udbParameter.Add(":LOT_NUMBER", OracleDbType.Varchar2, insertEntity.LOT_NO);
                    udbParameter.Add(":LINE_NAME", OracleDbType.Varchar2, insertEntity.LINE_NAME);
                    udbParameter.Add(":STATION_NAME", OracleDbType.Varchar2, "0");
                    udbParameter.Add(":MATERIAL_STATION_NAME", OracleDbType.Varchar2, "0");
                    udbParameter.Add(":LOT_SEQ", OracleDbType.Varchar2, insertEntity.LOT_SEQ);
                    udbParameter.Add(":CREATE_TIME", OracleDbType.Varchar2, insertEntity.CREATE_TIME);
                    udbParameter.Add(":SHIFT", OracleDbType.Varchar2, insertEntity.SHIFT);
                    strSqlList.Add(new KeyValuePair<string, List<OracleParameter>>(strInsert, udbParameter.GetParameters()));
                }

                try
                {
                    result = dbHander.ExecuteNonQueryBatchSql(strSqlList);
                    result.Status = true;
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Message = ex.Message;
                    this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call UpdateLogAndInsertTracking Fail: " + ex.Message);
                }
            }
            return result;

        }

        /// <summary>
        /// 根据lotnumber删除Tracking表
        /// </summary>
        /// <param name="lotEntityList"></param>
        /// <returns></returns>
        public ReturnResult DeleteTracking(List<LotMoEntity> lotEntityList)
        {
            ReturnResult result = new ReturnResult();
            var lotNumberList = lotEntityList.Select(x => x.LOT_NO).ToArray();
            string strDelete = string.Format(@"DELETE FROM   SFISM4.R_WIP_LOT_TRACKING_T  
                WHERE LOT_NUMBER in ('{0}')", string.Join("','", lotNumberList));
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            try
            {
                result = dbHander.ExecuteNonQuery(strDelete, dbParameter.GetParameters());
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call DeleteTracking Fail: " + ex.Message);
            }
            return result;
        }
        /// <summary>
        /// Lot重新生成时记录到ISSUE表
        /// </summary>
        /// <param name="lotEntityList"></param>
        /// <returns></returns>
        public ReturnResult InsertBatch_R_ISSUE_DESC_T(List<LotMoEntity> lotEntityList)
        {
            ReturnResult result = new ReturnResult();

            List<KeyValuePair<string, List<OracleParameter>>> strSqlList = new List<KeyValuePair<string, List<OracleParameter>>>();
            string strInsert = string.Format(@"INSERT INTO  SFISM4.R_ISSUE_DESC_T 
                (CUSTOMER,
                ISSUE_TYPE,
                PN,
                OCCUR_DESC)
                VALUES
                (:CUSTOMER,
                :ISSUE_TYPE,
                :PN,
                :OCCUR_DESC)");

            foreach (var entity in lotEntityList)
            {
                DBParameter dbParameter = new DBParameter();
                dbParameter.Clear();
                dbParameter.Add(":CUSTOMER", OracleDbType.Varchar2, "PPM_LOT");
                dbParameter.Add(":ISSUE_TYPE", OracleDbType.Varchar2, "LOTREBUILDER");
                dbParameter.Add(":PN", OracleDbType.Varchar2, entity.MO_ORDER);
                dbParameter.Add(":OCCUR_DESC", OracleDbType.Varchar2, entity.LOT_NO);

                strSqlList.Add(new KeyValuePair<string, List<OracleParameter>>(strInsert, dbParameter.GetParameters()));
            }
            try
            {
                result = dbHander.ExecuteNonQueryBatchSql(strSqlList);
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call InsertBatch_R_ISSUE_DESC_T Fail: " + ex.Message);
            }

            return result;
        }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="errorReason"></param>
        /// <returns></returns>
        public ReturnResult Insert_R_ISSUE_DESC_T(string errorReason)
        {
            ReturnResult result = new ReturnResult();
            string strInsert = string.Format(@"INSERT INTO  SFISM4.R_ISSUE_DESC_T 
                (CUSTOMER,
                ISSUE_TYPE,
                PN,
                OCCUR_DESC)
                VALUES
                (:CUSTOMER,
                :ISSUE_TYPE,
                :PN,
                :OCCUR_DESC)");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":CUSTOMER", OracleDbType.Varchar2, "PPM_LOT");
            dbParameter.Add(":ISSUE_TYPE", OracleDbType.Varchar2, "PPM_LOT");
            dbParameter.Add(":PN", OracleDbType.Varchar2, "ERROR");
            dbParameter.Add(":OCCUR_DESC", OracleDbType.Varchar2, errorReason);
            try
            {
                result = dbHander.ExecuteNonQuery(strInsert,dbParameter.GetParameters());
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Insert_R_ISSUE_DESC_T Fail: " + ex.Message);
            }
            return result;
        }



        /// <summary>
        /// 获取R_WIP_LOT_MO_LOG_T数据集
        /// </summary>
        /// <returns></returns>
        public List<LotMoEntity> GetMoEntityList()
        {
            List<LotMoEntity> lotMoEntityList = new List<LotMoEntity>();
            string strSql = string.Format(@"SELECT LM.LOT_NUMBER LOT_NO,LM.LINE_NAME,LM.MO MO_ORDER,LM.MO_SEQ MO_NO,TO_CHAR(LM.MO_START_TIME,'YYYY/MM/dd') CREATE_TIME ,LM.SHIFT FROM SFISM4.R_WIP_LOT_MO_LOG_T LM");
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();

            try
            {
                lotMoEntityList = dbHander.ExecuteAsList<LotMoEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call GetMoEntityList Fail: " + ex.Message);
            }
            return lotMoEntityList;
        }
        /// <summary>
        /// 批量更新R_WIP_LOT_MO_LOG_T表中的Lot_number
        /// </summary>
        /// <param name="lotEntityList"></param>
        /// <returns></returns>
        public ReturnResult UpdateBatch_R_WIP_LOT_MO_LOG_T(List<LotMoEntity> lotEntityList)
        {
            ReturnResult result = new ReturnResult();// {  Status=false};
            List<KeyValuePair<string, List<OracleParameter>>> strSqlList = new List<KeyValuePair<string, List<OracleParameter>>>();
            string strUpdate = string.Format(@"UPDATE SFISM4.R_WIP_LOT_MO_LOG_T SET LOT_NUMBER=:LOT_NO,JOB_STATUS='2' WHERE MO=:MO_ORDER");
           
            foreach (var entity in lotEntityList)
            {
                DBParameter dbParameter2 = new DBParameter();
                dbParameter2.Clear();
                string strUpdate1 = string.Format(@"UPDATE SFISM4.R_WIP_LOT_MO_LOG_T SET LOT_NUMBER='{0}',JOB_STATUS='2' WHERE MO='{1}'", entity.LOT_NO,entity.MO_ORDER);
              //  result = dbHander.ExecuteNonQuery(strUpdate1, dbParameter2.GetParameters());

                DBParameter dbParameter = new DBParameter();
                dbParameter.Clear();
                dbParameter.Add(":MO_ORDER", OracleDbType.Varchar2, entity.MO_ORDER);
                dbParameter.Add(":LOT_NO", OracleDbType.Varchar2, entity.LOT_NO);

               
                strSqlList.Add(new KeyValuePair<string, List<OracleParameter>>(strUpdate, dbParameter.GetParameters()));
            }
            try
            {
                result = dbHander.ExecuteNonQueryBatchSql(strSqlList);
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call UpdateBatch_R_WIP_LOT_MO_LOG_T Fail: " + ex.Message);
            }
            return result;

        }

        /// <summary>
        /// 根据Mo编号获取，LOt和MO信息
        /// </summary>
        /// <param name="MO"></param>
        /// <returns></returns>
        public LotMoEntity GetMoEntityByMoOrder(string MO_ORDER)
        {
            LotMoEntity lotMoEntity = new LotMoEntity();
            string strSql = string.Format(@"SELECT LM.LOT_NUMBER LOT_NO,LM.LINE_NAME,LM.MO MO_ORDER,LM.MO_SEQ MO_NO,TO_CHAR(LM.MO_START_TIME,'YYYY/MM/dd') CREATE_TIME,LM.SHIFT FROM SFISM4.R_WIP_LOT_MO_LOG_T LM WHERE LM.MO=:MO_ORDER");
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();
            oracleParamets.Add(":MO_ORDER", OracleDbType.Varchar2, MO_ORDER);
            try
            {
                lotMoEntity = dbHander.ExecuteAsObject<LotMoEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call GetMoEntityByMo Fail: " + ex.Message);
            }
            return lotMoEntity;
        }
        /// <summary>
        /// 获取Job_Status=1的数据集
        /// </summary>
        /// <returns></returns>
        public List<LotMoEntity> GetMoEntityListByStatus()
        {
            List<LotMoEntity> lotMoEntityList = new List<LotMoEntity>();
            string strSql = string.Format(@"SELECT LM.LOT_NUMBER LOT_NO,LM.LINE_NAME,LM.MO MO_ORDER,LM.MO_SEQ MO_NO,TO_CHAR(LM.MO_START_TIME,'YYYY/MM/dd') CREATE_TIME,LM.SHIFT, '0' LOT_SEQ FROM SFISM4.R_WIP_LOT_MO_LOG_T LM WHERE LM.JOB_STATUS='1'");
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();
            try
            {

                lotMoEntityList = dbHander.ExecuteAsList<LotMoEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call GetMoEntityListByStatus Fail: " + ex.Message);
            }
            return lotMoEntityList;
        }

        
        /// <summary>
        /// 根据MO单号更新R_WIP_LOT_MO_LOG_T表中的Lot_number号
        /// </summary>
        /// <param name="MO_NO"></param>
        /// <param name="LOT_NO"></param>
        /// <returns></returns>
        public ReturnResult Update_R_WIP_LOT_MO_LOG_T(string MO_ORDER, string LOT_NO)
        {
            ReturnResult result = new ReturnResult();
            string strUpdate = string.Format(@"UPDATE SFISM4.R_WIP_LOT_MO_LOG_T SET LOT_NUMBER=:LOT_NO,JOB_STATUS='2' WHERE MO=:MO_ORDER");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();

            dbParameter.Add(":MO_ORDER", OracleDbType.Varchar2, MO_ORDER);
            dbParameter.Add(":LOT_NO", OracleDbType.Varchar2, LOT_NO);
            try
            {
                result = dbHander.ExecuteNonQuery(strUpdate, dbParameter.GetParameters());
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Update_R_WIP_LOT_MO_LOG_T Fail: " + ex.Message);
            }
            return result;
        }
        /// <summary>
        /// 批量插入Lot数据到R_WIP_LOT_TRACKING_t
        /// </summary>
        /// <param name="lotEntityList"></param>
        /// <returns></returns>
        public ReturnResult InsertBatch_R_WIP_LOT_TRACKING_T(List<LotMoEntity> lotEntityList)
        {
            ReturnResult result = new ReturnResult();
            result = DeleteTracking(lotEntityList);
            if (result.Status)
            {
                List<KeyValuePair<string, List<OracleParameter>>> strSqlList = new List<KeyValuePair<string, List<OracleParameter>>>();
                string strInsert = string.Format(@"INSERT INTO  SFISM4.R_WIP_LOT_TRACKING_T 
                (LOT_NUMBER,
                LINE_NAME,
                STATION_NAME,
                IN_STATION_TIME ,
                MATERIAL_STATION_NAME,
                MATERIAL_IN_STATION_TIME ,
                LOT_SEQ,
                CREATE_TIME,
                SHIFT)
                VALUES
                (:LOT_NUMBER,
                :LINE_NAME,
                :STATION_NAME,
                SYSDATE,
                :MATERIAL_STATION_NAME,
                SYSDATE ,
                :LOT_SEQ,
                TO_DATE(:CREATE_TIME,'YYYY/MM/DD'),
                :SHIFT)");

                foreach (var entity in lotEntityList)
                {
                    DBParameter dbParameter = new DBParameter();
                    dbParameter.Clear();
                    dbParameter.Add(":LOT_NUMBER", OracleDbType.Varchar2, entity.LOT_NO);
                    dbParameter.Add(":LINE_NAME", OracleDbType.Varchar2, entity.LINE_NAME);
                    dbParameter.Add(":STATION_NAME", OracleDbType.Varchar2, "0");
                    dbParameter.Add(":MATERIAL_STATION_NAME", OracleDbType.Varchar2, "0");
                    dbParameter.Add(":LOT_SEQ", OracleDbType.Varchar2, entity.LOT_SEQ);
                    dbParameter.Add(":CREATE_TIME", OracleDbType.Varchar2, entity.CREATE_TIME);
                    dbParameter.Add(":SHIFT", OracleDbType.Varchar2, entity.SHIFT);
                    strSqlList.Add(new KeyValuePair<string, List<OracleParameter>>(strInsert, dbParameter.GetParameters()));
                }
                try
                {
                    result = dbHander.ExecuteNonQueryBatchSql(strSqlList);
                    result.Status = true;
                }
                catch (Exception ex)
                {
                    result.Status = false;
                    result.Message = ex.Message;
                    this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call InsertBatch_R_WIP_LOT_TRACKING_T Fail: " + ex.Message);
                }
            }
            return result;
        }
        

        /// <summary>
        /// 插入Lot数据到R_WIP_LOT_TRACKING_t
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ReturnResult Insert_R_WIP_LOT_TRACKING_T(LotMoEntity entity)
        {
            ReturnResult result = new ReturnResult();
            string strInsert = string.Format(@"INSERT INTO  SFISM4.R_WIP_LOT_TRACKING_T 
                (LOT_NUMBER,
                LINE_NAME,
                STATION_NAME,
                IN_STATION_TIME ,
                MATERIAL_STATION_NAME,
                MATERIAL_IN_STATION_TIME ,
                LOT_SEQ,
                CREATE_TIME,
                SHIFT)
                VALUES
                (:LOT_NUMBER,
                :LINE_NAME,
                :STATION_NAME,
                SYSDATE,
                :MATERIAL_STATION_NAME,
                SYSDATE ,
                :LOT_SEQ,
                TO_DATE(:CREATE_TIME,'YYYY/MM/DD'),
                :SHIFT)");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":LOT_NUMBER", OracleDbType.Varchar2, entity.LOT_NO);
            dbParameter.Add(":LINE_NAME", OracleDbType.Varchar2, entity.LINE_NAME);
            dbParameter.Add(":STATION_NAME", OracleDbType.Varchar2, "0");
            dbParameter.Add(":MATERIAL_STATION_NAME", OracleDbType.Varchar2, "0");
            dbParameter.Add(":LOT_SEQ", OracleDbType.Varchar2, entity.LOT_SEQ);
            dbParameter.Add(":CREATE_TIME", OracleDbType.Varchar2, entity.CREATE_TIME);
            dbParameter.Add(":SHIFT", OracleDbType.Varchar2, entity.SHIFT);
            try
            {
                result = dbHander.ExecuteNonQuery(strInsert, dbParameter.GetParameters());
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Insert_R_WIP_LOT_TRACKING_T Fail: " + ex.Message);
            }
            return result;
        }
        /// <summary>
        /// 获取当前日期当前班次的LOT_SEQ值
        /// </summary>
        /// <param name="shift"></param>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public int GetMaxLotSeq(string shift, string createTime,string lineName)
        {
            int result = 1;
            //string strSql = "SELECT * FROM (SELECT LOT_SEQ FROM SFISM4.R_WIP_LOT_TRACKING_T LR WHERE LR.SHIFT=:SHIFT AND LR.LINE_NAME=:LINE_NAME AND LR.CREATE_TIME=TO_DATE(:CREATE_TIME,'YYYY/MM/DD') ORDER BY LR.LOT_SEQ DESC ) WHERE ROWNUM=1";
            string strSql = "SELECT * FROM (SELECT CAST(LR.LOT_SEQ AS int) LOTSEQ FROM SFISM4.R_WIP_LOT_TRACKING_T LR WHERE LR.SHIFT=:SHIFT AND LR.LINE_NAME=:LINE_NAME AND LR.CREATE_TIME=TO_DATE(:CREATE_TIME,'YYYY/MM/DD') ORDER BY LOTSEQ DESC ) WHERE ROWNUM=1";
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":SHIFT", OracleDbType.Varchar2, shift);
            dbParameter.Add(":LINE_NAME", OracleDbType.Varchar2, lineName);
            dbParameter.Add(":CREATE_TIME", OracleDbType.Varchar2, createTime);
            var resultValue = dbHander.ExecuteAsScalar<object>(strSql, CommandType.Text, dbParameter.GetParameters());
            if (resultValue != null && resultValue.ToString().Length > 0)
            {
                result += int.Parse(resultValue.ToString());
            }
            return result;
        }
        #endregion

        #region By Lot上下架数据同步到MES
        /// <summary>
        /// 获取MES库中的架位号数据集
        /// </summary>
        /// <returns></returns>
        public List<SXJ_CELLNOEntity> Get_C_PICK_CONFIG_T()
        {
            List<SXJ_CELLNOEntity> cellNoList = new List<SXJ_CELLNOEntity>();
            string strSql = string.Format(@"SELECT  PK.CELL_NO,SUBSTR(PK.POS_SET,0,6) LINE_NAME  FROM SFIS1.C_PICK_CONFIG_T PK");
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();

            try
            {
                cellNoList = dbHander.ExecuteAsList<SXJ_CELLNOEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_C_PICK_CONFIG_T Fail: " + ex.Message);
            }
            return cellNoList;
        }
        /// <summary>
        /// 根据储位号，获取储位信息，储位号在SXJ_R_KITT_PN_T表中数据
        /// </summary>
        /// <param name="rackNO"></param>
        /// <returns></returns>
        public SXJ_R_KITT_PN_TEntity Get_SXJ_R_KITT_PN_TEntityByRackNo(string rackNO)
        {
            SXJ_R_KITT_PN_TEntity entity = new SXJ_R_KITT_PN_TEntity();
            string strSql = string.Format(@"
                SELECT MOGP,
                MATERIAL,
                VENDORCODE,
                QTY,
                ONRACKQTY,
                RACKNO,
                CREATE_TIME,
                LINE_NAME,
                USER,
                DESC1,
                DESC2,
                DESC3 FROM SFISM4.R_KITT_PN_T WHERE RACKNO=:RACKNO");
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();
            oracleParamets.Add(":RACKNO", OracleDbType.Varchar2, rackNO);
            try
            {
                entity = dbHander.ExecuteAsObject<SXJ_R_KITT_PN_TEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_SXJ_R_KITT_PN_TEntity Fail: " + ex.Message);
            }
            return entity;
        }
        /// <summary>
        /// 根据SXJ_WMSEntity，（Lot,pn,vc,rackno）获取SXJ_R_KITT_PN_T数据
        /// </summary>
        /// <param name="wmsEntity"></param>
        /// <returns></returns>
        public SXJ_R_KITT_PN_TEntity Get_SXJ_R_KITT_PN_TEntityByWMSEntity(SXJ_WMSEntity wmsEntity)
        {
            SXJ_R_KITT_PN_TEntity entity = new SXJ_R_KITT_PN_TEntity();
            string strSql = string.Format(@"
                SELECT MOGP,
                MATERIAL,
                VENDORCODE,
                QTY,
                ONRACKQTY,
                RACKNO,
                CREATE_TIME,
                LINE_NAME,
                USER,
                DESC1,
                DESC2,
                DESC3 FROM SFISM4.R_KITT_PN_T WHERE  MOGP=:MOGP AND RACKNO=:RACKNO AND MATERIAL=:MATERIAL AND VENDORCODE=:VENDORCODE ");
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();
            oracleParamets.Add(":MOGP", OracleDbType.Varchar2, wmsEntity.MOGP);
            oracleParamets.Add(":RACKNO", OracleDbType.Varchar2, wmsEntity.RACKNO);
            oracleParamets.Add(":MATERIAL", OracleDbType.Varchar2, wmsEntity.MATERIAL);
            oracleParamets.Add(":VendorCode", OracleDbType.Varchar2, wmsEntity.VENDORCODE);
            try
            {
                entity = dbHander.ExecuteAsObject<SXJ_R_KITT_PN_TEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_SXJ_R_KITT_PN_TEntity Fail: " + ex.Message);
            }
            return entity;
        }

        /// <summary>
        /// 根据（pn，vc，rackno）获取SXJ_R_KITT_PN_T数据
        /// </summary>
        /// <param name="rackNo">储位号</param>
        /// <param name="material">物料号</param>
        /// <param name="VendorCode">供应商号</param>
        /// <returns></returns>
        public SXJ_R_KITT_PN_TEntity Get_SXJ_R_KITT_PN_TEntityByPNAndVCAndRack(string rackNo, string material, string VendorCode)
        {
            SXJ_R_KITT_PN_TEntity entity = new SXJ_R_KITT_PN_TEntity();
            string strSql = string.Format(@"
                SELECT MOGP,
                MATERIAL,
                VENDORCODE,
                QTY,
                ONRACKQTY,
                RACKNO,
                CREATE_TIME,
                LINE_NAME,
                USER,
                DESC1,
                DESC2,
                DESC3 FROM SFISM4.R_KITT_PN_T WHERE  RACKNO=:RACKNO AND MATERIAL=:MATERIAL AND VENDORCODE=:VENDORCODE ");
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();
            oracleParamets.Add(":RACKNO", OracleDbType.Varchar2, rackNo);
            oracleParamets.Add(":MATERIAL", OracleDbType.Varchar2, material);
            oracleParamets.Add(":VendorCode", OracleDbType.Varchar2, VendorCode);
            try
            {
                entity = dbHander.ExecuteAsObject<SXJ_R_KITT_PN_TEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_SXJ_R_KITT_PN_TEntity Fail: " + ex.Message);
            }
            return entity;
        }

        /// <summary>
        /// 根据（lot,pn，vc）获取SXJ_R_KITT_PN_T数据
        /// </summary>
        /// <param name="lotNo">lot号</param>
        /// <param name="material">物料号</param>
        /// <param name="VendorCode">供应商号</param>
        /// <returns></returns>
        public SXJ_R_KITT_PN_TEntity Get_SXJ_R_KITT_PN_TEntityByLotAndPNAndVC(string lotNo, string material, string VendorCode)
        {
            SXJ_R_KITT_PN_TEntity entity = new SXJ_R_KITT_PN_TEntity();
            string strSql = string.Format(@"
                SELECT MOGP,
                MATERIAL,
                VENDORCODE,
                QTY,
                ONRACKQTY,
                RACKNO,
                CREATE_TIME,
                LINE_NAME,
                USER,
                DESC1,
                DESC2,
                DESC3 FROM SFISM4.R_KITT_PN_T WHERE  MOGP=:MOGP AND MATERIAL=:MATERIAL AND VENDORCODE=:VENDORCODE ");
            DBParameter oracleParamets = new DBParameter();
            oracleParamets.Clear();
            oracleParamets.Add(":MOGP", OracleDbType.Varchar2, lotNo);
            oracleParamets.Add(":MATERIAL", OracleDbType.Varchar2, material);
            oracleParamets.Add(":VendorCode", OracleDbType.Varchar2, VendorCode);
            try
            {
                entity = dbHander.ExecuteAsObject<SXJ_R_KITT_PN_TEntity>(strSql, CommandType.Text, oracleParamets.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_SXJ_R_KITT_PN_TEntity Fail: " + ex.Message);
            }
            return entity;
        }


        /// <summary>
        /// 插入数据到，架位信息表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ReturnResult Insert_SXJ_R_KITT_PN_T(SXJ_R_KITT_PN_TEntity entity)
        {
            ReturnResult result = new ReturnResult();
            string strInsert = string.Format(@"INSERT INTO  SFISM4.R_KITT_PN_T 
                (MOGP,
                MATERIAL,
                VENDORCODE,
                QTY,
                ONRACKQTY,
                RACKNO,
                CREATE_TIME,
                LINE_NAME,
                CREATE_NAME,
                DESC1,
                DESC2,
                DESC3)
                VALUES
                (:MOGP,
                :MATERIAL,
                :VENDORCODE,
                :QTY,
                :ONRACKQTY,
                :RACKNO,
                SYSDATE,
                :LINE_NAME,
                :CREATE_NAME,
                :DESC1,
                :DESC2,
                :DESC3)");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":MOGP", OracleDbType.Varchar2, entity.MOGP);
            dbParameter.Add(":MATERIAL", OracleDbType.Varchar2, entity.MATERIAL);
            dbParameter.Add(":VENDORCODE", OracleDbType.Varchar2, entity.VENDORCODE);
            dbParameter.Add(":QTY", OracleDbType.Int32, entity.QTY);
            dbParameter.Add(":ONRACKQTY", OracleDbType.Int32, entity.ONRACKQTY);
            dbParameter.Add(":RACKNO", OracleDbType.Varchar2, entity.RACKNO);
            //dbParameter.Add(":CREATE_TIME", OracleType.DateTime, entity.CREATE_TIME);
            dbParameter.Add(":LINE_NAME", OracleDbType.Varchar2, entity.LINE_NAME);
            dbParameter.Add(":CREATE_NAME", OracleDbType.Varchar2, entity.CREATE_NAME);
            dbParameter.Add(":DESC1", OracleDbType.Varchar2, entity.DESC1);
            dbParameter.Add(":DESC2", OracleDbType.Varchar2, entity.DESC2);
            dbParameter.Add(":DESC3", OracleDbType.Varchar2, entity.DESC3);


            try
            {
                result = dbHander.ExecuteNonQuery(strInsert, dbParameter.GetParameters());
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Insert_SXJ_R_KITT_PN_T Fail: " + ex.Message);
            }
            return result;
        }
        /// <summary>
        /// 插入数据到，架位信息存储历史表
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ReturnResult Insert_SXJ_R_KITT_PN_LOG_T(SXJ_R_KITT_PN_LOG_TEntity entity)
        {
            ReturnResult result = new ReturnResult();
            string strInsert = string.Format(@"INSERT INTO  SFISM4.R_KITT_PN_LOG_T 
                (MOGP,
                MATERIAL,
                VENDORCODE,
                QTY,
                ONRACKQTY,
                RACKNO,
                CREATE_TIME,
                LINE_NAME,
                CREATE_NAME,
                DESC1,
                DESC2,
                DESC3,
                CLOSE_TIME,
                STATUS,
                STATUS_INFO)
                VALUES
                (:MOGP,
                :MATERIAL,
                :VENDORCODE,
                :QTY,
                :ONRACKQTY,
                :RACKNO,
                SYSDATE,
                :LINE_NAME,
                :CREATE_NAME,
                :DESC1,
                :DESC2,
                :DESC3,
                SYSDATE,
                :STATUS,
                :STATUS_INFO)");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":MOGP", OracleDbType.Varchar2, entity.MOGP);
            dbParameter.Add(":MATERIAL", OracleDbType.Varchar2, entity.MATERIAL);
            dbParameter.Add(":VENDORCODE", OracleDbType.Varchar2, entity.VENDORCODE);
            dbParameter.Add(":QTY", OracleDbType.Int32, entity.QTY);
            dbParameter.Add(":ONRACKQTY", OracleDbType.Int32, entity.ONRACKQTY);
            dbParameter.Add(":RACKNO", OracleDbType.Varchar2, entity.RACKNO);
            //dbParameter.Add(":CREATE_TIME", OracleType.DateTime, entity.CREATE_TIME);
            dbParameter.Add(":LINE_NAME", OracleDbType.Varchar2, entity.LINE_NAME);
            dbParameter.Add(":CREATE_NAME", OracleDbType.Varchar2, entity.CREATE_NAME);
            dbParameter.Add(":DESC1", OracleDbType.Varchar2, entity.DESC1);
            dbParameter.Add(":DESC2", OracleDbType.Varchar2, entity.DESC2);
            dbParameter.Add(":DESC3", OracleDbType.Varchar2, entity.DESC3);
            dbParameter.Add(":STATUS", OracleDbType.Int32, entity.STATUS);
            dbParameter.Add(":STATUS_INFO", OracleDbType.Varchar2, entity.STATUS_INFO);

            try
            {
                result = dbHander.ExecuteNonQuery(strInsert, dbParameter.GetParameters());
                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = ex.Message;
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Insert_SXJ_R_KITT_PN_LOG Fail: " + ex.Message);
            }
            return result;
        }
        /// <summary>
        /// 更新SXJ_R_KITT_PN_T架位信息数据
        /// </summary>
        /// <param name="rackNO"></param>
        /// <param name="material"></param>
        /// <param name="vendorCode"></param>
        /// <param name="onrackqty"></param>
        /// <returns></returns>
        public ReturnResult Update_SXJ_R_KITT_PN_TEntity(SXJ_R_KITT_PN_TEntity entity)
        {
            ReturnResult result = new ReturnResult();
            string updateSql = string.Format(@" UPDATE SFISM4.R_KITT_PN_T SET ONRACKQTY=(ONRACKQTY+(:ONRACKQTY))
              WHERE MOGP=:MOGP AND MATERIAL=:MATERIAL AND 
              VENDORCODE=:VENDORCODE AND  RACKNO=:RACKNO");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":MOGP", OracleDbType.Varchar2, entity.MOGP);
            dbParameter.Add(":RACKNO", OracleDbType.Varchar2, entity.RACKNO);
            dbParameter.Add(":ONRACKQTY", OracleDbType.Int32, entity.ONRACKQTY);
            dbParameter.Add(":MATERIAL", OracleDbType.Varchar2, entity.MATERIAL);
            dbParameter.Add(":VENDORCODE", OracleDbType.Varchar2, entity.VENDORCODE);
            try
            {
                result = dbHander.ExecuteNonQuery(updateSql, dbParameter.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_SXJ_R_KITT_PN_TEntity Fail: " + ex.Message);
            }
            return result;
        }
        /// <summary>
        /// 删除R_KITT_PN_T表数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ReturnResult Delete_SXJ_R_KITT_PN_TEntity(SXJ_R_KITT_PN_TEntity entity)
        {
            ReturnResult result = new ReturnResult();
            string updateSql = string.Format(@" DELETE FROM SFISM4.R_KITT_PN_T 
              WHERE MOGP=:MOGP AND MATERIAL=:MATERIAL AND 
              VENDORCODE=:VENDORCODE AND  RACKNO=:RACKNO");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":MOGP", OracleDbType.Varchar2, entity.MOGP);
            dbParameter.Add(":RACKNO", OracleDbType.Varchar2, entity.RACKNO);
            dbParameter.Add(":ONRACKQTY", OracleDbType.Int32, entity.ONRACKQTY);
            dbParameter.Add(":MATERIAL", OracleDbType.Varchar2, entity.MATERIAL);
            dbParameter.Add(":VENDORCODE", OracleDbType.Varchar2, entity.VENDORCODE);
            try
            {
                result = dbHander.ExecuteNonQuery(updateSql, dbParameter.GetParameters());
            }
            catch (Exception ex)
            {

                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Delete_SXJ_R_KITT_PN_TEntity Fail: " + ex.Message);
            }
            return result;
        }

        #endregion

        #region 调用库存(WMS从MES获取RACK架位信息)
        /// <summary>
        /// 根据料号、客户别获取料号对应站别
        /// </summary>
        /// <param name="cust_No"></param>
        /// <param name="key_Part_No"></param>
        /// <returns></returns>
        public DataSet Get_C_KEYPARTS_MODE_T(string cust_No, string key_Part_No)
        {
            DataSet ds = new DataSet();
            string strSql = string.Format(@"SELECT A.KEY_PART_NO,A.PART_MODE FROM SFIS1.C_KEYPARTS_MODE_T A  WHERE A.CUST_NO=:CUST_NO AND A.KEY_PART_NO=:KEY_PART_NO");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":CUST_NO", OracleDbType.Varchar2, cust_No);
            dbParameter.Add(":KEY_PART_NO", OracleDbType.Varchar2, key_Part_No);
            try
            {
                ds = dbHander.ExecuteAsDataSet(strSql, dbParameter.GetParameters());
            }
            catch (Exception ex)
            {
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call Get_C_KEYPARTS_MODE_T Fail: " + ex.Message);
            }
            return ds;
        }
        #endregion

        #region 3.4 同步MO线体信息
        /// <summary>
        /// 同步MO线体信息
        /// </summary>
        /// <param name="tID"></param>
        /// <returns></returns>
        public DataTable MO_Synchronization(string tID)
        {
            DataTable dt = new DataTable();
            string strSql = string.Format(@"SELECT 
                                            T.TID,A.LINE_NAME 
                                            FROM SFISM4.R_WIP_LOG_T A
                                            LEFT JOIN SFISM4.R_WIP_TID_T T ON T.SERIAL_NUMBER=A.SERIAL_NUMBER
                                            WHERE A.GROUP_NAME='KITTING1' AND TID=:TID");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":TID", OracleDbType.Varchar2, tID);
            try
            {
                DataSet ds = dbHander.ExecuteAsDataSet(strSql, dbParameter.GetParameters());
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call MO_Synchronization [Service] Fail: " + ex.Message);
            }
            return dt;
        }
        #endregion

        #region 3.3 同步料架库存及水位信息
        /// <summary>
        /// 3.3 同步料架库存及水位信息
        /// </summary>
        /// <param name="lineName">线体名称</param>
        /// <returns></returns>
        public DataTable MO_RackStockAndWaterLevel(string lineName)
        {
            DataTable dt = new DataTable();
            string strSql = string.Format(@"SELECT 
                                            K.MOGP,K.LINE_NAME,K.MATERIAL,K.VENDORCODE,K.QTY,K.RACKNO 
                                            FROM SFIS1.C_PICK_CONFIG_T P
                                            INNER JOIN SFISM4.R_KITT_PN_T K ON P.CELL_NO = K.RACKNO
                                            WHERE SUBSTR(P.POS_SET,0,6)=:LINE");
            DBParameter dbParameter = new DBParameter();
            dbParameter.Clear();
            dbParameter.Add(":LINE", OracleDbType.Varchar2, lineName);
            try
            {
                DataSet ds = dbHander.ExecuteAsDataSet(strSql, dbParameter.GetParameters());
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    return ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                this.logHandler.Error(MethodBase.GetCurrentMethod().Name, "Call MO_RackStockAndWaterLevel [Service] Fail: " + ex.Message);
            }
            return dt;
        }
        #endregion
    }
}