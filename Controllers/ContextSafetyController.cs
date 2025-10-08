using FenecAI.API.Models;
using FenecAI.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FenecAI.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ContextSafetyController : ControllerBase
	{
		private readonly ContextSafetyService _contextSafetyService;

		public ContextSafetyController(ContextSafetyService contextSafetyService)
		{
			_contextSafetyService = contextSafetyService;
		}

		[HttpPost("analyze")]
		public async Task<IActionResult> AnalyzeText([FromBody] SafetyRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest(new { message = "Text cannot be empty." });

			var result = await _contextSafetyService.AnalyzeTextAsync(request.Text);

			return Ok(new
			{
				request.Text,
				result.Blocked,
				result.Raw,
				message = result.Blocked
					? "⚠️ Unsafe content detected. Action blocked."
					: "✅ Content passed safety evaluation."
			});
		}

		[HttpPost("image")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> AnalyzeUploadedImage([FromForm] ImageUploadRequest request)
		{
			if (request == null || request.Image.Length == 0)
				return BadRequest(new { message = "You must upload a valid image file." });

			try
			{
				var result = await _contextSafetyService.AnalyzeUploadedImageAsync(request.Image);

				return Ok(new
				{
					fileName = request.Image.FileName,
					result.Blocked,
					result.Raw,
					message = result.Blocked
						? "⚠️ Unsafe image detected. Upload blocked."
						: "✅ Image passed content safety evaluation."
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error analyzing image: {ex.Message}" });
			}
		}


	}
}
