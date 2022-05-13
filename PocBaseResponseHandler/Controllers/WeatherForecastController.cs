namespace PocBaseResponseHandler.Controllers;

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocBaseResponseHandler.Exceptions;
using PocBaseResponseHandler.Extensions;
using PocBaseResponseHandler.Models;

[ApiController]
[Route("api/[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    //Endpoints like that (which return specific object) don't fill information about status code
    [HttpGet(Name = "WeatherForecast")]
    public IEnumerable<WeatherForecast> OkWeatherForecastAction()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpDelete(Name = "BadRequest")]
    public async Task<IActionResult> BadRequestAction()
    {
        return await Task.FromResult(BadRequest());
    }

    [HttpPost(Name = "Ok")]
    public async Task<IActionResult> OkAction([FromBody] ExampleRequest request)
    {
        return await Task.FromResult(Ok());
    }

    [HttpGet(Name = "OkWithContent")]
    public IActionResult OkWithContentAction()
    {
        return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
    }

    [HttpPost(Name = "NotFound")]
    public async Task<IActionResult> NotFoundAction([FromBody] ExampleRequest request)
    {
        return await Task.FromResult(NotFound());
    }

    [HttpGet(Name = "Exception")]
    public async Task<IActionResult> ExceptionAction()
    {
        throw new CustomAppException("This is custom exception");
    }

    [HttpGet(Name = "UnknownException")]
    public async Task<IActionResult> UnknownExceptionAction()
    {
        throw new ArgumentException();
    }

    [HttpGet(Name = "Forbid")]
    public async Task<IActionResult> ForbidAction()
    {
        return await Task.FromResult(Forbid());
    }

    [HttpPut(Name = "Unauthorized")]
    public async Task<IActionResult> UnauthorizedAction([FromBody] ExampleRequest request)
    {
        return await Task.FromResult(Unauthorized());
    }

    [HttpDelete(Name = "InternalServerError")]
    public async Task<IActionResult> InternalServerErrorAction()
    {
        return await Task.FromResult(StatusCode(StatusCodes.Status500InternalServerError));
    }

    // Unauthorized exception from attribute is not handled by filers
    // Can be handled by middleware
    // Authorization filters are executed before action and result filters
    [HttpPut(Name = "AuthorizedAttribute")]
    [Authorize]
    public IEnumerable<WeatherForecast> AuthorizedAttributeAction([FromBody] ExampleRequest request)
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    // Unauthorized exception from attribute is not handled by filers
    // Can be handled by middleware
    // Authorization filters are executed before action and result filters
    [HttpPost(Name = "AdministratorOnly")]
    [Authorize(Roles = "Admin")]
    public IEnumerable<WeatherForecast> AdministratorOnlyAction([FromBody] ExampleRequest request)
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = true)]
    public RedirectResult Redirect()
    {
        return Redirect("swagger/index.html");
    }

    [HttpGet(Name = "FileZipResult")]
    public async Task<FileResult> FileZipResultAction(string payload)
    {
        var result = payload.ToZipFile();
        var fileName = $"{new DateTime(2022, 01, 05):yyyyMMddHHmmssfff}.zip";

        return File(result, "application/zip", fileName);
    }

    [HttpGet(Name = "OctetStream")]
    public async Task<FileResult> OctetStreamAction(string payload)
    {
        var result = Encoding.UTF8.GetBytes(payload);
        var fileName = $"{new DateTime(2022, 01, 05):yyyyMMddHHmmssfff}.xlsx";

        return File(result, "application/octet-stream", fileName);
    }
}
