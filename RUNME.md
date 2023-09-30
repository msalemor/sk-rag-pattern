# Running and deploying this application

TL;DR: This repository contains a C# implementation of a multi-collection RAG pattern using minimal API and Semantic Kernel. It supports various database connectors and allows for text ingestion, chunking, and vector storage. The process involves extracting text, chunking it, and saving it to a vector database. The application uses grounding to enhance query results by comparing vectorized queries to chunks in the database. Relevant chunks are returned and used to augment the prompt, which is then sent to OpenAI GPT for completion. Best practices include reviewing and cleaning up extracted text, iterating over prompts, considering token limits, testing different chunking logic, involving subject matter experts, and applying quality and relevance baselines. The API endpoints support recalling, inserting, querying, and deleting memories, and the server and API payload models are provided.

## Frontend

## Backend

## Ingestion

## **Required** backend environment variables or .env file

## Ingesting Data

You will need two shells:

- Shell 1 - Type: `make run`
- Shell 2 - Type: `make ingest`

## Running locally

- Type: `make run`

## Running locally as a container

- Type: `make docker-run`

## Deploying to Azure as a WebApp container

- Type: `make infra`
- Type: `make deploy`

## III. API Endpoints

The API allows for memories to be:

- Recalled by collection name and ID
- Insert by collection name and ID
- Queried
  - For the passed query, find the nearest results by relevance and count limit
  - Augment the prompt with the embedded text result
  - Process the completion of the query and embedded text results
- Deleted by collection name and ID

### 1.0 GET a memory - GET `/api/gpt/memory`

- Get a memory by collection and key.
- Parameters:
  - Collection Name
  - Memory ID

### 2.0 Create a memory - POST `/api/gpt/memory`

- Insert a memory by collection, key, and blob.
- Request Payload Model: Memory

### 3.0 DELETE a memory - `/api/gpt/memory`

- Delete a memory by collection and key.
- Request Payload Model: Memory

### 4.0 Query the database - POST - `/api/gpt/query`

- Find the nearest matches by query, relevance score, return limits, and token size.
- Request payload Model: Query
- Response payload Model: Completion

## IV. Server and API Payload Models

File: `src/backend/Models.cs`
```c#
record Memory(string collection, string key, string text);
record Query(string collection, string query, int maxTokens = 1000, int limit = 3, double minRelevanceScore = 0.77);
record Completion(string query, string text, object? usage, List<Citation>? learnMore = null);
record Citation(string collection, string doc);
```

<hr/>

**Memory:** A record representing a memory with attributes including the collection it belongs to, a unique key, and associated text.
Query: A record used for making queries, specifying the target collection, the query text, maximum token limits, result count limits, and minimum relevance score.
<hr/>

