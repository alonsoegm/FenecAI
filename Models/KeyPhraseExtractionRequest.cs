namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the request payload for extracting key phrases from text.
	/// </summary>
	public class KeyPhraseExtractionRequest
	{
		/// <summary>
		/// The text input to analyze and extract key phrases from.
		/// </summary>
		public required string Text { get; set; }
	}
}
