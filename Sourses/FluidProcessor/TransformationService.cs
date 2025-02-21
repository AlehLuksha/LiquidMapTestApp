using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Fluid;
using LiquidProcessor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FluidProcessor
{
    public class TransformationService : ITransformationService<IFluidTemplate>
    {
        public IFluidTemplate ParseTemplate(ILogger log, string liquidTemplate, bool useRubyNamingConvention, out string errorMessage)
        {
            if (string.IsNullOrEmpty(liquidTemplate))
            {
                errorMessage = "Liquid template is required.";
                return null;
            }

            if (!useRubyNamingConvention)
            {
                //FluidParser.NamingConvention = new NamingConventions.CSharpNamingConvention();
            }

            // register custom filters
            //

            try
            {
                // parse template
                var parser = new FluidParser();
                var template = parser.Parse(liquidTemplate);
                errorMessage = null;

                return template;
            }
            catch (Exception ex)
            {
                if (log != null)
                {
                    log.LogError(ex.Message, ex);
                }
                errorMessage = $"Error parsing Liquid template: {ex.Message}";
                return null;
            }
        }

        public string TransformJsonToText(ILogger log, IFluidTemplate template, string jsonData, string rootElement, out string errorMessage)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                errorMessage = "Json data is required.";
                return null;
            }

            string output;

            try
            {
                // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
                var modifiedData = "{" + rootElement + ":" + jsonData + "}";

                dynamic jObj = JsonConvert.DeserializeObject(modifiedData);
                var context = new TemplateContext(jObj);

                output = template.Render(context);
            }
            catch (Exception ex)
            {
                if (log != null)
                {
                    log.LogError(ex.Message, ex);
                }
                errorMessage = $"Error rendering Liquid template: {ex.Message}";
                return null;
            }

            errorMessage = null;
            return output;
        }
    }
}
