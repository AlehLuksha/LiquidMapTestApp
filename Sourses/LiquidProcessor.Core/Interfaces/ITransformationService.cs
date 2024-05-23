using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidProcessor.Core.Interfaces
{
    public interface ITransformationService<T>
    {
        T ParseTemplate(ILogger log, string liquidTemplate, bool useRubyNamingConvention, out string errorMessage);

        string TransformJsonToText(ILogger log, T template, string jsonData, string rootElement,
            out string errorMessage);
    }
}
