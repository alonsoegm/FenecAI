using Azure.AI.Vision.ImageAnalysis;

namespace FenecAI.API.Services.Interfaces
{
	public interface IImageAnalysisService
	{
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
		Task<ImageAnalysisResult> AnalyzeImageAsync(Stream imageStream);

		/// <summary>
		/// Analyzes an image using Azure AI Vision and generates a natural-language caption.
		/// </summary>
		/// <param name="imageStream">The input image stream to be analyzed.</param>
		/// <returns>
		/// A string containing the generated caption and its confidence score.
		/// </returns>
		Task<(string Caption, double Confidence)> GenerateCaptionAsync(Stream imageStream);

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
		Task<IEnumerable<object>> DetectObjectsAsync(Stream imageStream);

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
		Task<IEnumerable<object>> ExtractTextAsync(Stream imageStream);
	}
}
