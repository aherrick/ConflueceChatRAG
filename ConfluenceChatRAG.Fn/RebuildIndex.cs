//using ConfluenceChatRAG.Data.Models.Config;
//using ConfluenceChatRAG.Data.Services;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//namespace ConfluenceChatRAG.Fn;

//public class RebuildIndexTimer(ILogger<RebuildIndexTimer> logger, IConfiguration config)
//{
//    // Runs at midnight EST (5 AM UTC during standard time, 4 AM UTC during daylight time)
//    [Function("RebuildIndexTimer")]
//    public async Task Run([TimerTrigger("0 0 5 * * *")] TimerInfo timerInfo)
//    {
//        logger.LogInformation("RebuildIndex timer function started at: {Time}", DateTime.UtcNow);

//        var appConfig = config.Get<AppConfig>();

//        var confluenceService = new ConfluenceService(appConfig.ConfluenceOrg);
//        var pages = await confluenceService.GetPages();
//        logger.LogInformation("Fetched {Count} pages", pages.Count);

//        var embeddingService = new EmbeddingService(appConfig);
//        var result = await embeddingService.RebuildIndexAsync(
//            pages,
//            appConfig.AzureOpenAI.EmbeddingDeployment
//        );

//        logger.LogInformation(
//            "RebuildIndex complete. Pages={Pages}, Chunks={Chunks}",
//            result.PageCount,
//            result.ChunkCount
//        );

//        if (timerInfo.ScheduleStatus is not null)
//        {
//            logger.LogInformation("Next timer schedule at: {Next}", timerInfo.ScheduleStatus.Next);
//        }
//    }
//}