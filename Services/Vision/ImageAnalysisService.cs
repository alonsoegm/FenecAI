using Azure;
using Microsoft.Extensions.Options;
using FenecAI.API.Models.Settings;
using Azure.AI.Vision.ImageAnalysis;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Services.Vision
{
	/// <summary>
	/// Service responsible for analyzing images using Azure AI Vision.
	/// It communicates with the Azure Cognitive Services endpoint
	/// to extract visual information (e.g., tags, captions, objects).
	/// </summary>
	public class ImageAnalysisService: IImageAnalysisService
	{
		// Azure AI Vision endpoint URL
		private readonly string _endpoint;

		// Azure AI Vision subscription key (API key)
		private readonly string _key;

		/// <summary>
		/// Initializes the service by injecting the Azure Vision configuration
		/// (endpoint and key) from appsettings.json via dependency injection.
		/// </summary>
		/// <param name="options">Configuration options containing Azure Vision credentials.</param>
		public ImageAnalysisService(IOptions<AzureVisionSettings> options)
		{
			_endpoint = options.Value.Endpoint;
			_key = options.Value.Key;
		}

		/// <summary>
		/// Analyzes an image using Azure AI Vision and retrieves its visual analysis result.
		/// </summary>
		/// <param name="imageStream">The input image stream to be analyzed.</param>
		/// <returns>
		/// An <see cref="ImageAnalysisResult"/> object containing
		/// detected visual features such as tags, objects, and descriptions.
		/// </returns>
		/// <example>
		/// Example usage:
		/// <code>
		/// using var stream = File.OpenRead("image.png");
		/// var result = await _imageAnalysisService.AnalyzeImageAsync(stream);
		/// foreach (var tag in result.Tags.Values)
		/// {
		///     Console.WriteLine($"{tag.Name} ({tag.Confidence:P2})");
		/// }
		/// </code>
		/// </example>
		public async Task<ImageAnalysisResult> AnalyzeImageAsync(Stream imageStream)
		{
			// Create the Azure AI Vision client using endpoint and key credentials
			var client = new ImageAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));

			// Perform the image analysis by requesting the 'Tags' visual feature
			var result = await client.AnalyzeAsync(
				BinaryData.FromStream(imageStream),
				VisualFeatures.Tags
			);

			// Return the full analysis result, including detected tags
			return result.Value;
		}

		/// <summary>
		/// Analyzes an image using Azure AI Vision and generates a natural-language caption.
		/// </summary>
		/// <param name="imageStream">The input image stream to be analyzed.</param>
		/// <returns>
		/// A string containing the generated caption and its confidence score.
		/// </returns>
		public async Task<(string Caption, double Confidence)> GenerateCaptionAsync(Stream imageStream)
		{
			// Create the Azure AI Vision client using endpoint and key credentials
			var client = new ImageAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));

			// Perform image analysis requesting the 'Caption' visual feature
			var result = await client.AnalyzeAsync(
				BinaryData.FromStream(imageStream),
				VisualFeatures.Caption
			);

			// Retrieve the generated caption (Azure returns only one main caption)
			var captionResult = result.Value.Caption;

			// Return caption text and confidence score as a tuple
			return (captionResult.Text, captionResult.Confidence);
		}

		/// <summary>
		/// Analyzes an image using Azure AI Vision to detect objects and their bounding boxes.
		/// </summary>
		/// <param name="imageStream">The input image stream to be analyzed.</param>
		/// <returns>
		/// A collection of detected objects, each containing its name, confidence score,
		/// and bounding box coordinates (X, Y, Width, Height).
		/// </returns>
		/// <example>
		/// Example usage:
		/// <code>
		/// using var stream = File.OpenRead("image.png");
		/// var objects = await _imageAnalysisService.DetectObjectsAsync(stream);
		/// foreach (var obj in objects)
		/// {
		///     Console.WriteLine($"{obj.Name} ({obj.Confidence:P2}) at X:{obj.X}, Y:{obj.Y}");
		/// }
		/// </code>
		/// </example>
		public async Task<IEnumerable<object>> DetectObjectsAsync(Stream imageStream)
		{
			// Create the Azure AI Vision client using endpoint and key credentials
			var client = new ImageAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));

			// Perform the image analysis requesting the 'Objects' visual feature
			var result = await client.AnalyzeAsync(
				BinaryData.FromStream(imageStream),
				VisualFeatures.Objects
			);

			// Extract the detected objects from the response
			var detectedObjects = result.Value.Objects.Values.Select(obj => new
			{
				obj.Tags.FirstOrDefault()?.Name,     // Object label (e.g., "person", "car")
				obj.Tags.FirstOrDefault()?.Confidence, // Confidence score for the label
				obj.BoundingBox.X,
				obj.BoundingBox.Y,
				obj.BoundingBox.Width,
				obj.BoundingBox.Height
			});

			// Return the structured list of detected objects
			return detectedObjects;
		}

		/// <summary>
		/// Extracts printed or handwritten text from an image using Azure AI Vision's OCR (Read) feature.
		/// </summary>
		/// <param name="imageStream">The input image stream to be analyzed.</param>
		/// <returns>
		/// A collection of recognized text lines with their confidence scores.
		/// </returns>
		/// <example>
		/// Example usage:
		/// <code>
		/// using var stream = File.OpenRead("document.jpg");
		/// var lines = await _imageAnalysisService.ExtractTextAsync(stream);
		/// foreach (var line in lines)
		/// {
		///     Console.WriteLine($"{line.Text}");
		/// }
		/// </code>
		/// </example>
		public async Task<IEnumerable<object>> ExtractTextAsync(Stream imageStream)
		{
			// Create the Azure AI Vision client using endpoint and key credentials
			var client = new ImageAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));

			// Perform the image analysis requesting the 'Read' visual feature (OCR)
			var result = await client.AnalyzeAsync(
				BinaryData.FromStream(imageStream),
				VisualFeatures.Read
			);

			// Extract recognized lines of text from the Blocks property of ReadResult
			var lines = result.Value.Read?.Blocks
				.Select(block => new
				{
					Text = string.Join(" ", block.Lines.Select(line => line.Text)) // Combine all lines' text into a single string
				})
				.ToList();

			// Return an empty list if no text was found
			return lines?.Cast<object>() ?? new List<object>();
		}



	}
}
