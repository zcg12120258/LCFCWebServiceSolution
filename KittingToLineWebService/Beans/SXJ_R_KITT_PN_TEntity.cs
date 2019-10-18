using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KittingToLineWebService.Beans
{
    /// <summary>
    /// 存储在MES的架位信息表,实体类
    /// </summary>
    public class SXJ_R_KITT_PN_TEntity
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
        /// 物料号
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
        /// 创建时间
        /// </summary>
        public string CREATE_TIME { get; set; }
        /// <summary>
        /// 线体
        /// </summary>
        public string LINE_NAME { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CREATE_NAME { get; set; }
        /// <summary>
        /// 预留字段1
        /// </summary>
        public string DESC1 { get; set; }
        /// <summary>
        /// 预留字段2
        /// </summary>
        public string DESC2 { get; set; }
        /// <summary>
        /// 预留字段3
        /// </summary>
        public string DESC3 { get; set; }

    }
}