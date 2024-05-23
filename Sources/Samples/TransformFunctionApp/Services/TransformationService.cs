using System;
using System.Collections.Generic;
using DotLiquid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TransformFunctionApp.Helpers;
using TransformFunctionApp.Models;

namespace TransformFunctionApp.Services
{
    /// <summary>The service to transform Json to Text\Json using liquid templates</summary>
    public class TransformationService
    {
        /// <summary>Parses the liquid template.</summary>
        /// <param name="log">The log.</param>
        /// <param name="liquidTemplate">The liquid template.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>Template object</returns>
        public static Template ParseTemplate(ILogger log, string liquidTemplate, out string errorMessage)
        {
            if (string.IsNullOrEmpty(liquidTemplate))
            {
                errorMessage = "Liquid template is required.";
                return null;
            }

            // Execute the Liquid transform
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();

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
                { log.LogError(ex.Message, ex);
                }
                errorMessage = $"Error parsing Liquid template: {ex.Message}";
                return null;
            }

        }

        /// <summary>Transforms the json to text.</summary>
        /// <param name="log">The log.</param>
        /// <param name="liquidTemplate">The liquid template.</param>
        /// <param name="jsonData">The json data.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>Transformed string</returns>
        public static string TransformJsonToText(ILogger log, string liquidTemplate, string jsonData,
            out string errorMessage)
        {
            if (string.IsNullOrEmpty(liquidTemplate))
            {
                errorMessage = "Liquid template is required.";
                return null;
            }

            if (string.IsNullOrEmpty(jsonData))
            {
                errorMessage = "Json data is required.";
                return null;
            }

            var template = ParseTemplate(log, liquidTemplate, out string templateErrorMessage);
            if (!string.IsNullOrEmpty(templateErrorMessage) || template == null)
            {
                errorMessage = templateErrorMessage;
                return null;
            }

            var output = TransformJsonToText(log, template, jsonData, out errorMessage);

            return output;
        }

        /// <summary>Transforms the json to text.</summary>
        /// <param name="log">The log.</param>
        /// <param name="template">The template.</param>
        /// <param name="jsonData">The json data.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>Transformed string</returns>
        public static string TransformJsonToText(ILogger log, Template template, string jsonData,
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
                transformInput.Add("content", requestJson);

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

        /// <summary>Transforms the json to json.</summary>
        /// <param name="log">The log.</param>
        /// <param name="liquidTemplate">The liquid template.</param>
        /// <param name="jsonData">The json data.</param>
        /// <param name="error">The error.</param>
        /// <returns>Transformed Json string</returns>
        public static dynamic TransformJsonToJson(ILogger log, string liquidTemplate, string jsonData,
            out TransformError error)
        {
            var output = TransformationService.TransformJsonToText(log, liquidTemplate, jsonData, out var errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                error = new TransformError()
                {
                    Code = @"TransformException",
                    Message = errorMessage,
                };
                return String.Empty;
            }

            try
            {
                var jsonObject = JsonObject(output);
                error = null;
                //output = Regex.Replace(output, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                return jsonObject;
            }
            catch (Exception e)
            {
                errorMessage =
                    $"An error occurred while converting the transformed value to JSON. The transformed value is not a valid JSON. '{e.Message}'";

                error = new TransformError()
                {
                    Code = @"IncorrectLiquidTransformOutputType",
                    Message = errorMessage,
                };
                return string.Empty;
            }
        }

        public static dynamic TransformJsonToJson(ILogger log, Template template, string jsonData,
            out TransformError error)
        {
            var output = TransformationService.TransformJsonToText(log, template, jsonData, out var errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                error = new TransformError()
                {
                    Code = @"TransformException",
                    Message = errorMessage,
                };
                return String.Empty;
            }

            try
            {
                var jsonObject = JsonObject(output);
                error = null;
                //output = Regex.Replace(output, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                return jsonObject;
            }
            catch (Exception e)
            {
                errorMessage =
                    $"An error occurred while converting the transformed value to JSON. The transformed value is not a valid JSON. '{e.Message}'";

                error = new TransformError()
                {
                    Code = @"IncorrectLiquidTransformOutputType",
                    Message = errorMessage,
                };
                return string.Empty;
            }
        }

        public static dynamic JsonObject(string output)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include, 
                StringEscapeHandling = StringEscapeHandling.Default
            };

            dynamic jsonObject = JsonConvert.DeserializeObject(output, jsonSerializerSettings);
            return jsonObject;
        }
    }

    /// <summary>Additional custom filters</summary>
    public static class CustomFilters
    {
        /// <summary>Formats the specified input object.</summary>
        /// <param name="input">The input.</param>
        /// <param name="format">The format.</param>
        /// <returns>
        ///   <br />
        /// </returns>
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
