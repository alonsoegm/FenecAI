namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the request payload for entity extraction.
	/// </summary>
	public class EntityExtractionRequest
	{
		/// <summary>
		/// The input text to analyze for named entities.
		/// </summary>
		public required string Text { get; set; }
	}
}
