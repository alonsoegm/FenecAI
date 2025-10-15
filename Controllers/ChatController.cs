using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Models;
using FenecAI.API.Services;
using System.Text;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Provides endpoints for interacting with Azure OpenAI's chat models.
	/// Handles standard completions and streaming responses.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Generative AI - Chat")]
	[Produces("application/json")]
	public class ChatController : ControllerBase
	{
		private readonly IChatService _chatService;
		private readonly IContextSafetyService _contextSafetyService;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChatController"/> class.
		/// </summary>
		/// <param name="chatService">Service responsible for communicating with Azure OpenAI.</param>
		/// <param name="contextSafetyService">Service for analyzing prompts to ensure Responsible AI compliance.</param>
		public ChatController(IChatService chatService, IContextSafetyService contextSafetyService)
		{
			_chatService = chatService;
			_contextSafetyService = contextSafetyService;
		}

		/// <summary>
		/// Sends a user prompt to Azure OpenAI and returns the generated completion.
		/// </summary>
		/// <param name="request">
		/// A <see cref="ChatRequest"/> containing:
		/// - <b>Prompt:</b> the user question or instruction  
		/// - <b>Temperature, TopP, MaxTokens:</b> parameters controlling creativity and response length
		/// </param>
		/// <returns>
		/// A <see cref="ChatResponse"/> object containing:
		/// - The model-generated text output  
		/// - Token usage metrics  
		/// - The model name or deployment used  
		/// </returns>
		/// <remarks>
		/// <para>
		/// Before processing, the prompt is analyzed for harmful content (hate, violence, etc.)
		/// using <see cref="ContextSafetyService"/> to ensure Responsible AI compliance.
		/// </para>
		/// </remarks>
		[HttpPost("complete")]
		[SwaggerOperation(
			Summary = "Generate GPT text completion",
			Description = "Processes a user prompt using Azure OpenAI and returns the generated text along with token usage metrics."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CompleteChat([FromBody] ChatRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Prompt))
			{
				return BadRequest(new
				{
					error = "Prompt cannot be empty.",
					example = "Try sending: 'Explain Azure Functions in simple terms.'"
				});
			}

			// Step 1: Safety analysis (Responsible AI)
			var analysis = await _contextSafetyService.AnalyzeTextAsync(request.Prompt);
			if (analysis.Blocked || analysis.HateSeverity > 2 || analysis.ViolenceSeverity > 2)
			{
				return BadRequest(new
				{
					message = "Unsafe prompt detected. Please rephrase.",
					categories = analysis.Raw
				});
			}

			try
			{
				// Step 2: Generate response
				var response = await _chatService.GenerateChatCompletionAsync(request);

				// Step 3: Return structured success response
				return Ok(new
				{
					success = true,
					data = response
				});
			}
			catch (Exception ex)
			{
				// Step 4: Handle unexpected errors gracefully
				return StatusCode(500, new
				{
					success = false,
					message = "An error occurred while processing the chat request.",
					details = ex.Message
				});
			}
		}

		/// <summary>
		/// Streams the chat response token by token as the model generates it.
		/// Ideal for real-time chat interfaces or low-latency applications.
		/// </summary>
		/// <param name="request">A <see cref="ChatRequest"/> containing the user prompt and generation settings.</param>
		/// <remarks>
		/// <para>
		/// The endpoint uses <b>Server-Sent Events (SSE)</b> to continuously push partial responses.
		/// Clients should read the stream incrementally and stop reading when receiving <c>[DONE]</c>.
		/// </para>
		/// </remarks>
		[HttpPost("stream")]
		[SwaggerOperation(
			Summary = "Stream GPT completion tokens",
			Description = "Streams the GPT response in real-time using Server-Sent Events (SSE)."
		)]
		[Consumes("application/json")]
		[Produces("text/event-stream")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task StreamChat([FromBody] ChatRequest request)
		{
			Response.ContentType = "text/event-stream";
			Response.Headers.Append("Cache-Control", "no-cache");
			Response.Headers.Append("Connection", "keep-alive");

			var responseStream = Response.BodyWriter.AsStream();

			await foreach (var token in _chatService.StreamChatAsync(request))
			{
				var data = $"data: {token}\n\n";
				var bytes = Encoding.UTF8.GetBytes(data);
				await responseStream.WriteAsync(bytes);
				await responseStream.FlushAsync();
			}

			// Signal end of stream
			var endSignal = Encoding.UTF8.GetBytes("data: [DONE]\n\n");
			await responseStream.WriteAsync(endSignal);
			await responseStream.FlushAsync();
		}
	}
}
