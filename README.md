# Confluence Chat RAG

[![build](https://github.com/aherrick/ConflueceChatRAG/actions/workflows/build.yml/badge.svg)](https://github.com/aherrick/ConflueceChatRAG/actions/workflows/build.yml)

A Retrieval-Augmented Generation (RAG) chat application powered by Azure OpenAI and Azure AI Search for intelligent, context-aware responses from Confluence documentation.

## Stack

- **.NET 10** - Blazor Server + Azure Functions
- **Azure OpenAI** - GPT-4.1 for chat completion
- **Azure AI Search** - Vector search with embeddings
- **SQL Server** - Persistent chat history with EF Core
- **Bootstrap 5** - UI framework

## Projects

- **ConfluenceChatRAG** - Blazor Server UI
- **ConfluenceChatRAG.Fn** - Azure Functions API (chat, history)
- **ConfluenceChatRAG.Data** - Shared models and services
- **ConfluenceChatRAG.AppHost** - .NET Aspire orchestration

## Quick Start

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

2. **Configure `appsettings.json`** in `ConfluenceChatRAG`:
   ```json
   {
     "LogoBase64": "your-base64-encoded-logo-here"
   }
   ```

3. **Start Azurite** (for timer triggers):
   ```powershell
   .\start-azurite.ps1
   ```

4. **Run**:
   ```bash
   dotnet run --project ConfluenceChatRAG.AppHost
   ```

## Features

- **Persistent Chat History** - SQL database with EF Core
- **Session Management** - GUID-based sessions, reload to restore
- **Markdown Rendering** - Assistant responses rendered with Markdig
- **Source Citations** - Links to Confluence pages used in responses
- **Suggestions** - Follow-up question recommendations
- **Vector Search** - Semantic search via Azure AI Search
