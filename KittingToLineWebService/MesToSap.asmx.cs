using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using KittingToLineWebService.Core;
using CommonHandler.Model;
using KittingToLineWebService.Beans;

namespace KittingToLineWebService
{
    /// <summary>
    /// MesToSap 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class MesToSap : System.Web.Services.WebService
    {
        //数据库连接字符串
        private string connectionString = System.Configuration.ConfigurationManager.AppSettings["connstrbox_prd"];   //测试环境
        private DataProcessHelper dataProcessHelper;

        public MesToSap()
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
            //lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0026", MO_ORDER = "13222125", MO_NO = "7" });
            //lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0026", MO_ORDER = "13222126", MO_NO = "8" });
            //lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0027", MO_ORDER = "13222128", MO_NO = "10" });
            //lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0039", MO_ORDER = "13222118", MO_NO = "36" });
            //lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0041", MO_ORDER = "13222115", MO_NO = "33" });
            //lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0041", MO_ORDER = "13222114", MO_NO = "32" });
            
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
    }
}
