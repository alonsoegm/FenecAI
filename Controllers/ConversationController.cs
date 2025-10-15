// File: Controllers/ConversationController.cs
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Language AI - CLU")]
	[Produces("application/json")]
	public class ConversationController : ControllerBase
	{
		private readonly IConversationService _conversationService;

		public ConversationController(IConversationService conversationService)
		{
			_conversationService = conversationService;
		}

		/// <summary>
		/// Analyzes a text using Azure CLU and returns predicted intent and entities.
		/// </summary>
		/// <param name="request">User input text to analyze.</param>
		/// <returns>Predicted intent and recognized entities.</returns>
		[HttpPost("analyze")]
		[SwaggerOperation(
			Summary = "Analyze text using Azure CLU",
			Description = "Sends text to a CLU project and returns top intent and entities."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(ConversationalAnalysisResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> Analyze([FromBody] ConversationalAnalysisRequest request)
		{
			var result = await _conversationService.AnalyzeConversationAsync(request);
			return Ok(result);
		}
	}
}
