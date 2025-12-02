using ConfluenceChatRAG;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Resolve API base URL
string apiBaseUrl;

#if DEBUG
apiBaseUrl = "http://localhost:7082";
#else
apiBaseUrl =
    Environment.GetEnvironmentVariable("ConfluenceChatApiUrl")
    ?? throw new InvalidOperationException(
        "ConfluenceChatApiUrl environment variable is not set in production."
    );
#endif

builder.Services.AddHttpClient(
    "api",
    client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    }
);

await builder.Build().RunAsync();
