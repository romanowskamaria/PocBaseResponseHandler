namespace PocBaseResponseHandler.Infrastructure.SignalR.Hubs;

using Microsoft.AspNetCore.SignalR;

public class WeatherForecastHub : Hub
{
    public static readonly string Pattern = "/weatherForecastHub";
}
