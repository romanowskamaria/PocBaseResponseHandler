namespace PocBaseResponseHandler.Tests.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PocBaseResponseHandler.Models;
using PocBaseResponseHandler.Tests.Factories;
using PocBaseResponseHandler.Tests.Factories.Extensions;

[TestClass]
public class KestrelResponseHandlerMiddlewareTests
{
    private readonly ApplicationFactory<Program> factory;

    public KestrelResponseHandlerMiddlewareTests()
    {
        factory = new ApplicationFactory<Program>();
    }

    [TestMethod]
    public async Task UnauthorizedAttributeAction_Should_Have_Empty_Response_Body()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/AuthorizedAttributeAction";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PutAsJsonAsync(url, requestBody);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("unauthorized");
        baseResponse?.Error.Should().Be("Request has not been properly authorized");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task Invalid_Url_Should_Have_Error_Message_And_Empty_Response_Body()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/Invalid";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PutAsJsonAsync(url, requestBody);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("not_found");
        baseResponse?.Error.Should().Be("Requested resource has not been found");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task AdministratorOnlyAction_Should_Have_Error_Message_And_Empty_Response_Body()
    {
        //GIVEN
        var provider = TestClaimsProvider.WithUserClaims();
        var client = factory.CreateClientWithTestAuth(provider);

        const string url = "api/WeatherForecast/AdministratorOnlyAction";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PostAsJsonAsync(url, requestBody);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("forbidden");
        baseResponse?.Error.Should().Be("Forbidden");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task OkWeatherForecastAction_With_Invalid_Http_Call_Should_Return_Error_With_Empty_Response()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/OkWeatherForecastAction";

        //WHEN
        var response = await client.DeleteAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("method_not_allowed");
        baseResponse?.Error.Should().Be("Method Not Allowed");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task ForbidAction_Should_Return_Valid_BaseResponse_With_Forbidden_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/ForbidAction";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("forbidden");
        baseResponse?.Error.Should().Be("Forbidden");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task Metrics_Should_Not_be_Changed()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "/metrics";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();
        response.Content.Headers.GetValues("Content-Type").First().Should().StartWith("text/plain");
        var responseToString = await response.Content.ReadAsStringAsync();
        responseToString.Should().NotBeNullOrWhiteSpace();
        responseToString.Should().StartWith("#");
    }

    [TestMethod]
    public async Task Hub_Should_Not_be_Changed()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "/weatherForecastHub/negotiate";

        //WHEN
        var response = await client.PostAsJsonAsync(url, string.Empty);

        //THEN
        response.EnsureSuccessStatusCode();
        response.Content.Headers.GetValues("Content-Type").First().Should().StartWith("application/json");
        var responseToString = await response.Content.ReadAsStringAsync();
        responseToString.Should().NotBeNullOrWhiteSpace();
        responseToString.Should().StartWith("{\"negotiateVersion");
    }

    [TestMethod]
    public async Task OctetStreamAction_Without_Payload_Should_Map_To_BaseResponse_With_Code_Bad_Request()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "api/WeatherForecast/OctetStreamAction";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("bad_request");
        baseResponse?.Error.Should().Be("Incorrect URL or content format");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task OutsideControllerAction_With_Empty_Code_Without_Header_Should_Return_BaseResponse_With_Code_Ok()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "/outsidecontroller_empty_code_in_baseresponse";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNullOrEmpty();
        baseResponse?.Response.Should().NotBeNull();
        baseResponse?.Response?.ToString().Should().BeEquivalentTo(JsonSerializer.Serialize(new BaseResponse<DateTime>
        {
            Code = string.Empty,
            Error = "This is test endpoint",
            Response = new DateTime(2022, 01, 05),
            ResponseType = typeof(DateTime).FullName
        }));
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task OutsideControllerValidAction_With_Valid_BaseResponse_Without_Header_Should_Return_BaseResponse_With_Code_Ok()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "/outsidecontroller_valid_baseresponse_without_header";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNullOrEmpty();
        baseResponse?.Response.Should().NotBeNull();
        baseResponse?.Response?.ToString().Should().BeEquivalentTo(JsonSerializer.Serialize(new BaseResponse<DateTime>
        {
            Code = "ok",
            Error = string.Empty,
            Response = new DateTime(2022, 01, 05),
            ResponseType = typeof(DateTime).FullName
        }));
        baseResponse?.ResponseType.Should().BeNull();
    }


    [TestMethod]
    public async Task OutsideControllerAction_Different_Model_With_Code_Property_Should_Return_BaseResponse_With_Code_Ok()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "/outsidecontroller_different_model_with_code";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNullOrEmpty();
        baseResponse?.Response.Should().NotBeNull();
        baseResponse?.Response?.ToString().Should().BeEquivalentTo(JsonSerializer.Serialize(new
        {
            Code = "Hello, welcome",
            Test = "This is test endpoint",
            Description = new DateTime(2022, 01, 05),
            Info = typeof(DateTime).FullName
        }));
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task OutsideControllerExceptionAction_Should_Return_BaseResponse_With_Code_500()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "/outsidecontroller_exception";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("unknown_error");
        baseResponse?.Error.Should().Be("An unhandled error has occurred");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task OutsideControllerAction_Valid_Model_With_Header_Should_Return_BaseResponse_With_Code_Ok()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "/outsidecontroller_valid_baseresponse_with_header";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<string>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNullOrEmpty();
        baseResponse?.Response.Should().Be("Testing testing testing");
        baseResponse?.ResponseType.Should().Be(typeof(string).FullName);
    }

    [TestMethod]
    public async Task OutsideControllerAction_Text_Plain_Should_Return_BaseResponse_With_Code_Ok()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "/outsidecontroller_text_plain";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNullOrEmpty();
        baseResponse?.Response.Should().NotBeNull();
        baseResponse?.Response?.ToString().Should().Be("Testing");
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task OutsideControllerAction_Text_ContentType_Empty_Should_Return_BaseResponse_With_Code_Ok()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "/outsidecontroller_text_contenttype_empty";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNullOrEmpty();
        baseResponse?.Response.Should().NotBeNull();
        baseResponse?.Response?.ToString().Should().Be("Testing");
        baseResponse?.ResponseType.Should().BeNull();
    }
}
