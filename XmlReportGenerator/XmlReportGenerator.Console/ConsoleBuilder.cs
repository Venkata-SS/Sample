using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlReportGenerator.Impl;
using XmlReportGenerator.Interfaces;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Console
{
    internal class ConsoleBuilder
    {
        internal IFileProcessor GetFileProcessor(string[] args)
        {
            var serviceProvider = CreateHostBuilder(args).Build().Services;
            var serviceScope = serviceProvider.CreateScope();
            var provider = serviceScope.ServiceProvider;
            return provider.GetRequiredService<IFileProcessor>();
        }

        private IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                    services                   
                    .AddTransient<IXmlParser, CoalGeneratorParser>()
                    .AddTransient<IXmlParser, WindGeneratorParser>()
                    .AddTransient<IXmlParser, GasGeneratorParser>()
                     .AddTransient<IXmlFile, XmlFile>()
                    .AddTransient<IOutputGenerator, OutputGenerator>()
                    .AddTransient<IFileProcessor, FileProcessor>()
                    ).ConfigureAppConfiguration(app =>
                    {
                        app.AddJsonFile("appSettings.json", false);
                    })
                    .ConfigureLogging((_, logging) =>
                    {
                        logging.SetMinimumLevel(LogLevel.Trace);
                        logging.AddLog4Net("log4net.config");
                    });
        }
    }
}
