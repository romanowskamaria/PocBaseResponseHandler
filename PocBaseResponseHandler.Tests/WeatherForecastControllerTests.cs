namespace PocBaseResponseHandler.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Factories;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class WeatherForecastControllerTests
{
    private readonly ApplicationFactory<Program> factory;

    public WeatherForecastControllerTests()
    {
        factory = new ApplicationFactory<Program>();
    }

    [TestMethod]
    public async Task OkWeatherForecastAction_Should_Return_Valid_BaseResponse_With_ResponseType()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/OkWeatherForecastAction";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNull();
        baseResponse?.Response.Should().NotBeNull();
        baseResponse?.ResponseType.Should().Be(typeof(WeatherForecast[]).FullName);
    }

    [TestMethod]
    public async Task OkAction_Should_Return_Valid_BaseResponse_With_Ok_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/OkAction";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PostAsJsonAsync(url, requestBody);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNull();
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task OkWithContentAction_Should_Return_Valid_BaseResponse_With_Ok_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/OkWithContentAction";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNull();
        baseResponse?.Response.Should().NotBeNull();
        baseResponse?.ResponseType.Should().Be(typeof(WeatherForecast[]).FullName);
    }

    [TestMethod]
    public async Task BadRequestAction_Should_Return_Valid_BaseResponse_With_BadRequest_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/BadRequestAction";

        //WHEN
        var response = await client.DeleteAsync(url);

        //THEN
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("bad_request");
        baseResponse?.Error.Should().Be("Incorrect URL or content format");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task NotFoundAction_Should_Return_Valid_BaseResponse_With_NotFound_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/NotFoundAction";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PostAsJsonAsync(url, requestBody);

        //THEN
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("not_found");
        baseResponse?.Error.Should().Be("Requested resource has not been found");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task ExceptionAction_Should_Return_Valid_BaseResponse_With_Exception_Details()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/ExceptionAction";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("custom_exception_code");
        baseResponse?.Error.Should().Be("This is custom exception");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task UnknownExceptionAction_Should_Return_Valid_BaseResponse_With_Exception_Details()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/UnknownExceptionAction";

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
    public async Task UnauthorizedAction_Should_Return_Valid_BaseResponse_With_Unauthorized_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/UnauthorizedAction";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PutAsJsonAsync(url, requestBody);

        //THEN
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("unauthorized");
        baseResponse?.Error.Should().Be("Request has not been properly authorized");
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
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("forbidden");
        baseResponse?.Error.Should().Be("Forbidden");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task UnauthorizedAttributeAction_Should_Have_Empty_Response_Body()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/UnauthorizedAttributeAction";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PutAsJsonAsync(url, requestBody);

        //THEN
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var responseToString = await response.Content.ReadAsStringAsync();
        responseToString.Should().BeNullOrEmpty();
    }

    [TestMethod]
    public async Task InternalServerErrorAction_Should_Return_Valid_BaseResponse_With_InternalServerError_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/InternalServerErrorAction";

        //WHEN
        var response = await client.DeleteAsync(url);

        //THEN
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("internal_server_error");
        baseResponse?.Error.Should().Be("An unhandled exception was thrown by the application");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }
}
