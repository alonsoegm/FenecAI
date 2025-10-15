// File: Controllers/QnAController.cs
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Language AI - QnA")]
	[Produces("application/json")]
	public class QnAController : ControllerBase
	{
		private readonly IQnAService _qnaService;

		public QnAController(IQnAService qnaService)
		{
			_qnaService = qnaService;
		}

		/// <summary>
		/// Sends a question to the FenecAI knowledge base and returns the most relevant answer.
		/// </summary>
		/// <param name="request">Contains the user's question text.</param>
		/// <returns>Top matching answer with confidence score and alternate options.</returns>
		[HttpPost("ask")]
		[SwaggerOperation(
			Summary = "Ask FenecAI a question.",
			Description = "Queries the FenecAI QnA Knowledge Base using Azure Language Service and returns the top answer."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(QnAResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Ask([FromBody] QnARequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Question))
				return BadRequest("Question cannot be empty.");

			var result = await _qnaService.GetAnswerAsync(request);
			return Ok(result);
		}
	}
}
