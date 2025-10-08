using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Models;
using FenecAI.API.Services;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Provides endpoints for generating and comparing text embeddings using Azure OpenAI.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Embeddings are high-dimensional vector representations of text used for semantic search,
	/// document similarity, and Retrieval-Augmented Generation (RAG) pipelines.
	/// </para>
	/// <para>
	/// These endpoints help visualize and experiment with vector creation and comparison.
	/// </para>
	/// </remarks>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Generative AI - Embeddings")]
	[Produces("application/json")]
	public class EmbeddingsController : ControllerBase
	{
		private readonly EmbeddingsService _embeddingsService;

		/// <summary>
		/// Initializes the controller with the <see cref="EmbeddingsService"/>.
		/// </summary>
		/// <param name="embeddingsService">Service used to generate and compare embeddings.</param>
		public EmbeddingsController(EmbeddingsService embeddingsService)
		{
			_embeddingsService = embeddingsService;
		}

		/// <summary>
		/// Generates an embedding vector from a given text using Azure OpenAI’s embedding model.
		/// </summary>
		/// <param name="request">
		/// An <see cref="EmbeddingRequest"/> containing the input text to be vectorized.
		/// </param>
		/// <returns>
		/// An object containing:
		/// - <b>dimensions:</b> number of values in the embedding vector  
		/// - <b>embeddingSample:</b> first 10 values of the vector (for visualization)  
		/// - <b>preview:</b> first 100 characters of the original text  
		/// </returns>
		/// <remarks>
		/// <para>
		/// Each embedding is a numerical representation of the semantic meaning of the text.
		/// Similar texts produce vectors with high cosine similarity.
		/// </para>
		/// </remarks>
		[HttpPost("generate")]
		[SwaggerOperation(
			Summary = "Generate embedding vector",
			Description = "Creates a high-dimensional vector representation for the given text using Azure OpenAI embeddings."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Generate([FromBody] EmbeddingRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
			{
				return BadRequest(new
				{
					message = "Text cannot be empty.",
					example = "Try sending: 'AI enables computers to learn from data.'"
				});
			}

			var vector = await _embeddingsService.GenerateEmbeddingAsync(request.Text);

			return Ok(new
			{
				dimensions = vector.Length,
				preview = request.Text[..Math.Min(100, request.Text.Length)],
				embeddingSample = vector.Take(10),
				message = "✅ Embedding generated successfully."
			});
		}

		/// <summary>
		/// Compares two text inputs using cosine similarity between their embeddings.
		/// </summary>
		/// <param name="texts">
		/// A dictionary containing two keys: <c>textA</c> and <c>textB</c>.
		/// </param>
		/// <returns>
		/// A numerical similarity score between 0 and 1, where:
		/// - <b>1</b> = identical meaning  
		/// - <b>0</b> = completely unrelated  
		/// </returns>
		/// <remarks>
		/// <para>
		/// Use this endpoint to understand how semantically related two texts are.
		/// Example:
		/// <code>
		/// {
		///   "textA": "The car is fast.",
		///   "textB": "The automobile moves quickly."
		/// }
		/// </code>
		/// </para>
		/// </remarks>
		[HttpPost("similarity")]
		[SwaggerOperation(
			Summary = "Compare text similarity",
			Description = "Computes cosine similarity between embeddings of two texts to measure semantic closeness."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Compare([FromBody] Dictionary<string, string> texts)
		{
			// Validate input keys
			if (!texts.ContainsKey("textA") || !texts.ContainsKey("textB"))
			{
				return BadRequest(new
				{
					message = "Both 'textA' and 'textB' are required.",
					example = new
					{
						textA = "AI can improve productivity.",
						textB = "Artificial intelligence helps people work faster."
					}
				});
			}

			// Generate embeddings
			var v1 = await _embeddingsService.GenerateEmbeddingAsync(texts["textA"]);
			var v2 = await _embeddingsService.GenerateEmbeddingAsync(texts["textB"]);

			// Compute similarity
			var similarity = EmbeddingsService.CosineSimilarity(v1, v2);

			return Ok(new
			{
				similarity,
				interpretation = similarity switch
				{
					> 0.85 => "Texts are highly similar (almost identical meaning).",
					> 0.6 => "Texts are moderately similar (related topic).",
					> 0.3 => "Texts are somewhat related.",
					_ => "Texts are unrelated."
				}
			});
		}
	}
}
