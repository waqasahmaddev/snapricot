using Microsoft.Extensions.Logging.Abstractions;
using Snapricot.Core.Events;

namespace Snapricot.Core.Tests.Events;

public sealed class EventBusTests
{
    private sealed record Ping(int Value);

    private sealed record Pong;

    private readonly EventBus _bus = new(NullLogger<EventBus>.Instance);

    [Fact]
    public async Task Publish_DeliversToAllSubscribers_InSubscriptionOrder()
    {
        var received = new List<string>();
        _bus.Subscribe<Ping>((e, _) => { received.Add($"a{e.Value}"); return ValueTask.CompletedTask; });
        _bus.Subscribe<Ping>((e, _) => { received.Add($"b{e.Value}"); return ValueTask.CompletedTask; });

        await _bus.PublishAsync(new Ping(1), TestContext.Current.CancellationToken);

        Assert.Equal(["a1", "b1"], received);
    }

    [Fact]
    public async Task Publish_OnlyDeliversToSubscribersOfThatEventType()
    {
        var pongs = 0;
        _bus.Subscribe<Pong>((_, _) => { pongs++; return ValueTask.CompletedTask; });

        await _bus.PublishAsync(new Ping(1), TestContext.Current.CancellationToken);

        Assert.Equal(0, pongs);
    }

    [Fact]
    public async Task Publish_WithNoSubscribers_DoesNothing()
    {
        await _bus.PublishAsync(new Ping(1), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DisposingSubscription_StopsDelivery()
    {
        var count = 0;
        var subscription = _bus.Subscribe<Ping>((_, _) => { count++; return ValueTask.CompletedTask; });

        subscription.Dispose();
        subscription.Dispose();
        await _bus.PublishAsync(new Ping(1), TestContext.Current.CancellationToken);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task FailingHandler_DoesNotStopOtherHandlers()
    {
        var delivered = false;
        _bus.Subscribe<Ping>((_, _) => throw new InvalidOperationException("broken module"));
        _bus.Subscribe<Ping>((_, _) => { delivered = true; return ValueTask.CompletedTask; });

        await _bus.PublishAsync(new Ping(1), TestContext.Current.CancellationToken);

        Assert.True(delivered);
    }

    [Fact]
    public async Task Publish_WhenCancelled_Throws()
    {
        _bus.Subscribe<Ping>((_, _) => ValueTask.CompletedTask);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _bus.PublishAsync(new Ping(1), cts.Token).AsTask());
    }

    [Fact]
    public async Task HandlerSubscribingDuringPublish_IsNotCalledForThatPublish()
    {
        var lateCalls = 0;
        _bus.Subscribe<Ping>((_, _) =>
        {
            _bus.Subscribe<Ping>((_, _) => { lateCalls++; return ValueTask.CompletedTask; });
            return ValueTask.CompletedTask;
        });

        await _bus.PublishAsync(new Ping(1), TestContext.Current.CancellationToken);

        Assert.Equal(0, lateCalls);
    }
}
