using Microsoft.Extensions.Logging;

namespace Snapricot.Core.Events;

public sealed partial class EventBus(ILogger<EventBus> logger) : IEventBus
{
    private readonly Lock _gate = new();
    private readonly Dictionary<Type, Delegate[]> _handlers = [];

    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_gate)
        {
            // Copy-on-write keeps PublishAsync lock-free.
            _handlers[typeof(TEvent)] = [.. Snapshot(typeof(TEvent)), handler];
        }

        return new Subscription(() => Unsubscribe(typeof(TEvent), handler));
    }

    public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);

        Delegate[] handlers;
        lock (_gate)
        {
            handlers = Snapshot(typeof(TEvent));
        }

        foreach (var handler in handlers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await ((Func<TEvent, CancellationToken, ValueTask>)handler)(@event, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogHandlerFailed(ex, typeof(TEvent).Name);
            }
        }
    }

    private Delegate[] Snapshot(Type eventType) =>
        _handlers.TryGetValue(eventType, out var existing) ? existing : [];

    private void Unsubscribe(Type eventType, Delegate handler)
    {
        lock (_gate)
        {
            var remaining = Snapshot(eventType).Where(h => h != handler).ToArray();
            if (remaining.Length == 0)
            {
                _handlers.Remove(eventType);
            }
            else
            {
                _handlers[eventType] = remaining;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "A handler for {EventName} failed")]
    private partial void LogHandlerFailed(Exception exception, string eventName);

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        private Action? _unsubscribe = unsubscribe;

        public void Dispose() => Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
    }
}
