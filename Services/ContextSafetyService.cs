using Azure;
using Azure.AI.ContentSafety;
using FenecAI.API.Models;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Provides text and image content safety analysis using Azure AI Content Safety.
	/// This service detects potentially harmful content across multiple categories
	/// such as hate, self-harm, sexual, and violence.
	/// </summary>
	public class ContextSafetyService
	{
		private readonly ContentSafetyClient _safetyClient;
		private readonly ILogger<ContextSafetyService> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextSafetyService"/> class.
		/// </summary>
		/// <param name="config">Application configuration containing the Azure Content Safety credentials.</param>
		/// <param name="logger">Logger for tracking operations and exceptions.</param>
		public ContextSafetyService(IConfiguration config, ILogger<ContextSafetyService> logger)
		{
			_logger = logger;

			_safetyClient = new ContentSafetyClient(
				new Uri(config["AzureContentSafety:Endpoint"]),
				new AzureKeyCredential(config["AzureContentSafety:ApiKey"]));
		}

		/// <summary>
		/// Analyzes a text input and classifies its content into harmful categories such as hate, self-harm, sexual, and violence.
		/// </summary>
		/// <param name="text">The text to be analyzed.</param>
		/// <returns>
		/// A <see cref="ContentSafetyAnalysis"/> object containing severity scores for each content category,
		/// a boolean flag indicating whether the content should be blocked, and the raw analysis results.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Azure Content Safety returns severity levels from <b>0 (safe)</b> to <b>4 (highly unsafe)</b>.
		/// If any category has a severity above 2, or if it matches an existing blocklist, the content is flagged as <b>Blocked = true</b>.
		/// </para>
		/// <para>
		/// This method is used for textual content moderation, ensuring that the system adheres to Responsible AI principles.
		/// </para>
		/// </remarks>
		public async Task<ContentSafetyAnalysis> AnalyzeTextAsync(string text)
		{
			// Prepare and send the analysis request to Azure.
			var request = new AnalyzeTextOptions(text);
			var response = await _safetyClient.AnalyzeTextAsync(request);
			var result = response.Value;

			_logger.LogInformation("Text safety analysis completed for input: {Preview}",
				text[..Math.Min(60, text.Length)]);

			// Extract severity scores for each category.
			var hateSeverity = result.CategoriesAnalysis.FirstOrDefault(c => c.Category == TextCategory.Hate)?.Severity ?? 0;
			var selfHarmSeverity = result.CategoriesAnalysis.FirstOrDefault(c => c.Category == TextCategory.SelfHarm)?.Severity ?? 0;
			var sexualSeverity = result.CategoriesAnalysis.FirstOrDefault(c => c.Category == TextCategory.Sexual)?.Severity ?? 0;
			var violenceSeverity = result.CategoriesAnalysis.FirstOrDefault(c => c.Category == TextCategory.Violence)?.Severity ?? 0;

			// Build the structured response.
			return new ContentSafetyAnalysis
			{
				HateSeverity = hateSeverity,
				SelfHarmSeverity = selfHarmSeverity,
				SexualSeverity = sexualSeverity,
				ViolenceSeverity = violenceSeverity,
				Blocked = result.BlocklistsMatch.Any()
					|| hateSeverity > 2
					|| selfHarmSeverity > 2
					|| sexualSeverity > 2
					|| violenceSeverity > 2,
				Raw = result.CategoriesAnalysis.ToDictionary(
					c => c.Category.ToString(),
					c => c.Severity ?? 0)
			};
		}

		/// <summary>
		/// Analyzes an uploaded image and evaluates its safety level across multiple harmful content categories.
		/// </summary>
		/// <param name="imageFile">The image file uploaded by the user.</param>
		/// <returns>
		/// A <see cref="ContentSafetyAnalysis"/> object containing severity scores, a blocking flag, and raw data.
		/// </returns>
		/// <exception cref="ArgumentException">Thrown if the uploaded file is null or empty.</exception>
		/// <remarks>
		/// <para>
		/// This method converts the uploaded file into a <see cref="BinaryData"/> stream and sends it to the Azure Content Safety service.
		/// </para>
		/// <para>
		/// Interpretation tip:
		/// - Severity levels range from 0 (safe) to 4 (extremely unsafe).  
		/// - If any score is greater than 2, the image is marked as unsafe (<b>Blocked = true</b>).
		/// </para>
		/// </remarks>
		public async Task<ContentSafetyAnalysis> AnalyzeUploadedImageAsync(IFormFile imageFile)
		{
			try
			{
				if (imageFile == null || imageFile.Length == 0)
					throw new ArgumentException("Image file cannot be empty.");

				using var ms = new MemoryStream();
				await imageFile.CopyToAsync(ms);
				ms.Position = 0;

				// Convert image stream into a binary payload.
				var imageData = new ContentSafetyImageData(BinaryData.FromBytes(ms.ToArray()));
				var analyzeRequest = new AnalyzeImageOptions(imageData);

				// Send the image analysis request.
				var response = await _safetyClient.AnalyzeImageAsync(analyzeRequest);
				var result = response.Value;

				_logger.LogInformation("Image safety analysis completed for uploaded file: {FileName}", imageFile.FileName);

				// Extract severity scores per category.
				var hateSeverity = result.CategoriesAnalysis.FirstOrDefault(c => c.Category.ToString() == TextCategory.Hate.ToString())?.Severity ?? 0;
				var selfHarmSeverity = result.CategoriesAnalysis.FirstOrDefault(c => c.Category.ToString() == TextCategory.SelfHarm.ToString())?.Severity ?? 0;
				var sexualSeverity = result.CategoriesAnalysis.FirstOrDefault(c => c.Category.ToString() == TextCategory.Sexual.ToString())?.Severity ?? 0;
				var violenceSeverity = result.CategoriesAnalysis.FirstOrDefault(c => c.Category.ToString() == TextCategory.Violence.ToString())?.Severity ?? 0;

				// Build the structured result.
				return new ContentSafetyAnalysis
				{
					HateSeverity = hateSeverity,
					SelfHarmSeverity = selfHarmSeverity,
					SexualSeverity = sexualSeverity,
					ViolenceSeverity = violenceSeverity,
					Blocked = hateSeverity > 2
						|| selfHarmSeverity > 2
						|| sexualSeverity > 2
						|| violenceSeverity > 2,
					Raw = result.CategoriesAnalysis.ToDictionary(
						c => c.Category.ToString(),
						c => c.Severity ?? 0)
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error analyzing uploaded image: {FileName}", imageFile?.FileName);
				throw;
			}
		}
	}
}
