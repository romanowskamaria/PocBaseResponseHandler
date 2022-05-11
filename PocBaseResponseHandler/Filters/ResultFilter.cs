namespace PocBaseResponseHandler.Filters;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

// It will not run when there is an exception
// Run only when the action method executes successfully
// Run immediately before and after the execution of action results
// Result filters are only executed when an action or action filter produces an action result.
// Result filters are not executed when:
//      *An authorization filter or resource filter short-circuits the pipeline.
//      *An exception filter handles an exception by producing an action result. (will also not work if there is no exception filter)

public class ResultFilter : IAsyncResultFilter
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

    private static void HandleForbidResults(ResultExecutingContext resultContext)
    {
        if (resultContext.Result is ForbidResult forbidResult)
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
