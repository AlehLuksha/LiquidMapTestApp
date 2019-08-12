using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidMapTestApp
{
    class AzureLiquidSyntax
    {
        public static readonly string[][] _syntaxExceptions_filters = new string[][]
        {
            new []{ "StripNewlines", "strip_newlines" },
            new []{ "ReplaceFirst", "replace_first" }
        };

        public static readonly string[] Filters = new string[]
        {
            // array filters
            "Join", "First", "Last", "Concat", "Index", "Map", "Reverse", "Size", "Sort", "Where", "Uniq", "Reverse",
            // string filters
            "Append","camelcase","capitalize","Downcase","escape","handle/handleize","md5","sha1","sha256","hmac_sha1","hmac_sha256","newline_to_br","pluralize","Prepend","remove","remove_first","Replace","ReplaceFirst","slice","Split","strip","lstrip","rstrip","strip_html","StripNewLines","Truncate","TruncateWords","Upcase","url_encode","url_escape","url_param_escape",
            // math filters
            "abs","at_most","at_least","ceil","divided_by","floor","minus","plus","Round","times","modulo",
            // money filters
            "money","money_with_currency","money_without_trailing_zeros","money_without_currency",
            // URL filters
            // Additional filters
            "Date", "Default", "default_errors", "default_pagination", "format_address", "highlight", "highlight_active_tag", "json", "weight_with_unit", "placeholder_svg_tag",
            // Color, Font, Html filters
        };
    }
}
