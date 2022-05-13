namespace PocBaseResponseHandler.Filters;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using PocBaseResponseHandler.Exceptions;

// Are not as flexible as error handling middleware.
// Are good for trapping exceptions that occur within actions.
public class ExceptionFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        context.HttpContext.Response.Headers.Add(BaseResponseHelpers.RESPONSE_HAS_BEEN_HANDLED, nameof(ExceptionFilter));
        var exceptionHandler = context.Exception switch
        {
            ApplicationException => HandleApplicationException,
            _ => new Action<ExceptionContext>(HandleUnknownException)
        };

        exceptionHandler.Invoke(context);
    }

    private void HandleUnknownException(ExceptionContext context)
    {
        var response = new BaseResponse<object>
        {
            Code = "unknown_error",
            Error = "An unhandled error has occurred"
        };

        context.Result = new ObjectResult(response)
        {
            ContentTypes = new MediaTypeCollection
            {
                MediaTypeNames.Application.Json
            },
            StatusCode = 500
        };
        context.ExceptionHandled = true;
    }

    private void HandleApplicationException(ExceptionContext context)
    {
        var exception = context.Exception as ApplicationException;

        if (exception is null) throw new UnhandledErrorException();

        var response = new BaseResponse<object>
        {
            Code = exception.Code,
            Error = exception.Message
        };

        context.Result = new ObjectResult(response)
        {
            ContentTypes = new MediaTypeCollection
            {
                MediaTypeNames.Application.Json
            },
            StatusCode = 500
        };
        context.ExceptionHandled = true;
    }
}
