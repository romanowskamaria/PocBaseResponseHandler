namespace PocBaseResponseHandler.Filters;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PocBaseResponseHandler.Extensions;

// If exception then resultContext.Result is null
// It runs before IAsyncExceptionFilter
// Run immediately before and after an action method is called.
// Can change the arguments passed into an action.
// Can change the result returned from the action.
// Are not supported in Razor Pages.

public class ActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        HandleResultsWithStatusCode(resultContext);
    }

    private static void HandleResultsWithStatusCode(ActionExecutedContext resultContext)
    {
        if (resultContext.Result is IStatusCodeActionResult statusCodeActionResult)
        {
            var statusCode = statusCodeActionResult.StatusCode ?? resultContext.HttpContext.Response.StatusCode;
            var baseResponse = resultContext.Result switch
            {
                ObjectResult objectResult => MapObjectResultToBaseResponse(objectResult, statusCode),
                _ => statusCode.MapToBaseResponse()
            };
            resultContext.HttpContext.Response.Headers.Add(BaseResponseHelpers.RESPONSE_HAS_BEEN_HANDLED, nameof(ActionFilter));
            resultContext.Result = new ObjectResult(baseResponse)
            {
                ContentTypes = new MediaTypeCollection
                {
                    MediaTypeNames.Application.Json
                },
                StatusCode = 200
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
