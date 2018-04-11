using MassTransit;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MassTransitScoping
{
    public class ScopedObject
    {
        public Guid ScopeId { get; set; } = NewId.NextGuid();

        private ConsumeContext _context;

        public void SetContext(ConsumeContext context)
        {
            _context = context;
        }

        public Task DoSomeStaff()
        {
            if (_context == null)
                throw new NullReferenceException(nameof(_context));

            Log.Information($"ScopedObject ConsumeContext CorrelationId: { _context.CorrelationId }");

            //  publish message to bus using _context here... 

            return Task.CompletedTask;
        }
    }
}
