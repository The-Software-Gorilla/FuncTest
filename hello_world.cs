using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FuncTest;

public class hello_world
{
    private readonly ILogger<hello_world> _logger;

    private readonly String _response = @"
    {
        ""message"": ""Hello, TSG Azure Functions!"",
        ""version"": ""1.0.0"",
        ""copyright"": ""(c) 2025 - The Software Gorilla, a division of Intangere, LLC""
    }";

    public hello_world(ILogger<hello_world> logger)
    {
        _logger = logger;
    }

    [Function("hello_world")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new ContentResult
        {
            Content = _response,
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }
}
