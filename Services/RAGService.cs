using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using FenecAI.API.Config;
using FenecAI.API.Models;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Implements the Retrieval-Augmented Generation (RAG) pattern using Azure OpenAI and Azure AI Search.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The RAG workflow enhances generative AI models with factual grounding by retrieving
	/// relevant information from indexed company documents stored in Azure Blob Storage and
	/// Azure AI Search before generating a response.
	/// </para>
	/// <para>
	/// This class covers the AI-102 topics:
	/// - Document ingestion and vectorization (embeddings)
	/// - Semantic search with Azure AI Search
	/// - Context-based GPT completions
	/// </para>
	/// </remarks>
	public class RAGService
	{
		private readonly AzureOpenAIOptions _openAIOptions;
		private readonly IConfiguration _config;

		/// <summary>
		/// Initializes a new instance of <see cref="RAGService"/> with configuration and OpenAI options.
		/// </summary>
		/// <param name="openAIOptions">Azure OpenAI configuration section (endpoint, key, deployment).</param>
		/// <param name="config">Global application configuration (storage, search service credentials).</param>
		public RAGService(IOptions<AzureOpenAIOptions> openAIOptions, IConfiguration config)
		{
			_openAIOptions = openAIOptions.Value;
			_config = config;
		}

		/// <summary>
		/// Reads all <c>.txt</c> files from Blob Storage, splits them into smaller chunks,
		/// generates embeddings, and indexes them in Azure AI Search for retrieval.
		/// </summary>
		/// <returns>
		/// The total number of text chunks processed and indexed.
		/// </returns>
		/// <remarks>
		/// <para>
		/// <b>Ingestion Workflow:</b><br/>
		/// 1. Read all text documents from the configured blob container.<br/>
		/// 2. Split content into smaller, semantically meaningful chunks.<br/>
		/// 3. Generate embeddings for each chunk using <b>text-embedding-3-large</b>.<br/>
		/// 4. Upload the chunk content and vector embeddings into the Azure AI Search index.
		/// </para>
		/// </remarks>
		public async Task<int> IngestDocumentsAsync()
		{
			// --- Step 1: Initialize Azure clients ---
			var blobContainer = new BlobContainerClient(
				_config["AzureStorage:ConnectionString"],
				_config["AzureStorage:ContainerName"]);

			var embeddingClient = new AzureOpenAIClient(
				new Uri(_openAIOptions.Endpoint),
				new AzureKeyCredential(_openAIOptions.ApiKey))
				.GetEmbeddingClient("text-embedding-3-large");

			var searchClient = new SearchClient(
				new Uri(_config["AzureAI:SearchEndpoint"]),
				_config["AzureAI:SearchIndexName"],
				new AzureKeyCredential(_config["AzureAI:SearchApiKey"]));

			int totalChunks = 0;

			await foreach (var blob in blobContainer.GetBlobsAsync())
			{
				if (!blob.Name.EndsWith(".txt"))
					continue;

				var blobClient = blobContainer.GetBlobClient(blob.Name);
				var content = await ReadBlobContentAsync(blobClient);

				// --- Step 2: Split text into chunks ---
				var chunks = SplitTextIntoChunks(content, 500);

				// --- Step 3: Generate embeddings for each chunk ---
				var docsToIndex = new List<RAGDocument>();
				var embeddingResponse = await embeddingClient.GenerateEmbeddingsAsync(chunks);
				var embeddings = embeddingResponse.Value;

				// Map embeddings back to text chunks
				for (int i = 0; i < chunks.Count; i++)
				{
					docsToIndex.Add(new RAGDocument
					{
						Id = Guid.NewGuid().ToString(),
						Content = chunks[i],
						ContentVector = embeddings[i].ToFloats().ToArray(),
					});
				}

				// --- Step 4: Upload to Azure AI Search ---
				await searchClient.UploadDocumentsAsync(docsToIndex);
				totalChunks += docsToIndex.Count;
			}

			return totalChunks;
		}

		/// <summary>
		/// Executes a retrieval-augmented question-answering flow:
		/// retrieves semantically relevant documents from Azure AI Search and
		/// uses GPT to generate a grounded answer based on that context.
		/// </summary>
		/// <param name="question">The user’s natural language question.</param>
		/// <param name="topK">The number of top results to retrieve from the vector search (default = 5).</param>
		/// <returns>
		/// A <see cref="RAGQueryResponse"/> containing the grounded answer and the list of retrieved context sources.
		/// </returns>
		/// <remarks>
		/// <para>
		/// <b>Workflow:</b><br/>
		/// 1. Generate an embedding vector for the user’s question.<br/>
		/// 2. Perform a vector search on the indexed document embeddings.<br/>
		/// 3. Aggregate the most relevant text chunks as context.<br/>
		/// 4. Pass the context to GPT to produce a factual and contextualized answer.
		/// </para>
		/// <para>
		/// <b>Interpretation tip:</b> The <c>Sources</c> list contains the document chunks used to build the response.
		/// </para>
		/// </remarks>
		public async Task<RAGQueryResponse> QueryAsync(string question, int topK = 5)
		{
			try
			{
				// --- Step 1: Initialize clients ---
				var openAIClient = new AzureOpenAIClient(
					new Uri(_openAIOptions.Endpoint),
					new AzureKeyCredential(_openAIOptions.ApiKey));

				var embeddingClient = openAIClient.GetEmbeddingClient(_config["AzureOpenAI:EmbeddingDeployment"]);
				var chatClient = openAIClient.GetChatClient(_config["AzureOpenAI:Deployment"]);

				var searchClient = new SearchClient(
					new Uri(_config["AzureAI:SearchEndpoint"]),
					_config["AzureAI:SearchIndexName"],
					new AzureKeyCredential(_config["AzureAI:SearchApiKey"]));

				// --- Step 2: Generate embedding for the question ---
				var enrichedQuery = $"Question: {question}. Answer based on company documentation and internal manuals.";
				var embeddingResponse = await embeddingClient.GenerateEmbeddingsAsync(new List<string> { enrichedQuery });
				var queryVector = embeddingResponse.Value[0].ToFloats().ToArray();

				// --- Step 3: Perform vector search in Azure AI Search ---
				var vectorQuery = new VectorizedQuery(queryVector)
				{
					KNearestNeighborsCount = topK,
					Fields = { "contentVector" }
				};

				var searchOptions = new SearchOptions
				{
					VectorSearch = new() { Queries = { vectorQuery } },
					Size = topK,
					Select = { "content" }
				};

				var searchResults = await searchClient.SearchAsync<RAGDocument>(null, searchOptions);

				// --- Step 4: Gather top relevant chunks ---
				var context = new List<string>();
				await foreach (var result in searchResults.Value.GetResultsAsync())
				{
					if (!string.IsNullOrWhiteSpace(result.Document.Content))
						context.Add(result.Document.Content);
				}

				if (context.Count == 0)
				{
					return new RAGQueryResponse
					{
						Answer = "No relevant information was found in the indexed documents.",
						Sources = new List<string>()
					};
				}

				// --- Step 5: Clean context for better GPT readability ---
				var cleanedContext = context
					.Select(c => c.Replace("\r", "").Replace("\n", Environment.NewLine).Trim())
					.ToList();

				// --- Step 6: Build the system prompt for grounded answering ---
				var systemPrompt = @"You are an assistant for question answering based on company documentation.
                    Use the following context to answer the question as accurately as possible.
                    If the context partially matches, infer the most likely answer.

                    Context:
                    " + string.Join("\n---\n", cleanedContext);

				var messages = new List<ChatMessage>
				{
					new SystemChatMessage(systemPrompt),
					new UserChatMessage(question)
				};

				// --- Step 7: Request grounded completion from GPT ---
				var chatResponse = await chatClient.CompleteChatAsync(messages);
				var answer = chatResponse.Value.Content[0].Text
					.Replace("\r", "")
					.Replace("\n", Environment.NewLine)
					.Trim();

				return new RAGQueryResponse
				{
					Answer = answer,
					Sources = cleanedContext
				};
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[RAG ERROR] {ex.Message}");
				return new RAGQueryResponse
				{
					Answer = "An error occurred while processing the query.",
					Sources = new List<string> { ex.Message }
				};
			}
		}

		/// <summary>
		/// Reads the entire text content from a blob file.
		/// </summary>
		/// <param name="blobClient">The <see cref="BlobClient"/> pointing to the text file.</param>
		/// <returns>The full text content of the blob.</returns>
		private static async Task<string> ReadBlobContentAsync(BlobClient blobClient)
		{
			using var stream = await blobClient.OpenReadAsync();
			using var reader = new StreamReader(stream, Encoding.UTF8);
			return await reader.ReadToEndAsync();
		}

		/// <summary>
		/// Splits a text document into smaller chunks suitable for embedding generation.
		/// </summary>
		/// <param name="text">The full text to split.</param>
		/// <param name="maxChunkSize">Maximum size per chunk in characters (default = 800).</param>
		/// <returns>A list of smaller text segments.</returns>
		/// <remarks>
		/// <para>
		/// Text is first split by paragraphs, then aggregated into chunks that fit the
		/// embedding model’s optimal context window (~500–800 characters).
		/// </para>
		/// </remarks>
		private static List<string> SplitTextIntoChunks(string text, int maxChunkSize = 800)
		{
			var chunks = new List<string>();
			var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

			var currentChunk = new StringBuilder();

			foreach (var paragraph in paragraphs)
			{
				if (currentChunk.Length + paragraph.Length < maxChunkSize)
				{
					currentChunk.AppendLine(paragraph.Trim());
					currentChunk.AppendLine();
				}
				else
				{
					if (currentChunk.Length > 0)
					{
						chunks.Add(currentChunk.ToString().Trim());
						currentChunk.Clear();
					}

					if (paragraph.Length > maxChunkSize)
					{
						for (int i = 0; i < paragraph.Length; i += maxChunkSize)
						{
							int size = Math.Min(maxChunkSize, paragraph.Length - i);
							chunks.Add(paragraph.Substring(i, size).Trim());
						}
					}
					else
					{
						currentChunk.AppendLine(paragraph.Trim());
						currentChunk.AppendLine();
					}
				}
			}

			if (currentChunk.Length > 0)
				chunks.Add(currentChunk.ToString().Trim());

			return chunks;
		}
	}
}
