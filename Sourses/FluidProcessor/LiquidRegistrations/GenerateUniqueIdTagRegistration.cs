namespace FluidProcessor.Liquid.LiquidRegistrations;

using System;
using Fluid;
using Fluid.Ast;

public static class GenerateUniqueIdTagRegistration
{
    public static void RegisterCustomizations(FluidParser parser)
    {
        parser.RegisterEmptyTag("generateUniqueId", (writer, encoder, context) =>
        {
            writer.Write($"\"{Guid.NewGuid()}\"");
            return Statement.Normal();
        });
    }
}
