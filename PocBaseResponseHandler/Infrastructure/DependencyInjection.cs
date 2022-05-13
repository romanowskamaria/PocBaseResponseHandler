namespace PocBaseResponseHandler.Infrastructure;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using SignalR.Hubs;

internal static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddAppHealthChecks();
        services.AddSignalR();
    }

    public static void MapHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<WeatherForecastHub>(WeatherForecastHub.Pattern);
    }

    private static void AddAppHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddSignalRHub($"http://localhost:5150{WeatherForecastHub.Pattern}", "WeatherForecastHub", HealthStatus.Degraded);
    }
}
