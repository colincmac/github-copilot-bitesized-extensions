using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;


namespace Showcase.Shared.AIExtensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAIToolRegistry(this IServiceCollection services, IEnumerable<AIFunction>? tools = default)
    {
        return services.AddSingleton(new AIToolRegistry(tools ?? []));
    }


}
