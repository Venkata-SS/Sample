using FileProcessor.Impl;
using FileProcessor.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Repository.Impl;
using TransactionProcessor.Repository.Interfaces;

namespace FileProcessorConsole
{
    internal class ConsoleBuilder
    {

        internal IProcessTransaction GetProcessTransaction(string[] args)
        {
            var serviceProvider = CreateHostBuilder(args).Build().Services;
            var serviceScope = serviceProvider.CreateScope();
            var provider = serviceScope.ServiceProvider;
            return provider.GetRequiredService<IProcessTransaction>();
        }

        private IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                    services
                    .AddTransient<CsvContext, CsvContext>()
                    .AddTransient<IMetadataRepository, CsvMetadataRepository>()
                    .AddTransient<IOrderFileProcessor, AAAOrderFileProcessor>()
                    .AddTransient<IOrderFileProcessor, BBBOrderFileProcessor>()
                    .AddTransient<IOrderFileProcessor, CCCOrderFileProcessor>()                    
                    .AddTransient<IProcessTransaction, ProcessTransaction>()
                    ).ConfigureAppConfiguration(app =>
                    {
                        app.AddJsonFile("appSettings.json", false);
                    })
                    .ConfigureLogging( (_,logging) =>
                    {
                        logging.SetMinimumLevel(LogLevel.Trace);
                        logging.AddLog4Net("log4net.config");
                    });
        }
    }
}
