namespace PocBaseResponseHandler.Middlewares;

using System.Net;
using System.Net.Mime;
using PocBaseResponseHandler.ViewModels;

public static class ExceptionHandlerMiddleware
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseExceptionHandler(new ExceptionHandlerOptions
        {
            ExceptionHandler = async context =>
            {
                var response = context.Response;
                response.Headers.Add(BaseResponseHelpers.ResponseHasBeenHandled, nameof(ExceptionHandlerMiddleware));
                response.ContentType = MediaTypeNames.Application.Json;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.WriteAsJsonAsync(new BaseResponse<string>
                {
                    Code = "unknown_error",
                    Error = "An unhandled error has occurred",
                });
            }
        });
    }
}
