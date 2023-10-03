using dotenv.net;

namespace backend.Services;

public class AppSettings
{
    public string GptDeploymentName { get; private set; }
    public string AdaDeploymentName { get; private set; }
    public string Endpoint { get; private set; }
    public string ApiKey { get; private set; }
    public string DbPath { get; private set; }
    public string? AzureCognitiveSearchEndpoint {get; private set;}
    public string? AzureCognitiveSearchApiKey {get; private set;}
    public MemorySource MemoryProvider { get; private set; }



    public AppSettings()
    {
        // Read environment variables
        DotEnv.Load();
        GptDeploymentName = Environment.GetEnvironmentVariable("GPT_DEPLOYMENT_NAME") ?? "gpt";
        AdaDeploymentName = Environment.GetEnvironmentVariable("ADA_DEPLOYMENT_NAME") ?? "ada";
        Endpoint = Environment.GetEnvironmentVariable("GPT_ENDPOINT") ?? "";
        ApiKey = Environment.GetEnvironmentVariable("GPT_API_KEY") ?? "";
        DbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "./vectors.sqlite";
        string memoryProvider = Environment.GetEnvironmentVariable("MEMORY_PROVIDER") ?? "";

        if (string.IsNullOrEmpty(GptDeploymentName) || string.IsNullOrEmpty(Endpoint) || string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(AdaDeploymentName) ||  string.IsNullOrEmpty(memoryProvider))
        {
            Console.WriteLine("Missing configuration. Please set GPT_DEPLOYMENT_NAME, ADA_DEPLOYMENT_NAME, GPT_ENDPOINT, GPT_API_KEY, DB_PATH and MEMORY_PROVIDER environment variables.");
            Environment.Exit(1);
        }

        if (Enum.TryParse<MemorySource>(memoryProvider, out var parsedProvider))
        {
            this.MemoryProvider = parsedProvider;

            if (MemoryProvider == MemorySource.AZURECOGNITIVESEARCH)
            {
                AzureCognitiveSearchEndpoint = Environment.GetEnvironmentVariable("AZURE_COGNITIVE_SEARCH_ENDPOINT") ?? "";
                AzureCognitiveSearchApiKey = Environment.GetEnvironmentVariable("AZURE_COGNITIVE_SEARCH_API_KEY") ?? "";

                if (string.IsNullOrEmpty(AzureCognitiveSearchEndpoint) || string.IsNullOrEmpty(AzureCognitiveSearchApiKey))
                {
                    Console.WriteLine("Missing configuration. Please set AZURE_COGNITIVE_SEARCH_ENDPOINT and AZURE_COGNITIVE_SEARCH_API_KEY environment variables.");
                    Environment.Exit(1);
                }
            }
        }
        else
        {
            Console.WriteLine("Environment variable MEMORY_PROVIDER must be one of the following values: ");
            foreach (var possibleEnumValue in Enum.GetValues(typeof(MemorySource)))
            {
                Console.WriteLine(possibleEnumValue);
            }
            Environment.Exit(1);
        }
    }

    public enum MemorySource
    {
        SQLITE,
        AZURECOGNITIVESEARCH
    }
}