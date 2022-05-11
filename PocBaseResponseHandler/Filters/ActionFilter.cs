namespace PocBaseResponseHandler.Filters;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
        // ForbidResult doesn't implement IStatusCodeActionResult
        HandleForbidResults(resultContext);
    }

    private static void HandleResultsWithStatusCode(ActionExecutedContext resultContext)
    {
        if (resultContext.Result is IStatusCodeActionResult statusCodeActionResult)
        {
            var statusCode = statusCodeActionResult.StatusCode ?? resultContext.HttpContext.Response.StatusCode;
            var baseResponse = resultContext.Result switch
            {
                ObjectResult objectResult => MapObjectResultToBaseResponse(objectResult, statusCode),
                _ => CreateNewResponse(statusCode)
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

    private static void HandleForbidResults(ActionExecutedContext resultContext)
    {
        if (resultContext.Result is ForbidResult)
        {
            var statusCode = 403;
            var baseResponse = CreateNewResponse(statusCode);

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

    private static BaseResponse<object> MapObjectResultToBaseResponse(ObjectResult result, int? statusCode)
    {
        if (result is { Value: { } } response)
        {
            var responseType = response.Value.GetType();
            return CreateNewResponse(statusCode, response.Value, responseType);
        }

        return CreateNewResponse(statusCode);
    }

    private static BaseResponse<object> CreateNewResponse(int? statusCode, object? response = null, Type? responseType = null)
    {
        return statusCode switch
        {
            200 => new BaseResponse<object>
            {
                Code = "ok",
                Response = response,
                ResponseType = responseType?.FullName
            },
            204 => new BaseResponse<object>
            {
                Code = "ok",
                Response = response,
                ResponseType = responseType?.FullName
            },
            400 => new BaseResponse<object>
            {
                Code = "bad_request",
                Error = "Incorrect URL or content format"
            },
            401 => new BaseResponse<object>
            {
                Code = "unauthorized",
                Error = "Request has not been properly authorized"
            },
            403 => new BaseResponse<object>
            {
                Code = "forbidden",
                Error = "Forbidden"
            },
            404 => new BaseResponse<object>
            {
                Code = "not_found",
                Error = "Requested resource has not been found"
            },
            500 => new BaseResponse<object>
            {
                Code = "internal_server_error",
                Error = "An unhandled exception was thrown by the application"
            },
            _ => new BaseResponse<object>
            {
                Code = statusCode?.ToString() ?? "500"
            }
        };
    }
}
