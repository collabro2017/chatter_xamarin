using System;
using System.Collections.Generic;
using System.Text;

namespace Chadder.Client.Util
{
    public static class Extensions
    {
        public static string TimeOnly(this DateTime local)
        {
            return local.ToString("h:mmtt");
        }
    }
    public class ChadderHelper
    {
        public static string GetNiceDateFormat(DateTime time)
        {
            time = DateTime.SpecifyKind(time, DateTimeKind.Utc).ToLocalTime();
            if (time.Date == DateTime.Today)
                return time.TimeOnly();
            if (time.Date - DateTime.Today < TimeSpan.FromDays(7))
                return time.TimeOnly() + time.ToString(", ddd");
            return time.TimeOnly() + time.ToString(", MMM d");
        }
    }
}
