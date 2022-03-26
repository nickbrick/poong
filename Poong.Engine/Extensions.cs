using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    internal static partial class Extensions
    {
        public static bool HasAnyFlag(this Enum value, Enum flags)
        {
            return value != null && ((Convert.ToInt32(value) & Convert.ToInt32(flags)) != 0);
        }
    }
}
