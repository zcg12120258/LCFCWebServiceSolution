using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using KittingToLineWebService.Core;
using System.Data;
using CommonHandler.Model;
using KittingToLineWebService.Beans;

namespace KittingToLineWebService
{
    /// <summary>
    /// KittingToLineInterface 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://kittingtolineinterfaceuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class KittingToLineInterface : System.Web.Services.WebService
    {

         //数据库连接字符串
        private string connectionString = System.Configuration.ConfigurationManager.AppSettings["connstrbox_prd"];   //测试环境
        private DataProcessHelper dataProcessHelper;

        public KittingToLineInterface()
        {
            dataProcessHelper = new DataProcessHelper(connectionString);
        }

        #region 从SAP获取Lot号，更新R_WIP_LOT_MO_LOG_T表的Lot号、插入Lot号和Lot_Seq数据到r_wip_lot_tracking_t表
        /// <summary>
        /// DataTable列为LOT_NO,MO_NO,MO_ORDER
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        [WebMethod]
        public string Get_PPM_LotFromSap(List<LotMoEntity> lotMoList)
        {
            //説明List数据类似如下格式
            //List<LotMoEntity> lotMoList = new List<LotMoEntity>();
            //lotMoList.Add(new LotMoEntity { LOT_NO = "lot001", MO_NO = "1", MO_ORDER = "10702830" });
            //lotMoList.Add(new LotMoEntity { LOT_NO = "lot001", MO_NO = "2", MO_ORDER = "12911585" });
            //lotMoList.Add(new LotMoEntity { LOT_NO = "lot003", MO_NO = "3", MO_ORDER = "12911586" });
            ReturnResult result = new ReturnResult();
            result = dataProcessHelper.Get_PPM_LotFromSap_Process(lotMoList);
            if (result.Status)
            {
                return new { Flag = "Y", Log = "" }.ToString();
            }
            else
            {
                return new { Flag = "N", Log = result.Message }.ToString();
            }

        }
        #endregion

        #region By Lot上下架数据同步到MES
        /// <summary>
        /// By Lot上下架数据同步到MES
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        [WebMethod]
        public DataTable SXJ(DataTable dt)
        {
            //説明DataTable数据类似如下格式
            //DataTable dt = new DataTable("test");
            //dt.Columns.Add("MOGP", System.Type.GetType("System.String"));
            //dt.Columns.Add("MATERIAL", System.Type.GetType("System.String"));
            //dt.Columns.Add("VENDORCODE", System.Type.GetType("System.String"));
            //dt.Columns.Add("QTY", System.Type.GetType("System.Int32"));
            //dt.Columns.Add("ONRACKQTY", System.Type.GetType("System.Int32"));
            //dt.Columns.Add("RACKNO", System.Type.GetType("System.String"));
            //dt.Columns.Add("USER", System.Type.GetType("System.String"));
            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101001", "JLEN2", 500, 100, "120590103", "001" });//insert

            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101001", "JLEN2", 500, -50, "120590103", "001" });//update
            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101001", "JLEN3", 500, 150, "120590103", "001" });//error：rackno相同，vc不同
            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101002", "JLEN2", 500, 150, "120590103", "001" });//error：rackno相同，pn不同
            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101002", "JLEN3", 500, 150, "120590103", "001" });//error：rackno相同,vc和pn相同
            //dt.Rows.Add(new object[] { "20190314AA0002", "PK010101001", "JLEN2", 500, 150, "120590103", "001" });//Insert：rackno相同，pn和vc相同，lot不同
            //dt.Rows.Add(new object[] { "20190314AA0002", "PK010101002", "JLEN2", 500, 150, "120590103", "001" });//error：rackno相同，lot和pn不同
            //dt.Rows.Add(new object[] { "20190314AA0002", "PK010101001", "JLEN3", 500, 150, "120590103", "001" });//error：rackno相同，lot和vc不同
            //dt.Rows.Add(new object[] { "20190314AA0002", "PK010101002", "JLEN3", 500, 150, "120590103", "001" });//error：rackno相同，lot和pn和vc不同

            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101001", "JLEN2", 500, 150, "120590104", "001" });//error：rackno不同，lot，pn，vc 相同
            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101001", "JLEN3", 500, 150, "120590104", "001" });//Insert：rackno不同，vc不同
            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101002", "JLEN2", 500, 150, "120590105", "001" });//insert：rackno不同，pn不同
            //dt.Rows.Add(new object[] { "20190314AA0001", "PK010101002", "JLEN3", 500, 150, "120590106", "001" });//insert：rackno不同,vc,pn不同
            //dt.Rows.Add(new object[] { "20190314AA0002", "PK010101001", "JLEN2", 500, 150, "120590107", "001" });//error：rackno不同，lot，pn，vc 相同
            //dt.Rows.Add(new object[] { "20190314AA0002", "PK010101002", "JLEN2", 500, 150, "120590108", "001" });//insert：rackno不同，lot和pn不同
            //dt.Rows.Add(new object[] { "20190314AA0002", "PK010101001", "JLEN3", 500, 150, "120590109", "001" });//insert：rackno不同，lot和vc不同
            //dt.Rows.Add(new object[] { "20190314AA0002", "PK010101002", "JLEN3", 500, 150, "120590110", "001" });//insert：rackno不同，lot和pn和vc不同

            //dt.Rows.Add(new object[] { "20190314AA0005", "PK010101003", "JLEN3", 500, 150, "450590605", "001" });//error：mes中不存在
            DataTable resultDt = new DataTable();
            resultDt = dataProcessHelper.SXJ_Process(dt);
            return resultDt;
        }
        #endregion

        #region 调用库存(WMS从MES获取RACK架位信息)
        /// <summary>
        /// 根据料号、客户别获取料号对应站别
        /// </summary>
        /// <param name="cust_No"></param>
        /// <param name="key_Part_No"></param>
        /// <returns></returns>
        [WebMethod]
        public string ZB(string cust_No, string key_Part_No)
        {
            string result = string.Empty;
            result = dataProcessHelper.ZB_Process(cust_No,key_Part_No);
            return result;
        }
        #endregion

        #region 3.3 同步料架库存及水位信息
        /// <summary>
        /// 3.3 同步料架库存及水位信息
        /// </summary>
        /// <param name="lineName">线体名称</param>
        /// <returns></returns>
        [WebMethod]
        public DataTable KC(string lineName)
        {
            DataTable resultDt = dataProcessHelper.MO_RackStockAndWaterLevel(lineName);
            return resultDt;
        }
        #endregion

        #region 3.4 同步MO线体信息
        /// <summary>
        /// MO线体信息同步
        /// </summary>
        /// <param name="tID"></param>
        /// <returns></returns>
        [WebMethod]
        public DataTable MO_Synchronization(string tID)
        {
            DataTable resultDt = dataProcessHelper.MO_Synchronization(tID);
            return resultDt;
        }
        #endregion

    }
}
