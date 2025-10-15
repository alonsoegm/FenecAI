using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FenecAI.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Image Analysis")]
	[Produces("application/json")]
	public class ImageAnalysisController : ControllerBase
	{
		private readonly IImageAnalysisService _imageAnalysisService;

		// Inject the ImageAnalysisService through constructor dependency injection
		public ImageAnalysisController(IImageAnalysisService imageAnalysisService)
		{
			_imageAnalysisService = imageAnalysisService;
		}

		/// <summary>
		/// Analyzes an uploaded image using Azure AI Vision and returns the detected tags.
		/// </summary>
		/// <param name="image">The uploaded image file (IFormFile).</param>
		/// <returns>A JSON array with the detected tags and confidence scores.</returns>
		[HttpPost("analyze")]
		[SwaggerOperation(
			Summary = "Analyze an image and extract visual tags using Azure AI Vision.",
			Description = "Uploads an image and analyzes it using Azure AI Vision's Image Analysis service. The API returns a list of detected tags along with confidence scores, which represent the likelihood of each tag being accurate."
		)]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> AnalyzeImage([FromForm] ImageUploadRequest request)
		{
			// Validate the uploaded file
			if (request.Image == null || request.Image.Length == 0)
				return BadRequest("Please upload a valid image file.");

			try
			{
				// Open the uploaded image as a read-only stream
				using var stream = request.Image.OpenReadStream();

				// Call the Vision service to analyze the image
				var analysisResult = await _imageAnalysisService.AnalyzeImageAsync(stream);

				// Extract the tags from the result (each tag includes a name and confidence)
				var tags = analysisResult.Tags.Values.Select(tag => new
				{
					tag.Name,
					tag.Confidence
				});

				// Return the tags as a JSON response
				return Ok(new
				{
					Message = "Image analysis completed successfully.",
					Tags = tags
				});
			}
			catch (Exception ex)
			{
				// Handle unexpected errors gracefully
				return StatusCode(500, new
				{
					Message = "An error occurred while analyzing the image.",
					Error = ex.Message
				});
			}
		}

		/// <summary>
		/// Analyzes an uploaded image and generates a natural-language caption using Azure AI Vision.
		/// </summary>
		/// <param name="request">The uploaded image request (multipart/form-data).</param>
		/// <returns>
		/// A JSON response containing the generated caption and its confidence score.
		/// </returns>
		[HttpPost("caption")]
		[SwaggerOperation(
			Summary = "Generate a caption describing the content of an image using Azure AI Vision.",
			Description = "Uploads an image and analyzes it with Azure AI Vision's Image Analysis service to generate a descriptive caption in natural language. The response includes the caption text and a confidence score representing how certain the AI model is about the description."
		)]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GenerateCaption([FromForm] ImageUploadRequest request)
		{
			// Validate the uploaded file
			if (request.Image == null || request.Image.Length == 0)
				return BadRequest("Please upload a valid image file.");

			try
			{
				// Open the uploaded image as a read-only stream
				using var stream = request.Image.OpenReadStream();

				// Call the Vision service to generate a caption for the image
				var (caption, confidence) = await _imageAnalysisService.GenerateCaptionAsync(stream);

				// Return the caption and confidence score as a JSON response
				return Ok(new
				{
					Message = "Caption generated successfully.",
					Caption = caption,
					Confidence = confidence
				});
			}
			catch (Exception ex)
			{
				// Handle unexpected errors gracefully
				return StatusCode(500, new
				{
					Message = "An error occurred while generating the caption.",
					Error = ex.Message
				});
			}
		}

		/// <summary>
		/// Detects objects in an uploaded image using Azure AI Vision and returns their names, confidence, and bounding box coordinates.
		/// </summary>
		/// <param name="request">The uploaded image request (multipart/form-data).</param>
		/// <returns>
		/// A JSON response containing the list of detected objects with their bounding boxes and confidence scores.
		/// </returns>
		[HttpPost("objects")]
		[SwaggerOperation(
			Summary = "Detect objects within an image using Azure AI Vision.",
			Description = "Uploads an image and analyzes it using Azure AI Vision's Object Detection feature. The API returns a list of detected objects, each with a confidence score and bounding box coordinates representing their position in the image."
		)]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> DetectObjects([FromForm] ImageUploadRequest request)
		{
			// Validate the uploaded file
			if (request.Image == null || request.Image.Length == 0)
				return BadRequest("Please upload a valid image file.");

			try
			{
				// Open the uploaded image as a read-only stream
				using var stream = request.Image.OpenReadStream();

				// Call the Vision service to detect objects in the image
				var detectedObjects = await _imageAnalysisService.DetectObjectsAsync(stream);

				// Return the results as a JSON response
				return Ok(new
				{
					Message = "Object detection completed successfully.",
					Objects = detectedObjects
				});
			}
			catch (Exception ex)
			{
				// Handle unexpected errors gracefully
				return StatusCode(500, new
				{
					Message = "An error occurred while detecting objects in the image.",
					Error = ex.Message
				});
			}
		}

		/// <summary>
		/// Extracts printed or handwritten text from an uploaded image using Azure AI Vision OCR.
		/// </summary>
		/// <param name="request">The uploaded image request (multipart/form-data).</param>
		/// <returns>
		/// A JSON response containing the recognized text lines and their confidence scores.
		/// </returns>
		[HttpPost("ocr")]
		[SwaggerOperation(
			Summary = "Extract text from an image using Azure AI Vision OCR.",
			Description = "Uploads an image and analyzes it using Azure AI Vision's OCR (Read) capability. The API returns all detected lines of text along with confidence scores indicating recognition accuracy."
		)]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> ExtractText([FromForm] ImageUploadRequest request)
		{
			// Validate the uploaded file
			if (request.Image == null || request.Image.Length == 0)
				return BadRequest("Please upload a valid image file.");

			try
			{
				// Open the uploaded image as a read-only stream
				using var stream = request.Image.OpenReadStream();

				// Call the Vision service to extract text from the image
				var textLines = await _imageAnalysisService.ExtractTextAsync(stream);

				// Return the recognized text as a JSON response
				return Ok(new
				{
					Message = "Text extraction completed successfully.",
					Lines = textLines
				});
			}
			catch (Exception ex)
			{
				// Handle unexpected errors gracefully
				return StatusCode(500, new
				{
					Message = "An error occurred while extracting text from the image.",
					Error = ex.Message
				});
			}
		}



	}
}
