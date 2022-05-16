namespace PocBaseResponseHandler.Middlewares;

using System.Net.Mime;
using System.Text;
using Microsoft.IO;
using PocBaseResponseHandler.Extensions;
using PocBaseResponseHandler.ViewModels;

public class KestrelResponseHandlerMiddleware
{
    private readonly RequestDelegate next;

    public KestrelResponseHandlerMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var originalResponseBodyStream = httpContext.Response.Body;

        var recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        await using var responseStream = recyclableMemoryStreamManager.GetStream();
        httpContext.Response.Body = responseStream;

        await next(httpContext);

        var shouldResponseBeForwarded = ShouldResponseBeForwarded(httpContext);

        if (shouldResponseBeForwarded)
        {
            responseStream.Position = 0;
            await responseStream.CopyToAsync(originalResponseBodyStream, CancellationToken.None);
            return;
        }

        responseStream.Position = 0;
        using var currentResponseStreamReader = new StreamReader(responseStream);
        var currentResponse = await currentResponseStreamReader.ReadToEndAsync();

        var hasResponseAlreadyBeenHandled = await HasResponseAlreadyBeenHandled(currentResponse, httpContext.Response.Headers, httpContext.Response.ContentType);

        if (hasResponseAlreadyBeenHandled)
        {
            httpContext.Response.Headers.Remove(BaseResponseHelpers.ResponseHasBeenHandled);
            responseStream.Position = 0;
            await responseStream.CopyToAsync(originalResponseBodyStream, CancellationToken.None);
            return;
        }

        await HandleOtherResponseAsync(currentResponse, httpContext, originalResponseBodyStream, responseStream, CancellationToken.None);
    }

    private static bool ShouldResponseBeForwarded(HttpContext httpContext)
    {
        var isRedirect = httpContext.Response.StatusCode == 302;

        if (isRedirect)
        {
            return true;
        }

        var isSwitchingProtocolsRequest = httpContext.Response.StatusCode == 101;
        
        if (isSwitchingProtocolsRequest)
        {
            return true;
        }

        var isMetricsRequest = httpContext.Request.Path.Value?
            .Contains("/metrics", StringComparison.InvariantCultureIgnoreCase) ?? false;

        if (isMetricsRequest)
        {
            return true;
        }

        var isSignalRNegotiateRequest = httpContext.Request.Path.Value?
            .Contains("Hub/negotiate", StringComparison.InvariantCultureIgnoreCase) ?? false;

        if (isSignalRNegotiateRequest)
        {
            return true;
        }

        var isSwaggerPage = httpContext.Request.Path.Value?
            .Contains("swagger", StringComparison.InvariantCultureIgnoreCase) ?? false;

        if (isSwaggerPage)
        {
            return true;
        }

        var isContentTypeFilled = (string?)httpContext.Response.ContentType is { };
        var isResponseContentTypeJson = httpContext.Response.ContentType?
            .Contains("json", StringComparison.InvariantCultureIgnoreCase) ?? false;
        var isResponseContentTypeTextPlain = httpContext.Response.ContentType?
            .Contains(MediaTypeNames.Text.Plain, StringComparison.InvariantCultureIgnoreCase) ?? false;
        var isContentTypeSetAndItsNotJsonNorTextPlain = isContentTypeFilled && !isResponseContentTypeJson && !isResponseContentTypeTextPlain;

        if (isContentTypeSetAndItsNotJsonNorTextPlain)
        {
            return true;
        }

        return false;
    }

    private static Task<bool> HasResponseAlreadyBeenHandled(string? currentResponse, IHeaderDictionary headerDictionary, string? contentType)
    {
        var isResponseContentTypeJson = contentType?
            .Contains("json", StringComparison.InvariantCultureIgnoreCase) ?? false;

        if (!isResponseContentTypeJson)
        {
            return Task.FromResult(false);
        }

        var existResponseHasBeenHandledHeader = headerDictionary.ContainsKey(BaseResponseHelpers.ResponseHasBeenHandled);
        
        if (currentResponse?.TryDeserialize(out BaseResponse<object>? baseResponse) ?? false)
        {
            return Task.FromResult(baseResponse is { } && existResponseHasBeenHandledHeader);
        }

        return Task.FromResult(false);
    }

    private static async Task HandleOtherResponseAsync(string currentResponse, HttpContext httpContext, Stream originalResponseBodyStream, Stream responseStream, CancellationToken cancellationToken = default)
    {
        var baseResponse = httpContext.Response.StatusCode.MapToBaseResponse(currentResponse);
        await WriteBaseResponseToResponseStreamAsync(baseResponse, httpContext, responseStream, cancellationToken);
        httpContext.Response.StatusCode = 200;
        responseStream.Position = 0;
        await responseStream.CopyToAsync(originalResponseBodyStream, cancellationToken);
    }

    private static async Task WriteBaseResponseToResponseStreamAsync(BaseResponse<object>? baseResponse, HttpContext httpContext, Stream responseStream, CancellationToken cancellationToken = default)
    {
        if (baseResponse is null)
        {
            return;
        }

        var newResponseJsonString = baseResponse.ToIndentedIgnoreNullJson();
        var responseBuffer = Encoding.UTF8.GetBytes(newResponseJsonString);

        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.ContentLength = responseBuffer.LongLength;

        responseStream.Position = 0;
        responseStream.SetLength(0);
        await responseStream.WriteAsync(responseBuffer, cancellationToken);
    }
}
