using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KittingToLineWebService.Beans
{
    /// <summary>
    /// 上下架实体类
    /// </summary>
    public class SXJ_WMSEntity
    {
        /// <summary>
        /// Lot号
        /// </summary>
        public string MOGP { get; set; }
        /// <summary>
        /// 物料号
        /// </summary>
        public string MATERIAL { get; set; }
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string VENDORCODE { get; set; }
        /// <summary>
        /// 需求数量
        /// </summary>
        public int QTY { get; set; }
        /// <summary>
        /// 上下架数量
        /// </summary>
        public int ONRACKQTY { get; set; }
        /// <summary>
        /// Rack储位
        /// </summary>
        public string RACKNO { get; set; }
        
        /// <summary>
        /// 创建人
        /// </summary>
        public string USER { get; set; }
        /// <summary>
        /// 返回状态
        /// </summary>
        public int STATUS { get; set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public string STATUS_INFO { get; set; }

    }
}