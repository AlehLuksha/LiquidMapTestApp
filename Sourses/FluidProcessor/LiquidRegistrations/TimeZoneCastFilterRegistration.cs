namespace FluidProcessor.Liquid.LiquidRegistrations;

using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

public class TimeZoneCastFilterRegistration
{
    private const string FilterName = "time_zone_cast";

    public static void RegisterCustomizations(TemplateOptions options)
    {
        options.Filters.AddFilter(FilterName, AdjustTimezoneTo);
    }

    private static ValueTask<FluidValue> AdjustTimezoneTo(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        var originalDateTime = DateTimeOffset.Parse(input.ToStringValue(), CultureInfo.InvariantCulture);

        var argument = arguments.At(0);
        var argumentValue = argument.ToStringValue();
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(argumentValue);
        var castedTime = TimeZoneInfo.ConvertTime(originalDateTime, timezone);

        return new DateTimeValue(castedTime);
    }
}
