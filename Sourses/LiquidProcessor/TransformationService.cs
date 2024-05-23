using System;
using System.Collections.Generic;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LiquidProcessor.Core.Interfaces;

namespace DotLiquidProcessor
{
    /// <summary>The service to transform Json to Text\Json using liquid templates</summary>
    public class TransformationService: ITransformationService<Template>
    {
        /// <summary>Parses the liquid template.</summary>
        /// <param name="log">The log.</param>
        /// <param name="liquidTemplate">The liquid template.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>Template object</returns>
        public Template ParseTemplate(ILogger log, string liquidTemplate, bool useRubyNamingConvention, out string errorMessage)
        {
            if (string.IsNullOrEmpty(liquidTemplate))
            {
                errorMessage = "Liquid template is required.";
                return null;
            }

            if (!useRubyNamingConvention)
            {
                Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            }

            // register custom filters
            Template.RegisterFilter(typeof(CustomFilters));

            try
            {
                var template = Template.Parse(liquidTemplate);
                template.MakeThreadSafe();
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

        /// <summary>Transforms the json to text.</summary>
        /// <param name="log">The log.</param>
        /// <param name="template">The template.</param>
        /// <param name="jsonData">The json data.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>Transformed string</returns>
        public string TransformJsonToText(ILogger log, Template template, string jsonData, string rootElement,
            out string errorMessage)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                errorMessage = "Json data is required.";
                return null;
            }

            Hash inputHash;
            try
            {
                var transformInput = new Dictionary<string, object>();
                var requestJson =
                    JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonData, new DictionaryConverter());

                // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
                transformInput.Add(rootElement, requestJson);

                inputHash = Hash.FromDictionary(transformInput);
            }
            catch (Exception ex)
            {
                if (log != null)
                {
                    log.LogError(ex.Message, ex);
                }
                errorMessage = $"Error parsing Json data: {ex.Message}";
                return null;
            }

            string output;

            try
            {
                output = template.Render(inputHash);
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
