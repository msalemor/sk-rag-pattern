using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Skills.Core;
using server.Models;

namespace backend.Services;

public class SKService
{
    private readonly IKernel kernel;
    private readonly ILogger<SKService> logger;
    private readonly ILoggerFactory loggerFactory;

    public SKService(IKernel kernel, ILogger<SKService> logger, ILoggerFactory loggerFactory)
    {
        this.kernel = kernel;
        this.logger = logger;
        this.loggerFactory = loggerFactory;
    }

    public async Task<Tuple<IList<string>?, Exception?>> GetCollections()
    {
        try
        {
            var collections = await kernel.Memory.GetCollectionsAsync();
            return new Tuple<IList<string>?, Exception?>(collections, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting collections");
            return new Tuple<IList<string>?, Exception?>(null, ex);
        }
    }

    public async Task<Tuple<MemoryQueryResult?, Exception?>> GetMemoryAsync(string collection, string key)
    {
        try
        {
            var result = await kernel.Memory.GetAsync(collection, key);
            return new Tuple<MemoryQueryResult?, Exception?>(result, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving memory");
            return new Tuple<MemoryQueryResult?, Exception?>(null, ex);
        }
    }

    public async Task<Tuple<string?, Exception?>> SaveMemoryAsync(TextMemorySkill memorySkill, Memory memory)
    {
        if (string.IsNullOrEmpty(memory.key) || string.IsNullOrEmpty(memory.collection) || string.IsNullOrEmpty(memory.text))
        {
            return new Tuple<string?, Exception?>(null, new ArgumentException("Missing required fields. Must include text, key, and collection."));
        }
        try
        {
            //var skMemory = await memorySkill.RetrieveAsync(memory.collection, memory.key, loggerFactory: loggerFactory);
            var result = await kernel.Memory.GetAsync(memory.collection, memory.key);
            if (result is not null)
            {
                await kernel.Memory.RemoveAsync(memory.collection, memory.key);
            }
            return new Tuple<string?, Exception?>(await kernel.Memory.SaveInformationAsync(memory.collection, memory.text, memory.key, memory.description, memory.additionalMetadata), null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving memory");
            return new Tuple<string?, Exception?>(null, ex);
        }
    }

    public async Task<Exception?> DeleteMemoryAsync(string collection, string key)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(collection))
        {
            return new ArgumentException("Missing required fields. Must include collection and key.");
        }
        try
        {
            await kernel.Memory.RemoveAsync(collection, key);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting memory");
            return ex;
        }
    }

    public async Task<Tuple<Completion?, Exception?>> QueryAsync(Query query)
    {
        if (string.IsNullOrEmpty(query.query) || string.IsNullOrEmpty(query.collection) || query.maxTokens == 0 || query.limit < 0 || query.minRelevanceScore < 0 || query.temperature < 0)
        {
            return new Tuple<Completion?, Exception?>(null, new ArgumentException("Missing required fields. Must include query and collection, and limit, minimum relevance score, and temperature must be greater than 0."));
        }

        try
        {
            IAsyncEnumerable<MemoryQueryResult> queryResults =
                        kernel.Memory.SearchAsync(query.collection, query.query, limit: query.limit, minRelevanceScore: query.minRelevanceScore);

            StringBuilder promptData = new();

            var citations = new List<Citation>();
            await foreach (MemoryQueryResult r in queryResults)
            {
                promptData.Append(r.Metadata.Text + "\n\n");
                var parts = r.Metadata.Id.Split("-");
                if (!citations.Any(c => c.collection == query.collection && c.fileName == parts[0]))
                {
                    // By convention, this app will use the description field to store the URL
                    citations.Add(new Citation(query.collection, parts[0], r.Metadata.Description));
                }
            }
            if (citations.Count == 0)
                return new Tuple<Completion?, Exception?>(null, new Exception("No citations found"));

            var augmentedText = promptData.ToString();

            const string ragFunctionDefinition = "{{$input}}\n\nText:\n\"\"\"{{$data}}\n\"\"\"Use only the provided text.";
            var ragFunction = kernel.CreateSemanticFunction(ragFunctionDefinition, maxTokens: query.maxTokens);
            var result = await kernel.RunAsync(ragFunction, new(query.query)
            {
                ["data"] = augmentedText
            });

            var completion = new Completion(query.query, result.ToString(), null, citations);
            return new Tuple<Completion?, Exception?>(completion, null);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error querying");
            return new Tuple<Completion?, Exception?>(null, ex);
        }
    }
}
