using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonHandler.Setting
{
    /// <summary>
    /// Declaration of system settings.
    /// </summary>
    public static class SystemSetting
    {
        public static string ConnectionString = System.Configuration.ConfigurationManager.AppSettings["connstrbox_prd"]??"";
    }
}
