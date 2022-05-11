namespace PocBaseResponseHandler.Middlewares;

public class RequestResponseMiddleware
{
    private readonly RequestDelegate next;

    public RequestResponseMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        await next(httpContext);

        var statusCode = httpContext.Response.StatusCode;
    }
}
