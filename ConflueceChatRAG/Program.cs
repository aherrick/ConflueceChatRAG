using ConflueceChatRAG.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(
    "api",
    client =>
    {
#if DEBUG
        client.BaseAddress = new Uri("http://localhost:7082");
#else
        client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ConfluenceChatApiUrl"));
#endif
        client.Timeout = Timeout.InfiniteTimeSpan;
    }
);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Increase Blazor Server circuit timeout for long-running operations
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();