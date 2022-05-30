using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    public static partial class Extensions
    {
        public static bool HasAnyFlag(this Enum value, Enum flags)
        {
            return value != null && ((Convert.ToInt32(value) & Convert.ToInt32(flags)) != 0);
        }
        public static string ToOrdinalString(this int ordinal)
        {
            var suffix = "th";
            string number = ordinal.ToString();
            if (number.EndsWith("1") && !number.EndsWith("11")) suffix = "st";
            if (number.EndsWith("2") && !number.EndsWith("12")) suffix = "nd";
            if (number.EndsWith("3") && !number.EndsWith("13")) suffix = "rd";
            return number + suffix;
        }
    }
}
