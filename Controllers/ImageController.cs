using Azure.Storage.Blobs;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Provides endpoints for image generation using Azure OpenAI (DALL·E 3)
	/// with built-in Responsible AI filtering and optional Azure Blob Storage persistence.
	/// </summary>
	/// <remarks>
	/// <para>
	/// These endpoints allow generating AI-powered images based on text prompts.
	/// The API performs **content safety validation** before and after generation to ensure
	/// Responsible AI compliance, as required in the AI-102 exam objectives.
	/// </para>
	/// </remarks>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Generative AI - Image Generation")]
	[Produces("application/json")]
	public class ImageController : ControllerBase
	{
		private readonly IImageService _imageService;
		private readonly IContextSafetyService _contextSafetyService;
		private readonly IConfiguration _config;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageController"/> class.
		/// </summary>
		/// <param name="config">Application configuration for Azure storage and AI settings.</param>
		/// <param name="imageService">Service responsible for DALL·E image generation.</param>
		/// <param name="contextSafetyService">Service for evaluating prompt and image safety.</param>
		public ImageController(IConfiguration config, IImageService imageService, IContextSafetyService contextSafetyService)
		{
			_imageService = imageService;
			_config = config;
			_contextSafetyService = contextSafetyService;
		}

		/// <summary>
		/// Generates an image from a text prompt using DALL·E 3 in Azure OpenAI.
		/// </summary>
		/// <param name="request">
		/// The <see cref="ImageRequest"/> containing:
		/// - <b>Prompt:</b> description of the desired image  
		/// - <b>Size:</b> image dimensions (e.g. 256x256, 512x512, 1024x1024)
		/// </param>
		/// <returns>
		/// An object containing:
		/// - <b>imageUrl:</b> temporary Azure-hosted URL  
		/// - <b>revisedPrompt:</b> DALL·E’s optimized interpretation of the prompt  
		/// - <b>size:</b> image resolution used  
		/// </returns>
		/// <remarks>
		/// <para>
		/// Example:
		/// <code>
		/// {
		///   "prompt": "A cyberpunk fox meditating under neon lights",
		///   "size": "1024x1024"
		/// }
		/// </code>
		/// </para>
		/// </remarks>
		[HttpPost("generate")]
		[SwaggerOperation(
			Summary = "Generate an image from text (DALL·E 3)",
			Description = "Creates an AI-generated image from a text prompt using Azure OpenAI’s DALL·E 3 model with built-in content safety checks."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GenerateImage([FromBody] ImageRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Prompt))
				return BadRequest(new { message = "Prompt cannot be empty." });

			// Step 1: Responsible AI pre-check
			var analysis = await _contextSafetyService.AnalyzeTextAsync(request.Prompt);
			if (analysis.Blocked || analysis.HateSeverity > 2 || analysis.ViolenceSeverity > 2)
				return BadRequest(new { message = "Unsafe prompt detected. Please rephrase." });

			try
			{
				// Step 2: Generate image
				var (url, revisedPrompt) = await _imageService.GenerateImageAsync(request);

				// Step 3: Return metadata
				return Ok(new
				{
					prompt = request.Prompt,
					size = request.Size,
					imageUrl = url,
					revisedPrompt,
					message = "✅ Image generated successfully."
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error generating image: {ex.Message}" });
			}
		}

		/// <summary>
		/// Generates an image and automatically saves it to Azure Blob Storage after safety validation.
		/// </summary>
		/// <param name="request">The <see cref="ImageRequest"/> containing the prompt and desired size.</param>
		/// <returns>
		/// The Azure Blob Storage URI where the image was saved, and the revised prompt used by DALL·E.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This endpoint performs three stages:
		/// <list type="number">
		/// <item><description>Generates the image using Azure OpenAI.</description></item>
		/// <item><description>Performs content safety analysis on the generated image.</description></item>
		/// <item><description>Saves the validated image to Azure Blob Storage.</description></item>
		/// </list>
		/// </para>
		/// <para>
		/// Example:
		/// <code>
		/// {
		///   "prompt": "A futuristic fox coding in a neon-lit city",
		///   "size": "1024x1024"
		/// }
		/// </code>
		/// </para>
		/// </remarks>
		[HttpPost("generate-and-save")]
		[SwaggerOperation(
			Summary = "Generate and save image to Azure Blob Storage",
			Description = "Generates an image using DALL·E, validates it for safety, and uploads it to an Azure Blob container."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GenerateAndSave([FromBody] ImageRequest request)
		{
			var result = await GenerateImage(request) as OkObjectResult;
			if (result == null)
				return BadRequest(new { message = "Error generating image." });

			var imageData = (dynamic)result.Value;

			// Step 1: Post-generation safety validation
			var analysis = await _contextSafetyService.AnalyzeUploadedImageAsync(imageData.imageUrl);
			if (analysis.Blocked)
				return BadRequest(new { message = "Generated image failed content safety validation." });

			try
			{
				// Step 2: Save image to Blob Storage
				using var http = new HttpClient();
				var bytes = await http.GetByteArrayAsync(imageData.imageUrl);

				var blobService = new BlobServiceClient(_config["AzureStorage:ConnectionString"]);
				var container = blobService.GetBlobContainerClient(_config["AzureStorage:DalleContainer"]);
				await container.CreateIfNotExistsAsync();

				var blob = container.GetBlobClient($"generated/{Guid.NewGuid()}.png");
				using var ms = new MemoryStream(bytes);
				await blob.UploadAsync(ms, overwrite: true);

				return Ok(new
				{
					blobUri = blob.Uri,
					imageData.revisedPrompt,
					message = "✅ Image saved successfully to Azure Blob Storage."
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Error saving image: {ex.Message}" });
			}
		}
	}
}
