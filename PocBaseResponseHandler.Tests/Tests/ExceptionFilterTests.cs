namespace PocBaseResponseHandler.Tests.Tests;

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PocBaseResponseHandler.Tests.Factories;
using PocBaseResponseHandler.ViewModels;

[TestClass]
public class ExceptionFilterTests
{
    private readonly ApplicationFactory<Program> factory;

    public ExceptionFilterTests()
    {
        factory = new ApplicationFactory<Program>();
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
}
