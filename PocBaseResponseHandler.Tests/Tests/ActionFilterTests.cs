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
public class ActionFilterTests
{
    private readonly ApplicationFactory<Program> factory;

    public ActionFilterTests()
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
    public async Task NotFoundAction_Should_Return_Valid_BaseResponse_With_NotFound_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/NotFoundAction";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PostAsJsonAsync(url, requestBody);

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
    public async Task UnauthorizedAction_Should_Return_Valid_BaseResponse_With_Unauthorized_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/UnauthorizedAction";
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
    public async Task InternalServerErrorAction_Should_Return_Valid_BaseResponse_With_InternalServerError_Code()
    {
        //GIVEN
        var client = factory.CreateClient();
        const string url = "api/WeatherForecast/InternalServerErrorAction";

        //WHEN
        var response = await client.DeleteAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();

        var responseToString = await response.Content.ReadAsStringAsync();
        var baseResponse = JsonSerializer.Deserialize<BaseResponse<object>>(responseToString);
        baseResponse.Should().NotBeNull();
        baseResponse?.Code.Should().Be("internal_server_error");
        baseResponse?.Error.Should().Be("An unhandled exception was thrown by the application");
        baseResponse?.Response.Should().BeNull();
        baseResponse?.ResponseType.Should().BeNull();
    }

    [TestMethod]
    public async Task AuthorizedAttributeAction_Should_Return_Valid_BaseResponse_With_Ok_Code_For_User()
    {
        //GIVEN
        var provider = TestClaimsProvider.WithUserClaims();
        var client = factory.CreateClientWithTestAuth(provider);

        const string url = "api/WeatherForecast/AuthorizedAttributeAction";
        var requestBody = new ExampleRequest("test");

        //WHEN
        var response = await client.PutAsJsonAsync(url, requestBody);

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
    public async Task Redirect_Should_Not_Change_Response()
    {
        //GIVEN
        var provider = TestClaimsProvider.WithUserClaims();
        var client = factory.CreateClientWithTestAuth(provider);

        const string url = "api/WeatherForecast/Redirect";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var responseToString = await response.Content.ReadAsStringAsync();
        responseToString.Should().BeNullOrEmpty();
    }

    [TestMethod]
    public async Task AdministratorOnlyAction_Should_Return_Valid_BaseResponse_With_Ok_Code_For_Admin()
    {
        //GIVEN
        var provider = TestClaimsProvider.WithAdminClaims();
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
        baseResponse?.Code.Should().Be("ok");
        baseResponse?.Error.Should().BeNull();
        baseResponse?.Response.Should().NotBeNull();
        baseResponse?.ResponseType.Should().Be(typeof(WeatherForecast[]).FullName);
    }

    [TestMethod]
    public async Task FileZipResultAction_Should_Not_Change_Response()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "api/WeatherForecast/FileZipResultAction?payload=test";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();
        response.Content.Headers.GetValues("Content-Type").First().Should().StartWith("application/zip");
        response.Content.Headers.GetValues("Content-Disposition").First().Should().StartWith("attachment; filename=20220105000000000.zip; filename*=UTF-8''20220105000000000.zip");
        var responseToString = await response.Content.ReadAsStringAsync();
        responseToString.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod]
    public async Task OctetStreamAction_Should_Not_Change_Response()
    {
        //GIVEN
        var client = factory.CreateClient();

        const string url = "api/WeatherForecast/OctetStreamAction?payload=test";

        //WHEN
        var response = await client.GetAsync(url);

        //THEN
        response.EnsureSuccessStatusCode();
        response.Content.Headers.GetValues("Content-Type").First().Should().StartWith("application/octet-stream");
        response.Content.Headers.GetValues("Content-Disposition").First().Should().StartWith("attachment; filename=20220105000000000.xlsx; filename*=UTF-8''20220105000000000.xlsx");
        var responseToString = await response.Content.ReadAsStringAsync();
        responseToString.Should().Be("test");
    }
}