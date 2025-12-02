using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using ConfluenceChatRAG.Data.Models.Config;
using ConfluenceChatRAG.Data.Models.Dto;
using ConfluenceChatRAG.Data.Models.Vector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;

namespace ConfluenceChatRAG.Data.Services;

public class EmbeddingService
{
    public readonly IEmbeddingGenerator<string, Embedding<float>> Generator;
    public readonly VectorStoreCollection<string, ConfluencePageVector> Collection;

    public EmbeddingService(AppConfig config)
    {
        Generator = new AzureOpenAIClient(
            new Uri(config.AzureOpenAI.Endpoint),
            new AzureKeyCredential(config.AzureOpenAI.ApiKey)
        )
            .GetEmbeddingClient(config.AzureOpenAI.EmbeddingDeployment)
            .AsIEmbeddingGenerator();

        var searchIndexClient = new SearchIndexClient(
            new Uri(config.AzureAISearch.Endpoint),
            new AzureKeyCredential(config.AzureAISearch.ApiKey)
        );
        var vectorStore = new AzureAISearchVectorStore(searchIndexClient);

        Collection = vectorStore.GetCollection<string, ConfluencePageVector>(
            config.IndexName,
            BuildVectorDef(config.AzureOpenAI.EmbeddingDeployment)
        );
    }

    public async Task<RebuildIndexResponseDto> RebuildIndexAsync(
        List<ConfluencePageDto> pages,
        string embeddingModel
    )
    {
        await Collection.EnsureCollectionDeletedAsync();

        // Wait until actually gone
        while (await Collection.CollectionExistsAsync())
        {
            await Task.Delay(500);
        }

        await Collection.EnsureCollectionExistsAsync();

        int pageCount = 0;
        int chunkCount = 0;

        foreach (var page in pages)
        {
            if (string.IsNullOrWhiteSpace(page.Body))
            {
                continue;
            }

            pageCount++;

            var chunks = EmbeddingChunker.ChunkWithTitle(page.Title, page.Body, embeddingModel);
            chunkCount += chunks.Count;

            foreach (var chunk in chunks)
            {
                var embedding = await Generator.GenerateAsync(chunk.content);

                var record = new ConfluencePageVector
                {
                    Key = Guid.CreateVersion7().ToString(),
                    Name = page.Title,
                    Description = chunk.content,
                    Url = page.Url,
                    Vector = embedding.Vector,
                };

                await Collection.UpsertAsync(record);
            }
        }

        return new RebuildIndexResponseDto { PageCount = pageCount, ChunkCount = chunkCount };
    }

    private static VectorStoreCollectionDefinition BuildVectorDef(string embeddingModel)
    {
        int dims = embeddingModel.ToLowerInvariant() switch
        {
            "text-embedding-3-large" => 3072,
            _ => 1536,
        };

        return new VectorStoreCollectionDefinition
        {
            Properties =
            {
                new VectorStoreKeyProperty(nameof(ConfluencePageVector.Key), typeof(string)),
                // Name: full-text searchable (and filterable if you want)
                new VectorStoreDataProperty(nameof(ConfluencePageVector.Name), typeof(string)),
                //{
                //    IsFullTextIndexed = true,
                //    IsIndexed = true,
                //},
                // Description: full-text searchable
                new VectorStoreDataProperty(
                    nameof(ConfluencePageVector.Description),
                    typeof(string)
                ),
                //{
                //    IsFullTextIndexed = true,
                //    IsIndexed = true,
                //},
                new VectorStoreDataProperty(nameof(ConfluencePageVector.Url), typeof(string)),
                new VectorStoreVectorProperty(
                    nameof(ConfluencePageVector.Vector),
                    typeof(ReadOnlyMemory<float>),
                    dimensions: dims
                ),
            },
        };
    }
}