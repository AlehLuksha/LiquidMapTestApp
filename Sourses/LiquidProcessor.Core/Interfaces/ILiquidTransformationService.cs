using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidProcessor.Core.Interfaces
{
    public interface ILiquidTransformationService
    {
        string Transform(string template, string jsonData, string rootElement, bool useRubyNamingConvention, out string errorMessage);
    }
}
