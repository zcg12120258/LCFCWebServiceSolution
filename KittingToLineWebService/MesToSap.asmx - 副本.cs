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
        private string connectionString = System.Configuration.ConfigurationManager.AppSettings["connstrbox_dev"];   //测试环境
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
            List<LotMoEntity> lotMoList = new List<LotMoEntity>();
            lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0026", MO_ORDER = "13222125", MO_NO = "7" });
            lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0026", MO_ORDER = "13222126", MO_NO = "8" });
            lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0027", MO_ORDER = "13222128", MO_NO = "10" });
            lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0039", MO_ORDER = "13222118", MO_NO = "36" });
            lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0041", MO_ORDER = "13222115", MO_NO = "33" });
            lotMoList.Add(new LotMoEntity { LOT_NO = "20190731AS0041", MO_ORDER = "13222114", MO_NO = "32" });
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0005",	MO_ORDER = "13387142",	MO_NO = "6"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0005",	MO_ORDER = "13387141",	MO_NO = "5"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0005",	MO_ORDER = "13387140",	MO_NO = "4"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0114",	MO_ORDER = "13388599",	MO_NO = "355"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0114",	MO_ORDER = "13388598",	MO_NO = "354"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0124",	MO_ORDER = "13388682",	MO_NO = "374"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0124",	MO_ORDER = "13388681",	MO_NO = "373"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0124",	MO_ORDER = "13388680",	MO_NO = "372"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0073",	MO_ORDER = "13387874",	MO_NO = "389"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0016",	MO_ORDER = "13384325",	MO_NO = "4"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0016",	MO_ORDER = "13384324",	MO_NO = "3"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0102",	MO_ORDER = "13388661",	MO_NO = "325"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0102",	MO_ORDER = "13388660",	MO_NO = "324"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0102",	MO_ORDER = "13388659",	MO_NO = "323"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190917AS0235",	MO_ORDER = "13384512",	MO_NO = "202"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0021",	MO_ORDER = "13387134",	MO_NO = "2"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0080",	MO_ORDER = "13388628",	MO_NO = "272"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0080",	MO_ORDER = "13388627",	MO_NO = "271"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0024",	MO_ORDER = "13384310",	MO_NO = "16"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0024",	MO_ORDER = "13384309",	MO_NO = "15"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0017",	MO_ORDER = "13384327",	MO_NO = "6"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0017",	MO_ORDER = "13384326",	MO_NO = "5"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0112",	MO_ORDER = "13388591",	MO_NO = "340"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0113",	MO_ORDER = "13388562",	MO_NO = "350"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0110",	MO_ORDER = "13388603",	MO_NO = "344"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0110",	MO_ORDER = "13388602",	MO_NO = "343"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0036",	MO_ORDER = "13387120",	MO_NO = "1"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0100",	MO_ORDER = "13388622",	MO_NO = "320"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0100",	MO_ORDER = "13388621",	MO_NO = "319"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0129",	MO_ORDER = "13388666",	MO_NO = "377"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0129",	MO_ORDER = "13388665",	MO_NO = "376"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0091",	MO_ORDER = "13388658",	MO_NO = "302"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0091",	MO_ORDER = "13388657",	MO_NO = "301"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0091",	MO_ORDER = "13388656",	MO_NO = "300"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0091",	MO_ORDER = "13388655",	MO_NO = "299"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0091",	MO_ORDER = "13388654",	MO_NO = "298"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0032",	MO_ORDER = "13386853",	MO_NO = "3"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0023",	MO_ORDER = "13387132",	MO_NO = "13"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0023",	MO_ORDER = "13387131",	MO_NO = "12"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0048",	MO_ORDER = "13387402",	MO_NO = "113"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0048",	MO_ORDER = "13387401",	MO_NO = "112"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0010",	MO_ORDER = "13387136",	MO_NO = "17"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0010",	MO_ORDER = "13387135",	MO_NO = "16"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0130",	MO_ORDER = "13388707",	MO_NO = "384"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0130",	MO_ORDER = "13388706",	MO_NO = "383"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0029",	MO_ORDER = "13384319",	MO_NO = "25"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0119",	MO_ORDER = "13388594",	MO_NO = "353"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388582",	MO_NO = "268"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388581",	MO_NO = "267"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388580",	MO_NO = "266"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388579",	MO_NO = "265"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388578",	MO_NO = "264"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388577",	MO_NO = "263"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388576",	MO_NO = "262"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388575",	MO_NO = "261"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388574",	MO_NO = "260"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388573",	MO_NO = "259"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388572",	MO_NO = "258"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0078",	MO_ORDER = "13388571",	MO_NO = "257"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0002",	MO_ORDER = "13387125",	MO_NO = "6"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0071",	MO_ORDER = "13388436",	MO_NO = "349"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0071",	MO_ORDER = "13388435",	MO_NO = "348"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0093",	MO_ORDER = "13388608",	MO_NO = "306"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0093",	MO_ORDER = "13388607",	MO_NO = "305"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0044",	MO_ORDER = "13387266",	MO_NO = "104"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0044",	MO_ORDER = "13387265",	MO_NO = "103"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0089",	MO_ORDER = "13388650",	MO_NO = "294"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0089",	MO_ORDER = "13388649",	MO_NO = "293"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0007",	MO_ORDER = "13387146",	MO_NO = "10"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0007",	MO_ORDER = "13387145",	MO_NO = "9"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0011",	MO_ORDER = "13387156",	MO_NO = "21"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0126",	MO_ORDER = "13388437",	MO_NO = "375"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0046",	MO_ORDER = "13387408",	MO_NO = "108"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0046",	MO_ORDER = "13387407",	MO_NO = "107"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0115",	MO_ORDER = "13388601",	MO_NO = "357"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0115",	MO_ORDER = "13388600",	MO_NO = "356"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0085",	MO_ORDER = "13388638",	MO_NO = "282"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0085",	MO_ORDER = "13388637",	MO_NO = "281"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0003",	MO_ORDER = "13387124",	MO_NO = "5"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0003",	MO_ORDER = "13387123",	MO_NO = "4"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0003",	MO_ORDER = "13387122",	MO_NO = "3"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0084",	MO_ORDER = "13388636",	MO_NO = "280"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0084",	MO_ORDER = "13388635",	MO_NO = "279"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0079",	MO_ORDER = "13388626",	MO_NO = "270"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0079",	MO_ORDER = "13388625",	MO_NO = "269"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0117",	MO_ORDER = "13388712",	MO_NO = "359"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0092",	MO_ORDER = "13388606",	MO_NO = "304"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0092",	MO_ORDER = "13388605",	MO_NO = "303"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0040",	MO_ORDER = "13386848",	MO_NO = "3"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0121",	MO_ORDER = "13388439",	MO_NO = "362"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0104",	MO_ORDER = "13388664",	MO_NO = "328"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0103",	MO_ORDER = "13388663",	MO_NO = "327"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0103",	MO_ORDER = "13388662",	MO_NO = "326"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0008",	MO_ORDER = "13387148",	MO_NO = "12"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0008",	MO_ORDER = "13387147",	MO_NO = "11"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0090",	MO_ORDER = "13388653",	MO_NO = "297"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0090",	MO_ORDER = "13388652",	MO_NO = "296"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0090",	MO_ORDER = "13388651",	MO_NO = "295"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0072",	MO_ORDER = "13388431",	MO_NO = "347"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0072",	MO_ORDER = "13388430",	MO_NO = "346"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0108",	MO_ORDER = "13388566",	MO_NO = "336"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0108",	MO_ORDER = "13388565",	MO_NO = "335"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0074",	MO_ORDER = "13388587",	MO_NO = "252"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0074",	MO_ORDER = "13388586",	MO_NO = "251"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0074",	MO_ORDER = "13388584",	MO_NO = "250"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0038",	MO_ORDER = "13387159",	MO_NO = "1"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0019",	MO_ORDER = "13384331",	MO_NO = "10"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0019",	MO_ORDER = "13384330",	MO_NO = "9"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0014",	MO_ORDER = "13387157",	MO_NO = "22"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0012",	MO_ORDER = "13387154",	MO_NO = "19"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0012",	MO_ORDER = "13387153",	MO_NO = "18"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0026",	MO_ORDER = "13384314",	MO_NO = "20"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0026",	MO_ORDER = "13384313",	MO_NO = "19"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0039",	MO_ORDER = "13387185",	MO_NO = "2"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0070",	MO_ORDER = "13388434",	MO_NO = "339"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0037",	MO_ORDER = "13387161",	MO_NO = "3"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0037",	MO_ORDER = "13387160",	MO_NO = "2"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0031",	MO_ORDER = "13386850",	MO_NO = "2"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0031",	MO_ORDER = "13386849",	MO_NO = "1"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190917AS0236",	MO_ORDER = "13384509",	MO_NO = "204"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0013",	MO_ORDER = "13387155",	MO_NO = "20"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0107",	MO_ORDER = "13388564",	MO_NO = "334"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0107",	MO_ORDER = "13388563",	MO_NO = "333"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0081",	MO_ORDER = "13388630",	MO_NO = "274"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0081",	MO_ORDER = "13388629",	MO_NO = "273"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0035",	MO_ORDER = "13386852",	MO_NO = "11"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0035",	MO_ORDER = "13386851",	MO_NO = "10"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0020",	MO_ORDER = "13384332",	MO_NO = "11"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0087",	MO_ORDER = "13388642",	MO_NO = "286"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0087",	MO_ORDER = "13388641",	MO_NO = "285"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0086",	MO_ORDER = "13388640",	MO_NO = "284"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0086",	MO_ORDER = "13388639",	MO_NO = "283"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0028",	MO_ORDER = "13384318",	MO_NO = "24"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0028",	MO_ORDER = "13384317",	MO_NO = "23"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0106",	MO_ORDER = "13388570",	MO_NO = "332"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0106",	MO_ORDER = "13388569",	MO_NO = "331"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0097",	MO_ORDER = "13388616",	MO_NO = "314"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0097",	MO_ORDER = "13388615",	MO_NO = "313"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0127",	MO_ORDER = "13388672",	MO_NO = "379"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0127",	MO_ORDER = "13388671",	MO_NO = "378"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0109",	MO_ORDER = "13388596",	MO_NO = "342"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0109",	MO_ORDER = "13388595",	MO_NO = "341"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0018",	MO_ORDER = "13384329",	MO_NO = "8"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0018",	MO_ORDER = "13384328",	MO_NO = "7"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0128",	MO_ORDER = "13388675",	MO_NO = "382"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0128",	MO_ORDER = "13388674",	MO_NO = "381"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0128",	MO_ORDER = "13388673",	MO_NO = "380"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0015",	MO_ORDER = "13387121",	MO_NO = "1"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0123",	MO_ORDER = "13388679",	MO_NO = "371"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0123",	MO_ORDER = "13388678",	MO_NO = "370"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0123",	MO_ORDER = "13388677",	MO_NO = "369"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0123",	MO_ORDER = "13388676",	MO_NO = "368"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0022",	MO_ORDER = "13387130",	MO_NO = "1"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0001",	MO_ORDER = "13387128",	MO_NO = "2"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0001",	MO_ORDER = "13387127",	MO_NO = "1"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0120",	MO_ORDER = "13388561",	MO_NO = "361"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0120",	MO_ORDER = "13388560",	MO_NO = "360"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0076",	MO_ORDER = "13388590",	MO_NO = "255"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0116",	MO_ORDER = "13388604",	MO_NO = "358"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190917AS0237",	MO_ORDER = "13384514",	MO_NO = "205"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0098",	MO_ORDER = "13388618",	MO_NO = "316"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0098",	MO_ORDER = "13388617",	MO_NO = "315"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0045",	MO_ORDER = "13387406",	MO_NO = "106"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0045",	MO_ORDER = "13387405",	MO_NO = "105"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0009",	MO_ORDER = "13387151",	MO_NO = "15"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0009",	MO_ORDER = "13387150",	MO_NO = "14"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0009",	MO_ORDER = "13387149",	MO_NO = "13"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0043",	MO_ORDER = "13387268",	MO_NO = "102"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0043",	MO_ORDER = "13387267",	MO_NO = "101"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0082",	MO_ORDER = "13388632",	MO_NO = "276"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0082",	MO_ORDER = "13388631",	MO_NO = "275"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0096",	MO_ORDER = "13388614",	MO_NO = "312"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0096",	MO_ORDER = "13388613",	MO_NO = "311"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0105",	MO_ORDER = "13388568",	MO_NO = "330"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0105",	MO_ORDER = "13388567",	MO_NO = "329"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0118",	MO_ORDER = "13388593",	MO_NO = "352"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0118",	MO_ORDER = "13388592",	MO_NO = "351"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0069",	MO_ORDER = "13388433",	MO_NO = "338"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0069",	MO_ORDER = "13388432",	MO_NO = "337"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0095",	MO_ORDER = "13388612",	MO_NO = "310"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0095",	MO_ORDER = "13388611",	MO_NO = "309"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387184",	MO_NO = "23"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387183",	MO_NO = "22"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387182",	MO_NO = "21"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387181",	MO_NO = "20"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387180",	MO_NO = "19"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387179",	MO_NO = "18"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387178",	MO_NO = "17"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387177",	MO_NO = "16"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387175",	MO_NO = "15"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387174",	MO_NO = "14"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387173",	MO_NO = "13"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387172",	MO_NO = "12"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387171",	MO_NO = "11"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387170",	MO_NO = "10"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387169",	MO_NO = "9"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387168",	MO_NO = "8"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387167",	MO_NO = "7"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387166",	MO_NO = "6"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387164",	MO_NO = "5"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0041",	MO_ORDER = "13387163",	MO_NO = "4"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0047",	MO_ORDER = "13387411",	MO_NO = "111"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0047",	MO_ORDER = "13387410",	MO_NO = "110"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0047",	MO_ORDER = "13387409",	MO_NO = "109"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0006",	MO_ORDER = "13387144",	MO_NO = "8"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0006",	MO_ORDER = "13387143",	MO_NO = "7"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388705",	MO_NO = "409"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388704",	MO_NO = "408"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388703",	MO_NO = "407"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388702",	MO_NO = "406"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388701",	MO_NO = "405"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388700",	MO_NO = "404"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388699",	MO_NO = "403"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388697",	MO_NO = "402"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388696",	MO_NO = "401"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388695",	MO_NO = "400"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388694",	MO_NO = "399"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388693",	MO_NO = "398"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388692",	MO_NO = "397"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388691",	MO_NO = "396"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388690",	MO_NO = "395"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388689",	MO_NO = "394"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388688",	MO_NO = "393"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388687",	MO_NO = "392"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388686",	MO_NO = "391"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0132",	MO_ORDER = "13388684",	MO_NO = "390"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0030",	MO_ORDER = "13387133",	MO_NO = "14"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0027",	MO_ORDER = "13384316",	MO_NO = "22"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0027",	MO_ORDER = "13384315",	MO_NO = "21"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0075",	MO_ORDER = "13388589",	MO_NO = "254"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0075",	MO_ORDER = "13388588",	MO_NO = "253"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0111",	MO_ORDER = "13388713",	MO_NO = "345"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0042",	MO_ORDER = "13387129",	MO_NO = "100"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0049",	MO_ORDER = "13387404",	MO_NO = "115"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0049",	MO_ORDER = "13387403",	MO_NO = "114"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0101",	MO_ORDER = "13388623",	MO_NO = "321"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0101",	MO_ORDER = "13388624",	MO_NO = "322"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0034",	MO_ORDER = "13386858",	MO_NO = "8"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0034",	MO_ORDER = "13386857",	MO_NO = "7"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0034",	MO_ORDER = "13386856",	MO_NO = "6"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0034",	MO_ORDER = "13386855",	MO_NO = "5"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0034",	MO_ORDER = "13386854",	MO_NO = "4"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0125",	MO_ORDER = "13388670",	MO_NO = "367"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0125",	MO_ORDER = "13388669",	MO_NO = "366"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0125",	MO_ORDER = "13388668",	MO_NO = "365"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0125",	MO_ORDER = "13388667",	MO_NO = "364"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0094",	MO_ORDER = "13388610",	MO_NO = "308"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0094",	MO_ORDER = "13388609",	MO_NO = "307"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0131",	MO_ORDER = "13388711",	MO_NO = "388"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0131",	MO_ORDER = "13388710",	MO_NO = "387"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0131",	MO_ORDER = "13388709",	MO_NO = "386"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0131",	MO_ORDER = "13388708",	MO_NO = "385"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0099",	MO_ORDER = "13388620",	MO_NO = "318"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0099",	MO_ORDER = "13388619",	MO_NO = "317"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0033",	MO_ORDER = "13386859",	MO_NO = "9"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0077",	MO_ORDER = "13388585",	MO_NO = "256"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0122",	MO_ORDER = "13388438",	MO_NO = "363"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0083",	MO_ORDER = "13388634",	MO_NO = "278"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0083",	MO_ORDER = "13388633",	MO_NO = "277"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0025",	MO_ORDER = "13384312",	MO_NO = "18"});
            //lotMoList.Add(new LotMoEntity{ LOT_NO = "20190918AS0025",	MO_ORDER = "13384311",	MO_NO = "17"});
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
