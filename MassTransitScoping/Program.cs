using GreenPipes;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using MassTransit.Scoping;
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
                    var scopeProvider = new DependencyInjectionConsumerScopeProvider(container);

                    //e.Consumer(new ScopeConsumerFactory<DoSomeWorkConsumer>(scopeProvider), y =>
                    //{
                    //    //y.ConsumerMessage<DoSomeWork>(m => m.UseContextInjection(container));
                    //    //y.UseContextInjection(container);
                    //    y.Message<DoSomeWork>(m =>
                    //    {
                    //        //m.UseContextMessageInjection(container);
                    //        m.UseFilter(new InjectContextMessageFilter<DoSomeWorkConsumer>(container));
                    //    });
                    //});

                    //e.Consumer(new ScopeConsumerFactory<DoSomeWorkConsumer>(scopeProvider), cfg => cfg.UseFilter(new InjectContextFilter<DoSomeWorkConsumer>(container)));
                    //e.Consumer(container.GetRequiredService<DoSomeWorkConsumer>, cfg => cfg.ConsumerMessage<DoSomeWork>(m => m.UseFilter(new InjectContextFilter<DoSomeWorkConsumer, DoSomeWork>(container))));
                    e.Consumer(new ScopeConsumerFactory<DoSomeWorkConsumer>(scopeProvider), cfg => cfg.ConsumerMessage<DoSomeWork>(m => m.UseFilter(new InjectContextFilter<DoSomeWorkConsumer, DoSomeWork>(container))));

                    //e.Consumer(container.GetRequiredService<DoSomeWorkConsumer>, y =>
                    //{
                    //    y.ConsumerMessage<DoSomeWork>(m => m.UseFilter(new InjectContextFilter<DoSomeWorkConsumer, DoSomeWork>(container)));
                    //});

                    //e.Consumer(() => container.GetRequiredService<DoSomeWorkConsumer>(), cfg => cfg.UseFilter(new InjectContextFilter<DoSomeWorkConsumer>(container)));

                    //e.LoadFrom(container);

                    e.PrefetchCount = 2;
                });

                x.UseSerilog();
            }));

            container = services.BuildServiceProvider();

            var busControl = container.GetRequiredService<IBusControl>();

            busControl.Start();

            Log.Information("Receiver started...");

            for (var i = 1; i <= 2; i++)
            {
                var correlationId = NewId.NextGuid();

                Log.Information($"{i}: CorrelationId: {correlationId}");

                busControl.Publish<DoSomeWork>(new
                {
                    Id = NewId.NextGuid(),
                    CorrelationId = correlationId
                });
            }
        }
    }
}