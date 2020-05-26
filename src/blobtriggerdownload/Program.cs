using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Topshelf;

namespace sbconsumersvc
{
    class Program
    {
        private static IConfiguration Configuration { get; set; }
        private static ServiceProvider Provider { get; set; }


        private static void ConfigureServices(IConfiguration config)
        {          
            IServiceCollection services = new ServiceCollection();

            services.AddOptions();
            services.Configure<BlobEventConfig>(config.GetSection("BlobEventConfig"));
            services.Configure<BlobEventConnectionStrings>(config.GetSection("ConnectionStrings"));
            services.AddSingleton<BlobEventService>();


            Provider = services.BuildServiceProvider();
        }

        static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.secret.json", optional: true)
            .AddEnvironmentVariables()
            .Build();


            ConfigureServices(Configuration);

            HostFactory.Run(configure =>
            {
                configure.Service<BlobEventService>(service =>
                {
                    service.ConstructUsing(s => Provider.GetRequiredService<BlobEventService>());

                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });

                configure.StartAutomatically();
                configure.EnableServiceRecovery(r => r.RestartService(0));
                configure.RunAsLocalSystem();

                configure.SetServiceName("blobtriggerdownload");
                configure.SetDisplayName("Blob Trigger Download");
                configure.SetDescription("Download newly created blobs");

            });
        }
    }
}
