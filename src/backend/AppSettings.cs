using dotenv.net;

public class AppSettings
{
    public string GptDeploymentName { get; private set; }
    public string AdaDeploymentName { get; private set; }
    public string Endpoint { get; private set; }
    public string ApiKey { get; private set; }
    public string DbPath { get; private set; }

    public AppSettings()
    {
        // Read environment variables
        DotEnv.Load();
        GptDeploymentName = Environment.GetEnvironmentVariable("GPT_DEPLOYMENT_NAME") ?? "gpt";
        AdaDeploymentName = Environment.GetEnvironmentVariable("ADA_DEPLOYMENT_NAME") ?? "ada";
        Endpoint = Environment.GetEnvironmentVariable("GPT_ENDPOINT") ?? "";
        ApiKey = Environment.GetEnvironmentVariable("GPT_API_KEY") ?? "";
        DbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "./vectors.sqlite";

        if (string.IsNullOrEmpty(GptDeploymentName) || string.IsNullOrEmpty(Endpoint) || string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(AdaDeploymentName))
        {
            Console.WriteLine("Missing configuration Azure Chat Completion Service or Azure Text Embedding Generation Service");
            Environment.Exit(1);
        }
    }
}