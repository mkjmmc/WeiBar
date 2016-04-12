using System;

namespace BuTian.Utility
{
    /// <summary>
    /// 日期时间工具类
    /// </summary>
    public class DateTimeUtility
    {
        #region 将时间转换成毫秒数
        /// <summary>
        /// 将时间转换成毫秒数
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>毫秒数</returns>
        public static long GetTimeMilliseconds(DateTime time)
        {
            return (long)time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        #endregion

        /// <summary>
        /// 根据秒数获取时间
        /// </summary>
        /// <param name="seconds">秒数</param>
        /// <returns>时间</returns>
        public static DateTime GetTimeWithSeconds(long seconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(seconds);
        }

        #region 将毫秒数转化为DateTime时间
        /// <summary>
        /// 将毫秒数转化为DateTime时间
        /// </summary>
        /// <param name="milliseconds">毫秒数</param>
        /// <returns>时间</returns>
        public static DateTime GetTime(long milliseconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(milliseconds).ToLocalTime();
        }
        #endregion

        #region 换算计算时间为字符串

        /// <summary>
        /// 换算计算时间为字符串：今天，昨天等
        /// </summary>
        /// <param name="milliseconds">时间的毫秒数</param>
        /// <param name="lang">语言 zh  en</param>
        /// <returns></returns>
        public static string GetTimeString(long milliseconds, string lang = "zh")
        {
            var _time = GetTime(milliseconds);
            var _now = DateTime.Now;
            TimeSpan _timespan = _now - _time;

            switch (lang)
            {
                case "en":
                    if (_timespan < new TimeSpan(0, 0, 0))
                    {
                        // 未来时间
                        if (_time.Date == _now.Date)
                        {
                            // 一天以内
                            return "Today" + _time.ToString(" HH:mm"); // Today
                        }
                        if (_time.Date == _now.Date.AddDays(1))
                        {
                            // 一天以内
                            return "Tomorrow" + _time.ToString(" HH:mm");  // Tomorrow
                        }
                        if (_time.Year == _now.Year)
                        {
                            // 一年内 
                            return _time.ToString("MM-dd HH:mm");
                        }
                        return _time.ToString("yyyy-MM-dd HH:mm");
                    }
                    if (_timespan < new TimeSpan(0, 1, 0))
                    {
                        return "Just Now"; // just now
                    }
                    if (_timespan < new TimeSpan(1, 0, 0))
                    {
                        // 1小时以内
                        return _timespan.Minutes + (_timespan.Minutes == 1 ? " minute ago" : " mins ago");  // mins ago  minute
                    }
                    if (_time.Date == _now.Date)
                    {
                        // 一天以内
                        return "Today" + _time.ToString(" HH:mm"); // Today
                    }
                    if (_time.Date == _now.AddDays(-1).Date)
                    {
                        // 昨天
                        return "Yesterday" + _time.ToString(" HH:mm");  //Yesterday
                    }
                    if (_time.Year == _now.Year)
                    {
                        // 一年内 
                        return _time.ToString("MM-dd HH:mm");
                    }
                    return _time.ToString("yyyy-MM-dd HH:mm");
                default:
                    if (_timespan < new TimeSpan(0, 0, 0))
                    {
                        // 未来时间
                        if (_time.Date == _now.Date)
                        {
                            // 一天以内
                            return "今天" + _time.ToString(" HH:mm"); // Today
                        }
                        if (_time.Date == _now.Date.AddDays(1))
                        {
                            // 一天以内
                            return "明天" + _time.ToString(" HH:mm");  // Tomorrow
                        }
                        if (_time.Year == _now.Year)
                        {
                            // 一年内 
                            return _time.ToString("MM-dd HH:mm");
                        }
                        return _time.ToString("yyyy-MM-dd HH:mm");
                    }
                    if (_timespan < new TimeSpan(0, 1, 0))
                    {
                        return "刚刚"; // just now
                    }
                    if (_timespan < new TimeSpan(1, 0, 0))
                    {
                        // 1小时以内
                        return _timespan.Minutes + "分钟前";  // mins ago  minute
                    }
                    if (_time.Date == _now.Date)
                    {
                        // 一天以内
                        return "今天" + _time.ToString(" HH:mm"); // Today
                    }
                    if (_time.Date == _now.AddDays(-1).Date)
                    {
                        // 昨天
                        return "昨天" + _time.ToString(" HH:mm");  //Yesterday
                    }
                    if (_time.Year == _now.Year)
                    {
                        // 一年内 
                        return _time.ToString("MM-dd HH:mm");
                    }
                    return _time.ToString("yyyy-MM-dd HH:mm");
            }

        }

        /// <summary>
        /// 换算计算时间为字符串：今天，昨天等,一个月前的均显示 一个月前
        /// </summary>
        /// <param name="milliseconds">时间毫秒数</param>
        /// <param name="lang">语言</param>
        /// <returns></returns>
        public static string GetTimeString2(long milliseconds, string lang = "zh")
        {
            var _time = GetTime(milliseconds);
            var _now = DateTime.Now;
            TimeSpan _timespan = _now - _time;

            switch (lang)
            {
                case "en":
                    if (_timespan < new TimeSpan(0, 0, 0))
                    {
                        // 未来时间
                        if (_time.Date == _now.Date)
                        {
                            // 一天以内
                            return "Today" + _time.ToString(" HH:mm");
                        }
                        if (_time.Date == _now.Date.AddDays(1))
                        {
                            // 一天以内
                            return "Tomorrow" + _time.ToString(" HH:mm");
                        }
                        if (_time.Year == _now.Year)
                        {
                            // 一年内 
                            return _time.ToString("MM-dd HH:mm");
                        }
                        return _time.ToString("yyyy-MM-dd HH:mm");
                    }
                    if (_timespan < new TimeSpan(0, 1, 0))
                    {
                        return "Just Now";
                    }
                    if (_timespan < new TimeSpan(1, 0, 0))
                    {
                        // 1小时以内
                        return _timespan.Minutes + (_timespan.Minutes == 1 ? " minute ago" : " mins ago");  // mins ago  minute
                    }
                    if (_time.Date == _now.Date)
                    {
                        // 一天以内
                        return "Today" + _time.ToString(" HH:mm"); // Today
                    }
                    if (_time.Date == _now.AddDays(-1).Date)
                    {
                        // 昨天
                        return "Yesterday" + _time.ToString(" HH:mm");  //Yesterday
                    }
                    if (_timespan < new TimeSpan(30, 0, 0, 0))
                    {
                        return _time.ToString("MM-dd HH:mm");
                    }
                    // 一年内 
                    return _time.ToString("1 month ago");
                default:
                    if (_timespan < new TimeSpan(0, 0, 0))
                    {
                        // 未来时间
                        if (_time.Date == _now.Date)
                        {
                            // 一天以内
                            return "今天" + _time.ToString(" HH:mm");
                        }
                        if (_time.Date == _now.Date.AddDays(1))
                        {
                            // 一天以内
                            return "明天" + _time.ToString(" HH:mm");
                        }
                        if (_time.Year == _now.Year)
                        {
                            // 一年内 
                            return _time.ToString("MM-dd HH:mm");
                        }
                        return _time.ToString("yyyy-MM-dd HH:mm");
                    }
                    if (_timespan < new TimeSpan(0, 1, 0))
                    {
                        return "刚刚";
                    }
                    if (_timespan < new TimeSpan(1, 0, 0))
                    {
                        // 1小时以内
                        return _timespan.Minutes + "分钟前";
                    }
                    if (_time.Date == _now.Date)
                    {
                        // 一天以内
                        return "今天" + _time.ToString(" HH:mm");
                    }
                    if (_time.Date == _now.AddDays(-1).Date)
                    {
                        // 昨天
                        return "昨天" + _time.ToString(" HH:mm");
                    }
                    if (_timespan < new TimeSpan(30, 0, 0, 0))
                    {
                        return _time.ToString("MM-dd HH:mm");
                    }
                    // 一年内 
                    return _time.ToString("一个月前");
            }
        }

        #endregion

        #region 计算年龄
        /// <summary>
        /// 计算实际年龄
        /// </summary>
        /// <param name="birthday"></param>
        /// <returns></returns>
        public static int GetAge(long birthday)
        {
            DateTime _dt = GetTime(birthday);
            var _now = DateTime.Now;
            var _age = _now.Year - _dt.Year;
            if (_dt.AddYears(_age) < _now)
            {
                return _age + 1;
            }
            return _age;
        }
        #endregion


    }
}
