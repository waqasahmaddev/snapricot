using Microsoft.Extensions.DependencyInjection;
using Snapricot.Core.Events;

namespace Snapricot.Core;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddSnapricotCore(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus>();
        return services;
    }
}
