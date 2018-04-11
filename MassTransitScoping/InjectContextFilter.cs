using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MassTransitScoping
{
    public class InjectContextMessageFilter<TMessage> :
        IFilter<ConsumeContext<TMessage>>
        where TMessage : class

    {
        private IServiceProvider _container;

        public InjectContextMessageFilter(IServiceProvider container)
        {
            _container = container;
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("cqrslite");
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            try
            {
                var scoped = _container.GetRequiredService<ScopedObject>();

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

    public class InjectContextFilter<TConsumer> :
        IFilter<ConsumerConsumeContext<TConsumer>>
        where TConsumer : class

    {
        private IServiceProvider _container;

        public InjectContextFilter(IServiceProvider container)
        {
            _container = container;
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("cqrslite");
        }

        public async Task Send(ConsumerConsumeContext<TConsumer> context, IPipe<ConsumerConsumeContext<TConsumer>> next)
        {
            try
            {
                var scoped = _container.GetRequiredService<ScopedObject>();

                if (scoped != null)
                {
                    Log.Information($"Inject context with CorrelationId: {context.CorrelationId}");
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
        private IServiceProvider _container;

        public InjectContextFilter(IServiceProvider container)
        {
            _container = container;
        }

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("cqrslite");
        }

        public async Task Send(ConsumerConsumeContext<TConsumer, TMessage> context, IPipe<ConsumerConsumeContext<TConsumer, TMessage>> next)
        {
            try
            {
                var scoped = _container.GetRequiredService<ScopedObject>();

                if (scoped != null)
                {
                    Log.Information($"Inject context with CorrelationId: {context.CorrelationId}");
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

    public class InjectContextMessageSpecification<TMessage> :
        IPipeSpecification<ConsumeContext<TMessage>>
        where TMessage : class
    {
        private IServiceProvider _container;

        public InjectContextMessageSpecification(IServiceProvider container)
        {
            _container = container;
        }

        public void Apply(IPipeBuilder<ConsumeContext<TMessage>> builder)
        {
            builder.AddFilter(new InjectContextMessageFilter<TMessage>(_container));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }
    }

    public class InjectContextSpecification<TConsumer, TMessage> :
        IPipeSpecification<ConsumerConsumeContext<TConsumer, TMessage>>
        where TConsumer : class
        where TMessage : class
    {
        private IServiceProvider _container;

        public InjectContextSpecification(IServiceProvider container)
        {
            _container = container;
        }

        public void Apply(IPipeBuilder<ConsumerConsumeContext<TConsumer, TMessage>> builder)
        {
            builder.AddFilter(new InjectContextFilter<TConsumer, TMessage>(_container));
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
        private IServiceProvider _container;

        public InjectContextSpecification(IServiceProvider container)
        {
            _container = container;
        }

        public void Apply(IPipeBuilder<ConsumerConsumeContext<TConsumer>> builder)
        {
            builder.AddFilter(new InjectContextFilter<TConsumer>(_container));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }
    }

    public static class ConfiguratorExtensions
    {
        public static void UseContextMessageInjection<TMessage>(this IPipeConfigurator<ConsumeContext<TMessage>> configurator, IServiceProvider container)
            where TMessage : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new InjectContextMessageSpecification<TMessage>(container);

            configurator.AddPipeSpecification(specification);
        }

        public static void UseContextInjection<TConsumer, TMessage>(this IPipeConfigurator<ConsumerConsumeContext<TConsumer, TMessage>> configurator, IServiceProvider container)
            where TConsumer : class
            where TMessage : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new InjectContextSpecification<TConsumer, TMessage>(container);

            configurator.AddPipeSpecification(specification);
        }

        public static void UseContextInjection<TConsumer>(this IPipeConfigurator<ConsumerConsumeContext<TConsumer>> configurator, IServiceProvider container)
            where TConsumer : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new InjectContextSpecification<TConsumer>(container);

            configurator.AddPipeSpecification(specification);
        }
    }
}
