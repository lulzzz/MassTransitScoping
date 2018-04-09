using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace MassTransitScoping
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceProvider container = null;

            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Information("Starting Receiver...");

            var services = new ServiceCollection();

            services.AddScoped<ScopedObject>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<DoSomeWorkConsumer>();
            });

            services.AddSingleton(context => Bus.Factory.CreateUsingRabbitMq(x =>
            {
                IRabbitMqHost host = x.Host(new Uri("rabbitmq://guest:guest@localhost:5672/test"), h => { });

                x.ReceiveEndpoint(host, $"receiver_queue", e =>
                {
                    e.UseContextInjection(container);

                    e.LoadFrom(container);
                });

                x.UseSerilog();
            }));

            container = services.BuildServiceProvider();

            var busControl = container.GetRequiredService<IBusControl>();

            busControl.Start();

            Log.Information("Receiver started...");

            busControl.Publish<DoSomeWork>(new
            {
                Id = NewId.NextGuid(),
                CorrelationId = NewId.NextGuid()
            });
        }
    }
}