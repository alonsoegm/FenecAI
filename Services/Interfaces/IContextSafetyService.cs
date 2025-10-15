using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IContextSafetyService
	{
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
		Task<ContentSafetyAnalysis> AnalyzeTextAsync(string text);

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
		Task<ContentSafetyAnalysis> AnalyzeUploadedImageAsync(IFormFile imageFile);
	}
}
