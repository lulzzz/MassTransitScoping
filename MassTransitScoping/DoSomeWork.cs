using MassTransit;
using System;

namespace MassTransitScoping
{
    public interface DoSomeWork : CorrelatedBy<Guid>
    {
        Guid Id { get; }
    }
}
