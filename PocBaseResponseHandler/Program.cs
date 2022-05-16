using System.Net.Mime;
using System.Reflection;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using PocBaseResponseHandler;
using PocBaseResponseHandler.Filters;
using PocBaseResponseHandler.Handlers;
using PocBaseResponseHandler.Infrastructure;
using PocBaseResponseHandler.Middlewares;
using PocBaseResponseHandler.ViewModels;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(
    options =>
    {
        // If all enabled the happy path looks like this:
        // request -> Middleware before next -> IAsyncActionFilter before next -> controller method -> IAsyncActionFilter after next -> IAsyncResultFilter before next -> IAsyncAlwaysRunResultFilter -> IAsyncResultFilter after next -> Middleware after next -> out
        // error path (exception inside controller method):
        // request -> Middleware before next -> IAsyncActionFilter before next -> controller method -> IAsyncActionFilter after next -> IAsyncExceptionFilter -> IAsyncAlwaysRunResultFilter -> Middleware after next -> out

        options.Filters.Add(typeof(ActionFilter));
        // options.Filters.Add(typeof(ResultWrapper));
        // options.Filters.Add(typeof(AlwaysResultWrapper));
        options.Filters.Add(typeof(ExceptionFilter));
    }
).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddInfrastructure();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<KestrelResponseHandlerMiddleware>();
app.ConfigureExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics();
    endpoints.MapHubs();
    endpoints.MapGet("outsidecontroller_empty_code_in_baseresponse", async context =>
    {
        await context.Response.WriteAsJsonAsync(new BaseResponse<DateTime>
        {
            Code = string.Empty,
            Error = "This is test endpoint",
            Response = new DateTime(2022, 01, 05),
            ResponseType = typeof(DateTime).FullName
        });
    });

    endpoints.MapGet("outsidecontroller_valid_baseresponse_without_header", async context =>
    {
        await context.Response.WriteAsJsonAsync(new BaseResponse<DateTime>
        {
            Code = "ok",
            Error = string.Empty,
            Response = new DateTime(2022, 01, 05),
            ResponseType = typeof(DateTime).FullName
        });
    });

    endpoints.MapGet("outsidecontroller_different_model_with_code", async context =>
    {
        await context.Response.WriteAsJsonAsync(new
        {
            Code = "Hello, welcome",
            Test = "This is test endpoint",
            Description = new DateTime(2022, 01, 05),
            Info = typeof(DateTime).FullName
        });
    });

    endpoints.MapGet("outsidecontroller_exception", async _ =>
    {
        throw new ApplicationException();
    });

    endpoints.MapGet("outsidecontroller_valid_baseresponse_with_header", async context =>
    {
        context.Response.Headers.Add(BaseResponseHelpers.ResponseHasBeenHandled, "CustomAction");
        await context.Response.WriteAsJsonAsync(new BaseResponse<string>
        {
            Code = "ok",
            Error = string.Empty,
            Response = "Testing testing testing",
            ResponseType = typeof(string).FullName
        });
    });

    endpoints.MapGet("outsidecontroller_text_plain", async context =>
    {
        context.Response.ContentType = MediaTypeNames.Text.Plain;
        await context.Response.WriteAsync("Testing");
    });

    endpoints.MapGet("outsidecontroller_text_contenttype_empty", async context =>
    {
        await context.Response.WriteAsync("Testing");
    });
});

app.Run();
