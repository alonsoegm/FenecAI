namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the response returned by Azure AI Language service
	/// after performing key phrase extraction.
	/// </summary>
	public class KeyPhraseExtractionResponse
	{
		/// <summary>
		/// The list of key phrases identified in the analyzed text.
		/// </summary>
		public List<string> KeyPhrases { get; set; } = new();
	}
}
