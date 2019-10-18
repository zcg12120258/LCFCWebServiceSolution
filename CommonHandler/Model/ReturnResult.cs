using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonHandler.Model
{
    /// <summary>
    /// 返回值类
    /// </summary>
    public class ReturnResult
    {

        public bool Status { get; set; }
        public string Message { get; set; }
        public object Anything { get; set; }
    }
}
