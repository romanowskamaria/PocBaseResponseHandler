namespace PocBaseResponseHandler.ViewModels;

using System.Text.Json.Serialization;

public class BaseResponse<T>
{
    [JsonPropertyName("code")] public string? Code { get; init; } = default;
    [JsonPropertyName("error")] public string? Error { get; init; } = default;
    [JsonPropertyName("response")] public T? Response { get; set; } = default;
    [JsonPropertyName("type")] public string? ResponseType { get; set; } = default;
}