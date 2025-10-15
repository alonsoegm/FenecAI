namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the translated output text returned by Azure Translator.
	/// </summary>
	public class TranslationResponse
	{
		/// <summary>
		/// The detected source language of the input text.
		/// </summary>
		public string DetectedLanguage { get; set; } = string.Empty;

		/// <summary>
		/// The translated text in the target language.
		/// </summary>
		public string TranslatedText { get; set; } = string.Empty;

		/// <summary>
		/// The target language used for translation.
		/// </summary>
		public string TargetLanguage { get; set; } = string.Empty;
	}
}
