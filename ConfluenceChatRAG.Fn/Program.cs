using ConfluenceChatRAG.Data.Db;
using ConfluenceChatRAG.Data.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder
    .Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddDbContextFactory<ChatHistoryDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["SqlConnectionString"], sqlOptions =>
    {
        // Enable connection resiliency with automatic retries for transient failures
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,           // Retry up to 10 times
            maxRetryDelay: TimeSpan.FromSeconds(60),  // Max delay between retries
            errorNumbersToAdd: null);   // Use default transient error numbers
    });
    // Default to NoTracking to keep read operations lightweight. When an update is required,
    // re-query with AsTracking() or call context.ChatMessages.Update(entity) before SaveChanges().
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// Register ChatHistoryService as scoped (uses DbContext factory)
builder.Services.AddScoped<ChatHistoryService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<
        IDbContextFactory<ChatHistoryDbContext>
    >();
    using var context = contextFactory.CreateDbContext();
    context.Database.EnsureCreated();
}

app.Run();