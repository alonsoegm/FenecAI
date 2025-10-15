namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the input request for text translation.
	/// </summary>
	public class TranslationRequest
	{
		/// <summary>
		/// The text to be translated.
		/// </summary>
		public required string Text { get; set; }

		/// <summary>
		/// The target language code (ISO 639-1), e.g., "en", "es", "fr", "de".
		/// </summary>
		public required string TargetLanguage { get; set; }
	}
}
