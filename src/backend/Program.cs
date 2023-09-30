using System.Text;
using dotenv.net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.Sqlite;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Skills.Core;
using server.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read environment variables
DotEnv.Load();
var deploymentName = Environment.GetEnvironmentVariable("GPT_DEPLOYMENT_NAME") ?? "gpt";
var adaDeploymentName = Environment.GetEnvironmentVariable("ADA_DEPLOYMENT_NAME") ?? "ada";
var endpoint = Environment.GetEnvironmentVariable("GPT_ENDPOINT") ?? "";
var apiKey = Environment.GetEnvironmentVariable("GPT_API_KEY") ?? "";
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "./vectors.sqlite";

if (string.IsNullOrEmpty(deploymentName) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(adaDeploymentName))
{
    Console.WriteLine("Missing configuration Azure Chat Completion Service or Azure Text Embedding Generation Service");
    Environment.Exit(1);
}

// Configure Semantic Kernel
var sqliteStore = await SqliteMemoryStore.ConnectAsync(dbPath);
IKernel kernel = new KernelBuilder()
    .WithAzureChatCompletionService(deploymentName, endpoint, apiKey)
    .WithAzureTextEmbeddingGenerationService(adaDeploymentName, endpoint, apiKey)
    .WithMemoryStorage(sqliteStore)
    .Build();

var memorySkill = new TextMemorySkill(kernel.Memory);
kernel.ImportSkill(memorySkill);

// Build the WebApplication
var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Routes
app.MapGet("/api/gpt/memory", async (string collection, string key) =>
{
    var skMemory = await memorySkill.RetrieveAsync(collection, key, loggerFactory: null);
    if (string.IsNullOrEmpty(skMemory))
    {
        return Results.NotFound();
    }
    return Results.Ok(new Memory(collection, key, skMemory));
})
.WithName("GetMemory")
.WithOpenApi();

// Note: It is up to the calling application to implement the text extraction and chunking logic
app.MapPost("/api/gpt/memory", async ([FromBody] Memory memory) =>
{
    var skMemory = await memorySkill.RetrieveAsync(memory.collection, memory.key, loggerFactory: null);
    if (skMemory is not null)
    {
        await kernel.Memory.RemoveAsync(memory.collection, memory.key);
    }
    await kernel.Memory.SaveInformationAsync(memory.collection,
        id: memory.key,
        text: memory.text);
    return Results.Ok(memory);
})
.WithName("PostMemory")
.WithOpenApi();

app.MapDelete("/api/gpt/memory", async ([FromBody] Memory memory) =>
{
    try
    {
        await kernel.Memory.RemoveAsync(memory.collection, memory.key);
    }
    catch
    {
        return Results.NotFound();
    }
    return Results.Ok(memory);
})
.WithName("DeleteMemory")
.WithOpenApi();

app.MapPost("/api/gpt/query", async ([FromBody] Query query) =>
{
    IAsyncEnumerable<MemoryQueryResult> queryResults =
                kernel.Memory.SearchAsync(query.collection, query.query, limit: query.limit, minRelevanceScore: query.minRelevanceScore);

    StringBuilder promptData = new StringBuilder();

    var citations = new List<Citation>();
    await foreach (MemoryQueryResult r in queryResults)
    {
        promptData.Append(r.Metadata.Text + "\n\n");
        var parts = r.Metadata.Id.Split("-");
        if (!citations.Any(c => c.collection == query.collection && c.fileName == parts[0]))
        {
            citations.Add(new Citation(query.collection, parts[0]));
        }
    }
    if (citations.Count == 0)
        return Results.BadRequest();

    var augmentedText = promptData.ToString();

    const string ragFunctionDefinition = "{{$input}}\n\nText:\n\"\"\"{{$data}}\n\"\"\"Use only the provided text.";
    var ragFunction = kernel.CreateSemanticFunction(ragFunctionDefinition, maxTokens: query.maxTokens);
    var result = await kernel.RunAsync(ragFunction, new(query.query)
    {
        ["data"] = augmentedText
    });

    var completion = new Completion(query.query, result.ToString(), result.ModelResults.LastOrDefault()?.GetOpenAIChatResult()?.Usage, citations);
    return Results.Ok(completion);
})
.WithName("PostQuery")
.WithOpenApi();

// Serve static files from wwwroot folder
app.UseStaticFiles();

// automatically serve the index.html file
app.MapFallbackToFile("index.html");

app.Run();
