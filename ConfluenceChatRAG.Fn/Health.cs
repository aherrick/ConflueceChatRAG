using ConfluenceChatRAG.Data.Models.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfluenceChatRAG.Fn;

public class Health(ILogger<Health> logger, IConfiguration config)
{
    [Function(nameof(Health))]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var appConfig = config.Get<AppConfig>();
        logger.LogInformation("Health check requested.");

        return new OkObjectResult(
            new
            {
                Status = "Healthy",
                Environment = appConfig.ConfluenceOrg ?? "Unknown",
                ChatDeployment = appConfig.AzureOpenAI?.ChatDeployment ?? "Unknown",
                Timestamp = DateTime.UtcNow,
            }
        );
    }
}