using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MassTransitScoping
{
    public class InjectContextFilter : IFilter<ConsumeContext>
    {
        private IServiceProvider _container;

        public InjectContextFilter(IServiceProvider container)
        {
            _container = container;
        }

        public async Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
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

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateFilterScope("cqrslite");
        }
    }

    public class InjectContextSpecification : IPipeSpecification<ConsumeContext>
    {
        private IServiceProvider _container;

        public InjectContextSpecification(IServiceProvider container)
        {
            _container = container;
        }

        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new InjectContextFilter(_container));
        }

        public IEnumerable<ValidationResult> Validate()
        {
            yield break;
        }
    }

    public static class CqrsLiteMiddlewareConfiguratorExtensions
    {
        public static void UseContextInjection(this IConsumePipeConfigurator configurator, IServiceProvider container)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var specification = new InjectContextSpecification(container);

            configurator.AddPipeSpecification(specification);
        }
    }
}
