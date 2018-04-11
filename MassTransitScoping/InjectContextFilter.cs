using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MassTransitScoping
{
    public class InjectContextFilter<TConsumer> :
        IFilter<ConsumerConsumeContext<TConsumer>>
        where TConsumer : class

    {
        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("cqrslite");
        }

        public async Task Send(ConsumerConsumeContext<TConsumer> context, IPipe<ConsumerConsumeContext<TConsumer>> next)
        {
            try
            {
                var serviceScope = context.GetPayload<IServiceScope>();

                var scoped = serviceScope.ServiceProvider.GetRequiredService<ScopedObject>();

                if (scoped != null)
                {
                    scoped.SetContext(context);
                }

                await next.Send(context).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class InjectContextFilter<TConsumer, TMessage> : 
        IFilter<ConsumerConsumeContext<TConsumer, TMessage>> 
        where TConsumer : class
        where TMessage : class

    {
        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("cqrslite");
        }

        public async Task Send(ConsumerConsumeContext<TConsumer, TMessage> context, IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next)
        {
            try
            {
                var serviceScope = context.GetPayload<IServiceScope>();

                var scoped = serviceScope.ServiceProvider.GetRequiredService<ScopedObject>();

                if (scoped != null)
                {
                    scoped.SetContext(context);
                }

                await next.Send(context).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class InjectContextSpecification<TConsumer, TMessage> :
        IPipeSpecification<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        public void Apply(IPipeBuilder<ConsumerConsumeContext<TConsumer, TMessage>> builder)
        {
            builder.AddFilter(new InjectContextFilter<TConsumer, TMessage>());
        }

        public IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }
    }

    public class InjectContextSpecification<TConsumer> :
        IPipeSpecification<ConsumerConsumeContext<TConsumer>>
        where TConsumer : class
    {
        public void Apply(IPipeBuilder<ConsumerConsumeContext<TConsumer>> builder)
        {
            builder.AddFilter(new InjectContextFilter<TConsumer>());
        }

        public IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }
    }

    public static class ConfiguratorExtensions
    {
        public static void UseContextInjection<TConsumer, TMessage>(this IPipeConfigurator<ConsumerConsumeContext<TConsumer, TMessage>> configurator)
            where TConsumer : class
            where TMessage : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new InjectContextSpecification<TConsumer, TMessage>();

            configurator.AddPipeSpecification(specification);
        }

        public static void UseContextInjection<TConsumer>(this IPipeConfigurator<ConsumerConsumeContext<TConsumer>> configurator)
            where TConsumer : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new InjectContextSpecification<TConsumer>();

            configurator.AddPipeSpecification(specification);
        }
    }
}
