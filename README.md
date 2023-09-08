# A C# WebAPI minimal API<br/>RAG Multi Collection Implemtation

## Information

### Features

This is a straightforward implementation of a Semantic Kernel RAG pattern for multiple collections, utilizing a C# minimal API. The implementation is based on the Semantic Kernel (SK) framework, where memories are implemented through an interface. This design allows for the interchangeability of memories with various connectors like Azure Search, PostgreSQL, DuckDB, and more. Additionally, each collection can represent different entities such as customers, business units, or areas.

### Models

```c#
record Memory(string collection, string key, string text);
record Query(string collection, string query, int maxTokens = 1000, int limit = 3, double minRelevanceScore = 0.77);
record Completion(string query, string text, object? usage);
```

### Routes

The API allows for memories to be:

- Insert
- Recalled
- Searched
  - For the passed query, find the nearest results by relevance and count limit
  - Augment the prompt with the embedded text result
  - Process the completion of the query and embedded text results
- Deleted

#### GET - /api/memory

- Get a memory by collection and key.

#### POST - /api/memory

- Insert a memory by collection, key, and blob.
- Request Payload: Memory

#### DELETE - /api/memory

- Delete a memory by collection and key.
- Request Payload: Memory

#### POST - /api/findnearest

- Find the nearest matches by query, relevance score, return limits and token size.
- Request payload: Query
- Response payload: Completion
