# 🚀 Confluence Chat RAG

[![build](https://github.com/aherrick/ConflueceChatRAG/actions/workflows/build.yml/badge.svg)](https://github.com/aherrick/ConflueceChatRAG/actions/workflows/build.yml)
[![publish-web](https://github.com/aherrick/ConflueceChatRAG/actions/workflows/publish-web.yml/badge.svg)](https://github.com/aherrick/ConflueceChatRAG/actions/workflows/publish-web.yml)
[![publish-fn](https://github.com/aherrick/ConflueceChatRAG/actions/workflows/publish-fn.yml/badge.svg)](https://github.com/aherrick/ConflueceChatRAG/actions/workflows/publish-fn.yml)

A Retrieval-Augmented Generation (RAG) chat application powered by Azure OpenAI and Azure AI Search for intelligent, context-aware responses from Confluence documentation. 🤖📚

## 🛠️ Stack

- **.NET 10** - Blazor WebAssembly + Azure Functions
- **Azure OpenAI** - Bring your own chat deployment
- **Azure AI Search** - Vector search with embeddings
- **SQL Server** - Persistent chat history with EF Core
- **Bootstrap 5** - UI framework

## 📦 Projects

| Project | Description |
|---------|-------------|
| **ConfluenceChatRAG** | Blazor WebAssembly UI (hosted on Azure Static Web Apps) |
| **ConfluenceChatRAG.Fn** | Azure Functions API (chat, history, index rebuild) |
| **ConfluenceChatRAG.Data** | Server-side services (EF Core, AI Search, OpenAI) |
| **ConfluenceChatRAG.Shared** | Shared DTOs and models (used by both WASM and API) |

## ⚡ Quick Start

1. **Configure `local.settings.json`** in `ConfluenceChatRAG.Fn`:
   ```json
   {
     "Values": {
       "SqlConnectionString": "Server=(localdb)\\mssqllocaldb;Database=ChatHistory;Trusted_Connection=True;",
       "IndexName": "confluence-pages",
       "ConfluenceOrg": "your-org-name",
       "AzureOpenAI:Endpoint": "https://your-openai.cognitiveservices.azure.com/",
       "AzureOpenAI:EmbeddingDeployment": "text-embedding-3-large",
       "AzureOpenAI:ChatDeployment": "gpt-4.1",
       "AzureOpenAI:ApiKey": "your-openai-api-key",
       "AzureAISearch:Endpoint": "https://your-search.search.windows.net",
       "AzureAISearch:ApiKey": "your-search-api-key"
     }
   }
   ```

2. **Configure `appsettings.json`** in `ConfluenceChatRAG/wwwroot`:
   ```json
   {
     "LogoUrl": "https://your-domain.com/logo.png"
   }
   ```

3. **Start Azurite** (for local Azure Storage emulation):
   ```powershell
   azurite --silent
   ```

## ▶️ How to Run

Run both the Blazor WebAssembly app and the Azure Functions API:

```bash
# Terminal 1 - Blazor WASM UI
dotnet run --project ConfluenceChatRAG

# Terminal 2 - Azure Functions API
func start --project ConfluenceChatRAG.Fn
```

The Blazor app runs entirely in the browser and communicates with the Azure Functions API. 🖥️🔗🧩

## 🚀 Deployment

- **Web App**: Deployed to Azure Static Web Apps via `publish-web.yml`
- **Functions**: Deployed to Azure Functions via `publish-fn.yml`

Both workflows are triggered manually via `workflow_dispatch`.

## ✨ Features

- 💾 **Persistent Chat History** - SQL database with EF Core
- 🗂️ **Session Management** - GUID-based sessions, reload to restore
- 📝 **Markdown Rendering** - Assistant responses rendered with Markdig
- 🔗 **Source Citations** - Links to Confluence pages used in responses
- 💡 **Suggestions** - Follow-up question recommendations
- 🧠 **Vector Search** - Semantic search via Azure AI Search

---

Made with ❤️ using Blazor, Azure, and OpenAI.
