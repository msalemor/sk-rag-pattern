# A C# WebAPI minimal API<br/>RAG Multi Collection Implemtation

## Information

### Features

This repo includes a multi-collection RAG pattern implementation using C# minimal API and Semantic Kernel (SK). Through interfaces and configuration, SK supports different databases connectors like Azure Search, PostgreSQL, Duck DB, Redis, volatile memory (a RAM DB), and others. As this implementation is multi-collection, each collection could represent different entities such as customers, business units, or areas. 

There are areas of concern that need to be taken into consideration in RAG patterns such as:

- Ingestion
  -	Managing the sources (text, PDFs, images, etc.).
  -	Extracting the text from the sources.
  -	Maybe keeping track of the source locations (to quote references).
- Text Chunking or smart chunking
  -	Chunking large text sources into smaller pieces.
- Embedding and vector DB storage
  - Embedding the text chunks (basically convert the text to a numerical vector representation)
  - Saving the chunks in a vector DB. In SK, the text and the text embedding is called a memory.
- Working with Token Limits
  -	Token limitations in the LLM and embedding models.
- Processing Prompt and completions
  -	Turning the query into an embedding
  -	Comparing the query embedding against the vector DB embeddings returning the relevance scores and requested limits.
  -	Using the text in the top relevant results to augment the prompt.
  -	Sending the prompt for completion with the original query and the augmented context.



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

#### GET a memory - GET `/api/gpt/memory`

- Get a memory by collection and key.
- Parameters:
  - Collection Name
  - Memory ID

#### Create a memory - POST `/api/gpt/memory`

- Insert a memory by collection, key, and blob.
- Request Payload Model: Memory

#### DELETE a memory - `/api/gpt/memory`

- Delete a memory by collection and key.
- Request Payload Model: Memory

#### Query the database - POST - `/api/gpt/query`

- Find the nearest matches by query, relevance score, return limits, and token size.
- Request payload Model: Query
- Response payload Model: Completion
