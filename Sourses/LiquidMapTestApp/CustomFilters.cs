using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidMapTestApp
{
    public static class CustomFilters
    {
        public static string Format(object input, string format)
        {
            if (input == null)
                return null;
            else if (string.IsNullOrWhiteSpace(format))
                return input.ToString();

            var result = string.Format("{0:" + format + "}", input);
            return result;
        }
    }
}
