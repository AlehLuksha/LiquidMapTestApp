using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidMapTestApp
{
    public class AzureLiquidHelper
    {
        public static string ConvertFromAzureLiquidSyntax(string text)
        {
            // operators
            text = text.Replace(" Contains ", " contains ");

            //filters
            foreach (var filter in AzureLiquidSyntax.Filters)
            {
                text = text
                    .Replace("| " + filter, "| " + filter.ToLower())
                    .Replace("|" + filter, "|" + filter.ToLower());
            }

            //replace syntax exceptions
            foreach (var token in AzureLiquidSyntax._syntaxExceptions_filters)
            {
                text = text
                    .Replace("| " + token[0], "| " + token[1].ToLower())
                    .Replace("|" + token[0], "|" + token[1].ToLower());
            }

            return text;
        }

        public static string ConvertToAzureLiquidSyntax(string text)
        {
            text = text.Replace("contains", "Contains");

            foreach (var filter in AzureLiquidSyntax.Filters)
            {
                text = text.Replace(filter, filter.ToUpper());
            }

            return text;
        }
    }
}
