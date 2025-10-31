using DotLiquid;
using Fluid;
using LiquidProcessor.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace LiquidMapTestApp
{
    static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            Application.Run(ServiceProvider.GetRequiredService<MainForm>());
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Register your services here
            services.AddTransient<ITransformationService<Template>, DotLiquidProcessor.TransformationService>();
            services.AddTransient<ITransformationService<IFluidTemplate>, FluidProcessor.TransformationService>();

            // Register your forms
            services.AddTransient<MainForm>();
        }
    }
}
