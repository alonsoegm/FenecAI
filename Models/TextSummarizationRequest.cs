namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the input request for text summarization.
	/// </summary>
	public class TextSummarizationRequest
	{
		/// <summary>
		/// The text input to summarize.
		/// </summary>
		public required string Text { get; set; }
	}
}
