namespace FluidProcessor.Liquid.LiquidRegistrations;

using Fluid;
using Fluid.Ast;

public static class AddRootContentTagRegistration
{
    private static readonly string RootTagName = "addRootTag";

    private static readonly string RootTagValue = "content";

    public static void RegisterCustomizations(FluidParser parser)
    {
        parser.RegisterEmptyTag(RootTagName, (writer, encoder, context) =>
        {
            context.SetValue(RootTagValue, context.Model);
            return Statement.Normal();
        });
    }
}
