# A C# WebAPI minimal API<br/>RAG Multi Collection Implemtation

## Information

### Features

This is a straightforward implementation of a Semantic Kernel RAG pattern for multiple collections, utilizing a C# minimal API. The implementation is based on the Semantic Kernel (SK) framework, where memories are implemented through an interface. This design allows for the interchangeability of memories with various connectors like Azure Search, PostgreSQL, DuckDB, and more. Additionally, each collection can represent different entities such as customers, business units, or areas.

The API allows for memories to be:

- Inserted (ingestion),
- Recalled (recalled by ID)
- Searched (found nearest by relevance and limits)
- Deleted (management)

### Routes

#### GET - /api/memory

- Get a memory by collection and key.

#### POST - /api/memory

- Insert a memory by collection, key, and blob.

#### DELETE - /api/memory

- Delete a memory by collection and key.

#### POST - /api/findnearest

- Find the nearest matches by query, relevance score, return limits and token size.
