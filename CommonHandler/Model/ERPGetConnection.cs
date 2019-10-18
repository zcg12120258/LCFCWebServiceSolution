using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonHandler.Model
{
    /// <summary>
    /// ERP连接
    /// </summary>
    public class ERPGetConnection
    {
        public string USER { get; set; }
        public string PASSWD { get; set; }
        public string LANG { get; set; }
        public string CLIENT { get; set; }
        public string ASHOST { get; set; }
        public string SYSNR { get; set; }
        public string MSHOST { get; set; }
        public string R3NAME { get; set; }
        public string GROUP { get; set; }
    }
}
