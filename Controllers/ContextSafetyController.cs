using FenecAI.API.Models;
using FenecAI.API.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Provides endpoints for analyzing and validating text and image content
	/// using Azure Content Safety to ensure compliance with Responsible AI practices.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller helps detect and classify potentially harmful or unsafe content,
	/// such as hate speech, self-harm, sexual content, and violence — both in text and images.
	/// </para>
	/// <para>
	/// It aligns with **Responsible AI** objectives of the Azure AI Engineer (AI-102) certification.
	/// </para>
	/// </remarks>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Responsible AI - Content Safety")]
	[Produces("application/json")]
	public class ContextSafetyController : ControllerBase
	{
		private readonly ContextSafetyService _contextSafetyService;

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextSafetyController"/> class.
		/// </summary>
		/// <param name="contextSafetyService">Service that handles Azure Content Safety analysis.</param>
		public ContextSafetyController(ContextSafetyService contextSafetyService)
		{
			_contextSafetyService = contextSafetyService;
		}

		/// <summary>
		/// Analyzes a text input and classifies it for potentially harmful content.
		/// </summary>
		/// <param name="request">
		/// A <see cref="SafetyRequest"/> containing the text to analyze.
		/// </param>
		/// <returns>
		/// A detailed object showing severity levels for each detected category (Hate, Violence, Sexual, SelfHarm),
		/// and whether the content is considered blocked.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Example:
		/// <code>
		/// {
		///   "text": "I hate everyone and want to destroy everything."
		/// }
		/// </code>
		/// </para>
		/// <para>
		/// The service assigns severity scores (0–6) for each category:
		/// <list type="bullet">
		/// <item><description>0–2 = Safe</description></item>
		/// <item><description>3–4 = Moderate risk</description></item>
		/// <item><description>5–6 = High risk (blocked)</description></item>
		/// </list>
		/// </para>
		/// </remarks>
		[HttpPost("analyze")]
		[SwaggerOperation(
			Summary = "Analyze text for unsafe content",
			Description = "Detects and classifies text containing hate, violence, sexual, or self-harm content using Azure Content Safety."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> AnalyzeText([FromBody] SafetyRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest(new { message = "Text cannot be empty." });

			var result = await _contextSafetyService.AnalyzeTextAsync(request.Text);

			return Ok(new
			{
				input = request.Text,
				result.Blocked,
				result.Raw,
				interpretation = result.Blocked
					? "⚠️ Unsafe content detected. Action blocked."
					: "✅ Content passed safety evaluation."
			});
		}

		/// <summary>
		/// Analyzes an uploaded image for unsafe visual content.
		/// </summary>
		/// <param name="request">
		/// A <see cref="ImageUploadRequest"/> containing the image file to analyze.
		/// </param>
		/// <returns>
		/// Analysis result showing whether the image passed content safety validation.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Example (via Swagger or Postman):
		/// <code>
		/// POST /api/ContextSafety/image
		/// Content-Type: multipart/form-data
		/// image: [select a .png or .jpg file]
		/// </code>
		/// </para>
		/// <para>
		/// The image is analyzed for explicit or violent content using the
		/// <b>Azure AI Content Safety - Image API</b>.
		/// </para>
		/// </remarks>
		[HttpPost("image")]
		[SwaggerOperation(
			Summary = "Analyze uploaded image for unsafe content",
			Description = "Evaluates an image file for potential explicit or violent content using Azure Content Safety."
		)]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> AnalyzeUploadedImage([FromForm] ImageUploadRequest request)
		{
			if (request == null || request.Image == null || request.Image.Length == 0)
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
