using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TransformFunctionApp.Models;
using TransformFunctionApp.Services;

namespace TransformFunctionApp.Functions
{
    public static class TransformJson
    {
        [FunctionName("TransformJson")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                var liquidTemplate = data?.liquidTemplate?.ToString();
                var jsonData = data.jsonData?.ToString();


                var output = TransformationService.TransformJsonToJson(log, liquidTemplate, jsonData, out TransformError transformError);
                
                if (transformError != null)
                {
                    var jsonResponse = JObject.FromObject(transformError);

                    return new BadRequestObjectResult(jsonResponse);
                }

                return new JsonResult(output);

            }
            catch (Exception e)
            {
                var errorMessage = e.Message;

                var response = new TransformError()
                {
                    Code = @"TransformException",
                    Message = errorMessage,
                };

                var jsonResponse = JObject.FromObject(response);

                return new BadRequestObjectResult(jsonResponse);
            }

        }



    }
}
