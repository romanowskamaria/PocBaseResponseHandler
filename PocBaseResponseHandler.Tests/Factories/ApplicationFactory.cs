namespace PocBaseResponseHandler.Tests.Factories;

using Microsoft.AspNetCore.Mvc.Testing;

public class ApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
}
