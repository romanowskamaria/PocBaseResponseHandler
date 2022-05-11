namespace PocBaseResponseHandler.Models;

public class ExampleRequest
{
    public ExampleRequest(string id)
    {
        Id = id;
    }

    public string Id { get; }
}
