namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the input request for sentiment analysis.
	/// </summary>
	public class SentimentAnalysisRequest
	{
		/// <summary>
		/// The text input to analyze for sentiment.
		/// </summary>
		public required string Text { get; set; }
	}
}
