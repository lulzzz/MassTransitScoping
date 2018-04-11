using MassTransit;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MassTransitScoping
{
    public class DoSomeWorkConsumer : IConsumer<DoSomeWork>
    {
        private ScopedObject _scoped;

        public DoSomeWorkConsumer(ScopedObject scoped)
        {
            Log.Information($"Initialize DoSomeWorkConsumer consumer");

            _scoped = scoped ?? throw new ArgumentNullException(nameof(scoped));
        }

        public async Task Consume(ConsumeContext<DoSomeWork> context)
        {
            await _scoped.DoSomeStaff();
        }
    }
}
