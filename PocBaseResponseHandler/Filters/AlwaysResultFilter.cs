namespace PocBaseResponseHandler.Filters;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PocBaseResponseHandler.Extensions;

// It runs after IAsyncExceptionFilter
// Runs for all action results. This includes action results produced by:
//      *Authorization filters and resource filters that short-circuit.
//      *Exception filters (will not be triggered after exception if there is no exception filter).
// Is not executed in cases where an authorization filter or resource filter short-circuits the request to prevent execution of the action

public class AlwaysResultFilter : IAsyncAlwaysRunResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        HandleResultsWithStatusCode(context);
        HandleForbidResults(context);
        await next();
    }

    private static void HandleResultsWithStatusCode(ResultExecutingContext resultContext)
    {
        if (resultContext.Result is IStatusCodeActionResult statusCodeActionResult)
        {
            var statusCode = statusCodeActionResult.StatusCode ?? resultContext.HttpContext.Response.StatusCode;
            var baseResponse = resultContext.Result switch
            {
                ObjectResult objectResult => MapObjectResultToBaseResponse(objectResult, statusCode),
                _ => statusCode.MapToBaseResponse()
            };

            resultContext.Result = new ObjectResult(baseResponse)
            {
                ContentTypes = new MediaTypeCollection
                {
                    MediaTypeNames.Application.Json
                },
                StatusCode = statusCode
            };
        }
    }

    private static void HandleForbidResults(ResultExecutingContext resultContext)
    {
        if (resultContext.Result is ForbidResult forbidResult)
        {
            var statusCode = 403;
            var baseResponse = statusCode.MapToBaseResponse();

            resultContext.Result = new ObjectResult(baseResponse)
            {
                ContentTypes = new MediaTypeCollection
                {
                    MediaTypeNames.Application.Json
                },
                StatusCode = statusCode
            };
        }
    }

    private static BaseResponse<object> MapObjectResultToBaseResponse(ObjectResult result, int statusCode)
    {
        if (result is { Value: { } } response)
        {
            var responseType = response.Value.GetType();
            return statusCode.MapToBaseResponse(response.Value, responseType);
        }

        return statusCode.MapToBaseResponse();
    }
}
