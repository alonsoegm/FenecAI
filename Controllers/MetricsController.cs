using FenecAI.API.Models;
using FenecAI.API.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Provides endpoints for analyzing prompt efficiency, token usage, latency, and cost
	/// when interacting with Azure OpenAI models.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller measures **performance metrics** of a prompt such as:
	/// - Input and output token counts  
	/// - Total tokens consumed  
	/// - Estimated cost (USD)  
	/// - Response latency (milliseconds)  
	/// </para>
	/// <para>
	/// Useful for evaluating optimization strategies and **cost-efficiency** in Generative AI solutions.
	/// </para>
	/// </remarks>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Generative AI - Prompt Metrics")]
	[Produces("application/json")]
	public class MetricsController : ControllerBase
	{
		private readonly MetricsService _metricsService;

		/// <summary>
		/// Initializes a new instance of the <see cref="MetricsController"/> class.
		/// </summary>
		/// <param name="metricsService">Service responsible for analyzing OpenAI model usage and performance.</param>
		public MetricsController(MetricsService metricsService)
		{
			_metricsService = metricsService;
		}

		/// <summary>
		/// Analyzes a prompt to measure token usage, estimated cost, and model latency.
		/// </summary>
		/// <param name="request">
		/// A <see cref="ChatRequest"/> containing the text prompt to analyze.
		/// </param>
		/// <returns>
		/// A <see cref="MetricsResponse"/> including:
		/// - <b>PromptTokens:</b> tokens used for the input  
		/// - <b>CompletionTokens:</b> tokens used for the output  
		/// - <b>TotalTokens:</b> sum of input and output tokens  
		/// - <b>EstimatedCostUsd:</b> approximate cost per request  
		/// - <b>ResponseTimeMs:</b> total processing latency  
		/// </returns>
		/// <remarks>
		/// <para>
		/// Example:
		/// <code>
		/// {
		///   "prompt": "Explain how Azure Cognitive Search integrates with OpenAI embeddings."
		/// }
		/// </code>
		/// </para>
		/// <para>
		/// This endpoint is ideal for understanding performance trade-offs between creativity
		/// and efficiency when tuning GPT parameters.
		/// </para>
		/// </remarks>
		[HttpPost("tokens")]
		[SwaggerOperation(
			Summary = "Analyze token usage and cost",
			Description = "Returns model usage metrics including token count, latency, and estimated cost for a given prompt."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(MetricsResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> AnalyzePrompt([FromBody] ChatRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Prompt))
			{
				return BadRequest(new
				{
					message = "Prompt cannot be empty.",
					example = "Try sending: 'Summarize the benefits of using Azure AI services in enterprise solutions.'"
				});
			}

			var metrics = await _metricsService.AnalyzePromptAsync(request.Prompt);

			// Add interpretation layer for developers
			var interpretation = metrics switch
			{
				{ TotalTokens: < 100 } => "✅ Lightweight request — very efficient for small queries.",
				{ TotalTokens: < 1000 } => "⚙️ Moderate cost — suitable for production workloads.",
				_ => "🚨 Heavy prompt — optimize or reduce verbosity to save cost."
			};

			return Ok(new
			{
				metrics.Model,
				metrics.PromptTokens,
				metrics.CompletionTokens,
				metrics.TotalTokens,
				metrics.EstimatedCostUsd,
				metrics.ResponseTimeMs,
				interpretation,
				message = metrics.Message
			});
		}
	}
}
