namespace PocBaseResponseHandler.Middlewares;

using System.Net.Mime;
using System.Text;
using Microsoft.IO;
using PocBaseResponseHandler.Extensions;

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

        if (ShouldResponseBeForwarded(httpContext))
        {
            responseStream.Position = 0;
            await responseStream.CopyToAsync(originalResponseBodyStream);
            return;
        }

        responseStream.Position = 0;
        using var currentResponseStreamReader = new StreamReader(responseStream);
        var currentResponse = await currentResponseStreamReader.ReadToEndAsync();

        if (await HasResponseAlreadyBeenHandled(currentResponse, httpContext.Response.Headers, httpContext.Response.ContentType))
        {
            httpContext.Response.Headers.Remove(BaseResponseHelpers.RESPONSE_HAS_BEEN_HANDLED);
            responseStream.Position = 0;
            await responseStream.CopyToAsync(originalResponseBodyStream);
            return;
        }

        await HandleOtherResponse(currentResponse, httpContext, originalResponseBodyStream, responseStream);
    }

    private static bool ShouldResponseBeForwarded(HttpContext httpContext)
    {
        var isRedirect = httpContext.Response.StatusCode == 302;
        var isSwitchingProtocolsRequest = httpContext.Response.StatusCode == 101;

        var isMetricsRequest = httpContext.Request.Path.Value?
            .Contains("/metrics", StringComparison.InvariantCultureIgnoreCase) ?? false;
        var isSignalRNegotiateRequest = httpContext.Request.Path.Value?
            .Contains("Hub/negotiate", StringComparison.InvariantCultureIgnoreCase) ?? false;
        var isSwaggerPage = httpContext.Request.Path.Value?
            .Contains("swagger", StringComparison.InvariantCultureIgnoreCase) ?? false;

        var isContentTypeFilled = (string?)httpContext.Response.ContentType is { };
        var isResponseContentTypeJson = httpContext.Response.ContentType?
            .Contains("json", StringComparison.InvariantCultureIgnoreCase) ?? false;
        var isResponseContentTypeTextPlain = httpContext.Response.ContentType?
            .Contains(MediaTypeNames.Text.Plain, StringComparison.InvariantCultureIgnoreCase) ?? false;
        var isContentTypeSetAndItsNotJsonNorTextPlain = isContentTypeFilled && !isResponseContentTypeJson && !isResponseContentTypeTextPlain;

        return isRedirect || isSwitchingProtocolsRequest || isMetricsRequest || isSignalRNegotiateRequest || isSwaggerPage || isContentTypeSetAndItsNotJsonNorTextPlain;
    }

    private static async Task<bool> HasResponseAlreadyBeenHandled(string currentResponse, IHeaderDictionary headerDictionary, string contentType)
    {
        var isResponseContentTypeJson = contentType?
            .Contains("json", StringComparison.InvariantCultureIgnoreCase) ?? false;

        if (!isResponseContentTypeJson) return false;

        var existResponseHasBeenHandledHeader = headerDictionary.ContainsKey(BaseResponseHelpers.RESPONSE_HAS_BEEN_HANDLED);
        if (currentResponse.TryDeserialize(out BaseResponse<object>? baseResponse))
            return baseResponse is { } && existResponseHasBeenHandledHeader;

        return false;
    }

    private static async Task HandleOtherResponse(string currentResponse, HttpContext httpContext, Stream originalResponseBodyStream, Stream responseStream)
    {
        var baseResponse = httpContext.Response.StatusCode.MapToBaseResponse(currentResponse);
        await WriteBaseResponseToResponseStream(baseResponse, httpContext, responseStream);
        httpContext.Response.StatusCode = 200;
        responseStream.Position = 0;
        await responseStream.CopyToAsync(originalResponseBodyStream);
    }

    private static async Task WriteBaseResponseToResponseStream(BaseResponse<object>? baseResponse, HttpContext httpContext, Stream responseStream)
    {
        if (baseResponse is null) return;

        var newResponseJsonString = baseResponse.ToIndentedIgnoreNullJson();
        var responseBuffer = Encoding.UTF8.GetBytes(newResponseJsonString);

        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.ContentLength = responseBuffer.LongLength;

        responseStream.Position = 0;
        responseStream.SetLength(0);
        await responseStream.WriteAsync(responseBuffer);
    }
}
