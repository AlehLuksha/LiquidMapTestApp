namespace LiquidMapTestApp
{
    public class LiquidSyntax
    {
        public static string[] Keywords = new[]
        {
            "assign", "capture", "endcapture", "increment", "decrement",
            "if", "else", "elsif", "endif",
            "case", "when", "endcase",
            "comment", "endcomment",
            "unless", "endunless",
            "for", "break", "continue", "endfor",
            "cycle", "tablerow", "endtablerow", "raw", "endraw",
            "blank", "null", "and", "or", "true", "false"
        };

        public static string[] Filters = new[]
        {
            // array filters
            "join", "first", "last", "concat", "index", "map", "reverse", "size", "sort", "where", "uniq", "reverse",
            // string filters
            "append","camelcase","capitalize","downcase","escape","handle/handleize","md5","sha1","sha256","hmac_sha1","hmac_sha256","newline_to_br","pluralize","prepend","remove","remove_first","replace","replace_first","slice","split","strip","lstrip","rstrip","strip_html","strip_newlines","truncate","truncatewords","upcase","url_encode","url_escape","url_param_escape",
            // math filters
            "abs","at_most","at_least","ceil","divided_by","floor","minus","plus","round","times","modulo",
            // money filters
            "money","money_with_currency","money_without_trailing_zeros","money_without_currency",
            // URL filters
            // Additional filters
            "date", "default", "default_errors", "default_pagination", "format_address", "highlight", "highlight_active_tag", "json", "weight_with_unit", "placeholder_svg_tag",
            // Color, Font, Html filters
        };

        public static string[] CustomFilters = new[]
        {
            // my custom filters
            "format", "where_is_null", "null_if_empty", "null_if_empty_else_string"
        };

    }
}
