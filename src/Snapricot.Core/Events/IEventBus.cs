namespace Snapricot.Core.Events;

/// <summary>
/// In-process pub/sub. Modules communicate only through events, never by referencing each other (ADR-0005).
/// </summary>
public interface IEventBus
{
    /// <summary>Subscribe a handler. Dispose the returned token to unsubscribe.</summary>
    IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler)
        where TEvent : class;

    /// <summary>
    /// Deliver to every current subscriber, in subscription order. A failing handler is logged
    /// and skipped so one broken module can't break the others.
    /// </summary>
    ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}
