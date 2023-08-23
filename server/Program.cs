using System.Text;
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

// Read settings
var configuring = builder.Configuration;
var deploymentName = configuring["AzureChatCompletionService:DeploymentName"];
var endpoint = configuring["AzureChatCompletionService:Endpoint"];
var apiKey = configuring["AzureChatCompletionService:ApiKey"];
var adaDeploymentName = configuring["AzureTextEmbeddingGenerationService:DeploymentName"];

if (string.IsNullOrEmpty(deploymentName) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(adaDeploymentName))
{
    Console.WriteLine("Missing configuration Azure Chat Completion Service or Azure Text Embedding Generation Service");
    return;
}

// Configure Semantic Kernel
var sqliteStore = await SqliteMemoryStore.ConnectAsync("./vectors.sqlite");
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
app.MapPost("/api/gpt/findnearest", async ([FromBody] Query query) =>
{
    IAsyncEnumerable<MemoryQueryResult> queryResults =
                kernel.Memory.SearchAsync(query.collection, query.query, limit: query.limit, minRelevanceScore: query.minRelevanceScore);

    StringBuilder promptData = new StringBuilder();

    await foreach (MemoryQueryResult r in queryResults)
    {
        promptData.Append(r.Metadata.Text + "\n\n");
    }
    var augmentedText = promptData.ToString();

    const string ragFunctionDefinition = "{{$input}}\n\nText:\n\"\"\"{{$data}}\n\"\"\"";

    var ragFunction = kernel.CreateSemanticFunction(ragFunctionDefinition, maxTokens: query.maxTokens);

    var result = await kernel.RunAsync(ragFunction, new(query.query)
    {
        ["data"] = augmentedText
    });
    var completion = new Completion(query.query, result.ToString(), result.ModelResults.LastOrDefault()?.GetOpenAIChatResult()?.Usage);
    return Results.Ok(completion);
})
.WithName("FindNearest")
.WithOpenApi();


app.MapGet("/api/gpt/memory", async (string collection, string key) =>
{
    var mem = await memorySkill.RetrieveAsync(collection, key, logger: null);
    if (string.IsNullOrEmpty(mem))
    {
        return Results.NotFound();
    }
    var outmem = new Memory(collection, key, mem);
    return Results.Ok(outmem);
})
.WithName("PostMemory")
.WithOpenApi();


app.MapPost("/api/gpt/memory", async ([FromBody] Memory memory) =>
{
    var mem = await memorySkill.RetrieveAsync(memory.collection, memory.key, logger: null);
    if (mem is not null)
    {
        await kernel.Memory.RemoveAsync(memory.collection, memory.key);
    }
    await kernel.Memory.SaveInformationAsync(memory.collection,
        id: memory.key,
        text: memory.text);
    return Results.Ok(memory);
})
.WithName("GetMemory")
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


app.Run();


