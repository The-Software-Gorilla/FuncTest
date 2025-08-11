using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestAzFunc;

public class HelloWorld
{
    private readonly ILogger<HelloWorld> _logger;

    private readonly String _helloWorldMessage = @"
        {
            ""message"" : ""Hello, TSG Hello World!"",
            ""version"": ""1.0.0"",
            ""copyright"" : ""Copyright (c) 2025 The Software Gorilla, a division of Intangere, LLC. All rights reserved."",
            ""license"" : ""GPL""
        }";

    public HelloWorld(ILogger<HelloWorld> logger)
    {
        _logger = logger;
    }

    [Function("hello_world")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new ContentResult
        {
            Content = _helloWorldMessage,
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }
}
