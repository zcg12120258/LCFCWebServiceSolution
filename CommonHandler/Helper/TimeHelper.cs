using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonHandler.Helper
{
    public static class TimeHelper
    {
        /// <summary>
        /// DateTime格式化
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTimeFormat DateTimeFormatConvert(this DateTime dt)
        {
            DateTimeFormat dateTimeFormat = new DateTimeFormat();
            dateTimeFormat.TimeSelf = dt;
            dateTimeFormat.dateTimeFormat = dt.ToString("yyyyMMdd");
            dateTimeFormat.dateTimeFormat1 = dt.ToString("yyyy-MM-dd");
            dateTimeFormat.dateTimeFormat2 = dt.ToString("yyyy/MM/dd");
            dateTimeFormat.dateTimeFormat3 = dt.ToString("yyyy/MM");
            dateTimeFormat.dateTimeLastMonth = dt.AddMonths(-1).ToString("yyyy/MM");
            dateTimeFormat.dateTimeYYYYMMDDHHMMSS = dt.ToString("yyyyMMddHHmmss");
            dateTimeFormat.dayOfWeek = dt.DayOfWeek.ToString();
            dateTimeFormat.firstDayOfMonth = new DateTime(dt.Year, dt.Month, 1);
            dateTimeFormat.lastDayOfMonth = dateTimeFormat.firstDayOfMonth.AddMonths(1).AddDays(-1);
            int dayOfWeekIndex = GetWeekNumberOfDay(dt);
            dateTimeFormat.MondayDate = dt.AddDays(1 - dayOfWeekIndex).Date;
            return dateTimeFormat;
        }

        /// <summary>
        /// 判断当前时间
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static bool IsRange(this DateTime dt,DateTime startTime,DateTime endTime)
        {
            if ((dt - startTime).TotalSeconds > 0 && (endTime - dt).TotalSeconds > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>返回当前日期的星期编号</summary>
        /// <param name="dt">日期</param>
        /// <returns>星期数字编号</returns>
        public static int GetWeekNumberOfDay(DateTime dt)
        {
            int week = 0;
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    week = 1;
                    break;
                case DayOfWeek.Tuesday:
                    week = 2;
                    break;
                case DayOfWeek.Wednesday:
                    week = 3;
                    break;
                case DayOfWeek.Thursday:
                    week = 4;
                    break;
                case DayOfWeek.Friday:
                    week = 5;
                    break;
                case DayOfWeek.Saturday:
                    week = 6;
                    break;
                case DayOfWeek.Sunday:
                    week = 7;
                    break;
            }
            return week;
        }

    }

    public class DateTimeFormat
    {
        /// <summary>
        /// 实际时间值
        /// </summary>
        public DateTime TimeSelf { get; set; }

        /// <summary>
        /// yyyyMMdd时间格式
        /// </summary>
        public string dateTimeFormat { get; set; }

        /// <summary>
        /// yyyy-MM-dd时间格式
        /// </summary>
        public string dateTimeFormat1 { get; set; }

        /// <summary>
        /// yyyy/MM/dd时间格式
        /// </summary>
        public string dateTimeFormat2 { get; set; }
        /// <summary>
        /// yyyy/MM时间格式
        /// </summary>
        public string dateTimeFormat3 { get; set; }

        /// <summary>
        /// yyyy/MM时间格式
        /// </summary>
        public string dateTimeLastMonth { get; set; }

        /// <summary>
        /// yyyyMMddHHmmss
        /// </summary>
        public string dateTimeYYYYMMDDHHMMSS { get; set; }
        /// <summary>
        /// 周几
        /// </summary>
        public string dayOfWeek { get; set; }
        /// <summary>
        /// 某月第一天
        /// </summary>
        public DateTime firstDayOfMonth { get; set; }
        /// <summary>
        /// 某月最后一天
        /// </summary>
        public DateTime lastDayOfMonth { get; set; }
        /// <summary>
        /// 获取当前日期的周一时间
        /// </summary>
        public DateTime MondayDate { get; set; }

    }
}
