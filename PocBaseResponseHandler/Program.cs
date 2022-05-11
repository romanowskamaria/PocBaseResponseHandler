using Microsoft.AspNetCore.Authentication;
using PocBaseResponseHandler.Filters;
using PocBaseResponseHandler.Handlers;
using PocBaseResponseHandler.Middlewares;

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
);

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseMiddleware<RequestResponseMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
