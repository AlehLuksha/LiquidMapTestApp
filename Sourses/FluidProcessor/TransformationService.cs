using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Fluid;
using FluidProcessor.Liquid.LiquidRegistrations;
using LiquidProcessor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FluidProcessor
{
    public class TransformationService : ITransformationService<IFluidTemplate>, ILiquidTransformationService
    {
        public string Transform(string templateString, string jsonData, string rootElement, bool useRubyNamingConvention, out string errorMessage)
        {
            var template = ParseTemplate(null, templateString, false, out errorMessage);

            if (template != null && string.IsNullOrEmpty(errorMessage)) 
            {
                return TransformJsonToText(null, template, jsonData, rootElement, out errorMessage);
            }
            else
            {
                return "Error";
            }

        }

        public IFluidTemplate ParseTemplate(ILogger log, string liquidTemplate, bool useRubyNamingConvention, out string errorMessage)
        {
            if (string.IsNullOrEmpty(liquidTemplate))
            {
                errorMessage = "Liquid template is required.";
                return null;
            }

            // register custom filters
            TimeZoneCastFilterRegistration.RegisterCustomizations(TemplateOptions.Default);

            var parser = new FluidParser();
            // register custom tags
            AddRootContentTagRegistration.RegisterCustomizations(parser);
            GenerateUniqueIdTagRegistration.RegisterCustomizations(parser);

            try
            {
                // parse template
                if (parser.TryParse(liquidTemplate, out var template, out errorMessage))
                {
                    return template;
                }
                else
                    return null;
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
                var modifiedData = jsonData;

                // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
                if (!string.IsNullOrEmpty(rootElement))
                {
                    modifiedData = "{" + rootElement + ":" + jsonData + "}";
                }

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
