namespace PocBaseResponseHandler.Middlewares;

using System.Net;
using System.Net.Mime;

public static class ExceptionHandlerMiddleware
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseExceptionHandler(new ExceptionHandlerOptions
        {
            ExceptionHandler = async context =>
            {
                var response = context.Response;
                response.Headers.Add(BaseResponseHelpers.RESPONSE_HAS_BEEN_HANDLED, nameof(ExceptionHandlerMiddleware));
                response.ContentType = MediaTypeNames.Application.Json;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsJsonAsync(new BaseResponse<string>
                {
                    Code = "unknown_error",
                    Error = "An unhandled error has occurred"
                });
            }
        });
    }
}
