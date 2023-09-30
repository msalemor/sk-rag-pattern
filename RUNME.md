# Running and deploying this application

TL;DR: This repository contains a C# implementation of a multi-collection RAG pattern using minimal API and Semantic Kernel. It supports various database connectors and allows for text ingestion, chunking, and vector storage. The process involves extracting text, chunking it, and saving it to a vector database. The application uses grounding to enhance query results by comparing vectorized queries to chunks in the database. Relevant chunks are returned and used to augment the prompt, which is then sent to OpenAI GPT for completion. Best practices include reviewing and cleaning up extracted text, iterating over prompts, considering token limits, testing different chunking logic, involving subject matter experts, and applying quality and relevance baselines. The API endpoints support recalling, inserting, querying, and deleting memories, and the server and API payload models are provided.

## Frontend

## Backend

## Ingestion

## Required backend environment variables or .env file

## Ingesting Data

Type: `make run`
Type: `make ingest`

## Running locally

Type: `make run`

## Running locally as a container

Type: `make docker-run`

## Deploying to Azure as a WebApp container

Type: `make infra`
Type: `make deploy`
