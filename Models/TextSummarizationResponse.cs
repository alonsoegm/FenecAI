namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the summarized output of the provided text.
	/// </summary>
	public class TextSummarizationResponse
	{
		/// <summary>
		/// The summarized version of the text.
		/// </summary>
		public string Summary { get; set; } = string.Empty;

		/// <summary>
		/// The number of sentences included in the summary.
		/// </summary>
		public int SentenceCount { get; set; }
	}
}
