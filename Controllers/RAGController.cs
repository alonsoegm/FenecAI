using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Services;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Handles all Retrieval-Augmented Generation (RAG) operations — including
	/// document ingestion, embedding generation, and context-based querying.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller demonstrates how to combine **Azure Blob Storage**, **Azure AI Search**,  
	/// and **Azure OpenAI embeddings** to build a RAG pipeline — one of the core objectives  
	/// of the **AI-102: Azure AI Engineer Associate** certification.
	/// </para>
	/// <para>
	/// The process:
	/// <list type="number">
	/// <item><description>📂 Upload documents to Azure Blob Storage.</description></item>
	/// <item><description>🧠 Ingest and index them into Azure AI Search with vector embeddings.</description></item>
	/// <item><description>💬 Query the index — the assistant retrieves the most relevant context and answers intelligently.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Knowledge Mining - RAG")]
	[Produces("application/json")]
	public class RAGController : ControllerBase
	{
		private readonly IRAGService _ragService;

		/// <summary>
		/// Initializes a new instance of the <see cref="RAGController"/> class.
		/// </summary>
		/// <param name="ragService">Service that manages ingestion and semantic querying.</param>
		public RAGController(IRAGService ragService)
		{
			_ragService = ragService;
		}

		/// <summary>
		/// Ingests all text documents from Azure Blob Storage and indexes them in Azure AI Search with embeddings.
		/// </summary>
		/// <returns>
		/// A success message indicating how many text chunks were processed and indexed.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Example:
		/// <code>
		/// POST /api/RAG/ingest
		/// </code>
		/// </para>
		/// <para>
		/// Each document is split into smaller chunks (~500–800 characters),
		/// converted into embeddings using the <b>text-embedding-3-large</b> model,
		/// and uploaded to Azure Cognitive Search for semantic retrieval.
		/// </para>
		/// </remarks>
		[HttpPost("ingest")]
		[SwaggerOperation(
			Summary = "Ingest and index documents",
			Description = "Reads all .txt files from Azure Blob Storage, creates embeddings, and indexes them into Azure AI Search."
		)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> IngestDocuments()
		{
			try
			{
				var chunks = await _ragService.IngestDocumentsAsync();
				return Ok(new
				{
					success = true,
					message = $"✅ Indexed {chunks} text chunks into Azure AI Search."
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "Error occurred during document ingestion.",
					details = ex.Message
				});
			}
		}

		/// <summary>
		/// Queries the ingested and indexed documents using a natural language question.
		/// </summary>
		/// <param name="request">
		/// A <see cref="RAGQueryRequest"/> containing:
		/// - <b>Question:</b> user query  
		/// - <b>TopK:</b> number of most relevant chunks to retrieve (default: 5)
		/// </param>
		/// <returns>
		/// A <see cref="RAGQueryResponse"/> containing:
		/// - <b>Answer:</b> model-generated response grounded in context  
		/// - <b>Sources:</b> contextual excerpts retrieved from Azure AI Search  
		/// </returns>
		/// <remarks>
		/// <para>
		/// Example:
		/// <code>
		/// {
		///   "question": "What are the core values of FenecAI?",
		///   "topK": 3
		/// }
		/// </code>
		/// </para>
		/// <para>
		/// The model uses embeddings to perform **semantic vector search** and constructs
		/// an answer grounded in retrieved documents.
		/// </para>
		/// </remarks>
		[HttpPost("query")]
		[SwaggerOperation(
			Summary = "Query indexed documents (RAG)",
			Description = "Performs a semantic search using vector embeddings and returns a grounded GPT answer with supporting sources."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(RAGQueryResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> QueryDocuments([FromBody] RAGQueryRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Question))
				return BadRequest(new { message = "Question cannot be empty." });

			try
			{
				var result = await _ragService.QueryAsync(request.Question, request.TopK);

				return Ok(new
				{
					success = true,
					result.Answer,
					result.Sources,
					message = "✅ Query processed successfully with context grounding."
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "Error occurred while processing the query.",
					details = ex.Message
				});
			}
		}
	}
}
