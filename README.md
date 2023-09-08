# A C# WebAPI minimal API<br/>RAG Multi Collection Implemtation

## Information

### Features

This is a straightforward implementation of a Semantic Kernel RAG pattern for multiple collections, utilizing a C# minimal API. The implementation is based on the Semantic Kernel (SK) framework, where memories are implemented through an interface. This design allows for the interchangeability of memories with various connectors like Azure Search, PostgreSQL, DuckDB, and more. Additionally, each collection can represent different entities such as customers, business units, or areas.

### Payload Models

```c#
record Memory(string collection, string key, string text);
record Query(string collection, string query, int maxTokens = 1000, int limit = 3, double minRelevanceScore = 0.77);
record Completion(string query, string text, object? usage);
```

Memory: A record representing a memory with attributes including the collection it belongs to, a unique key, and associated text.
Query: A record used for making queries, specifying the target collection, the query text, maximum token limits, result count limits, and minimum relevance score.

### Routes

The API allows for memories to be:

- Recalled by collection name and ID
- Insert by collection name and ID
- Queried
  - For the passed query, find the nearest results by relevance and count limit
  - Augment the prompt with the embedded text result
  - Process the completion of the query and embedded text results
- Deleted by collection name and ID

#### GET a memory - GET /api/memory

- Get a memory by collection and key.
- Parameters:
  - Collection Name
  - Memory ID

#### Create a memory - POST /api/memory

- Insert a memory by collection, key, and blob.
- Request Payload Model: Memory

#### DELETE a memory - /api/memory

- Delete a memory by collection and key.
- Request Payload Model: Memory

#### Query the database - POST - /api/query

- Find the nearest matches by query, relevance score, return limits, and token size.
- Request payload Model: Query
- Response payload Model: Completion
