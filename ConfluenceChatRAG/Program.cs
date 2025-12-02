using ConfluenceChatRAG;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient(
    "api",
    client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["ConfluenceChatApiUrl"] ?? "http://localhost:7082"
        );
    }
);

await builder.Build().RunAsync();