using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KittingToLineWebService.Beans
{
    /// <summary>
    /// LotAndMo实体类
    /// </summary>
    public class LotMoEntity
    {
        /// <summary>
        /// Lot编号
        /// </summary>
        public string LOT_NO { get; set; }
        /// <summary>
        /// MO排序号
        /// </summary>
        public string MO_NO { get; set; }

        /// <summary>
        /// Mo编号
        /// </summary>
        public string MO_ORDER { get; set; }
        /// <summary>
        /// 线体名称
        /// </summary>
        public string LINE_NAME { get; set; }

        /// <summary>
        /// 班次
        /// </summary>
        public string SHIFT { get; set; }

        /// <summary>
        /// 生产时间
        /// </summary>
        public string CREATE_TIME { get; set; }


        /// <summary>
        /// Lot排序号
        /// </summary>
        public string LOT_SEQ { get; set; }

        public string JOB_STATUS { get; set; }
    }

    public class LotEntity
    {
        /// <summary>
        /// Lot编号
        /// </summary>
        public string LOT_NO { get; set; }
        /// <summary>
        /// MO排序号
        /// </summary>
        public string MO_NO { get; set; }

        /// <summary>
        /// Mo编号
        /// </summary>
        public string MO_ORDER { get; set; }
    }

    public class LotMoEntityGroup
    {
        
        /// <summary>
        /// 线体名称
        /// </summary>
        public string LINE_NAME { get; set; }

        /// <summary>
        /// 班次
        /// </summary>
        public string SHIFT { get; set; }

        /// <summary>
        /// 生产时间
        /// </summary>
        public string CREATE_TIME { get; set; }

        public List<LotMoEntity> LotMoEntityList { get; set; }

        public List<LotMoEntity> LotMoEntityListSort { get; set; }
    }
}