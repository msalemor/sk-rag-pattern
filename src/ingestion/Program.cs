using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel.Text;

namespace ingestion;
class Program
{
    private const string Source_Folder = "../../data/";
    private const string Endpoint = "http://localhost:5087/api/gpt/memory";
    private const int Chunk_size = 512;
    private const string Collection_Name = "docs";
    static HttpClient client = new();

    static List<string> ChunkText(string content, int chunk_size)
    {
        var lines = TextChunker.SplitPlainTextLines(content, chunk_size / 2);
        // return paragraphs
        return TextChunker.SplitPlainTextParagraphs(lines, chunk_size);
    }

    static void Main(string[] args)
    {
        var fileParagraphs = ExtractParagraphsFromFilesInFolderAsync(Source_Folder).GetAwaiter().GetResult();
        foreach (var fileParagraph in fileParagraphs)
        {
            int count = 1;
            var paragraphs = ChunkText(fileParagraph.Item2, Chunk_size);
            foreach (var paragraph in paragraphs)
            {
                var payload = new
                {
                    collection = Collection_Name,
                    key = $"{fileParagraph.Item1}-{paragraphs.Count}-{count}",
                    text = paragraph
                };
                var jsonContent = JsonSerializer.Serialize(payload);
                var resp = client.PostAsync(new Uri(Endpoint), new StringContent(jsonContent, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Added {fileParagraph.Item1}-{paragraphs.Count}-{count}");
                }
                else
                {
                    Console.WriteLine($"Failed to add {fileParagraph.Item1}-{paragraphs.Count}-{count}");
                }
                count++;
            }
        }
    }

    static Task<Tuple<string, string>> GetFileContent(string filePath)
    {
        return Task.Run(() =>
        {
            var fileName = Path.GetFileName(filePath);
            var fileContent = File.ReadAllText(filePath);
            return new Tuple<string, string>(fileName, fileContent);
        });
    }

    static async Task<List<Tuple<string, string>>> ExtractParagraphsFromFilesInFolderAsync(string folderPath)
    {
        var files = Directory.GetFiles(folderPath, "*.txt");
        var fileContent = new List<Tuple<string, string>>();

        // Read all files in parallel
        var tasks = new List<Task<Tuple<string, string>>>();
        foreach (var file in files)
        {
            tasks.Add(GetFileContent(file));
        }
        await Task.WhenAll(tasks);

        // Add file name and content to list
        foreach (var task in tasks)
        {
            fileContent.Add(new Tuple<string, string>(task.Result.Item1, task.Result.Item2));
        }

        return fileContent;
    }
}
