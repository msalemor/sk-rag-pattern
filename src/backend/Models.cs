namespace server.Models;
record Memory(string collection, string key, string text);
record Query(string collection, string query, int maxTokens = 1000, int limit = 3, double minRelevanceScore = 0.77);
record Completion(string query, string text, object? usage, List<Citation>? learnMore = null);
record Citation(string collection, string doc);