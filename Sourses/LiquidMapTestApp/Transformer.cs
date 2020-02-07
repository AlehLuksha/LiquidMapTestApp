using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiquidMapTestApp
{
    public class Transformer
    {
        public readonly string Template;
        public readonly bool UseRubyNamingConvention;
        public readonly DotLiquid.Template LiquidTemplate;

        public Transformer(string template, bool useRubyNamingConvention = false)
        {
            Template = template;
            UseRubyNamingConvention = useRubyNamingConvention;
            if (!useRubyNamingConvention)
            {
                DotLiquid.Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            }
            LiquidTemplate = DotLiquid.Template.Parse(template);
            LiquidTemplate.MakeThreadSafe();

        }

        //public string RenderFromString(string content, string rootElement = null)
        //{
        //    Dictionary<string, object> dicContent;
        //    JsonSerializerSettings sets = new JsonSerializerSettings
        //    {
        //        CheckAdditionalContent = true,
        //        MaxDepth = null
        //    };
        //    var jo = JObject.Parse(content);
        //    var dic = jo.ToDictionary();

        //    if (rootElement is null)
        //    {
        //        dicContent = (Dictionary<string, object>)dic;
        //    }
        //    else
        //    {
        //        dicContent = new Dictionary<string, object>
        //        {
        //            { rootElement, dic }
        //        };
        //    }
        //    var obj = DotLiquid.Hash.FromDictionary(dicContent);
        //    return LiquidTemplate.Render(obj);
        //}

    }
}
