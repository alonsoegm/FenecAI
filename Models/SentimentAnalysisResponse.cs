namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the response returned after analyzing sentiment in text.
	/// </summary>
	public class SentimentAnalysisResponse
	{
		/// <summary>
		/// The overall sentiment label (Positive, Neutral, or Negative).
		/// </summary>
		public string Sentiment { get; set; } = string.Empty;

		/// <summary>
		/// The confidence score for positive sentiment.
		/// </summary>
		public double PositiveScore { get; set; }

		/// <summary>
		/// The confidence score for neutral sentiment.
		/// </summary>
		public double NeutralScore { get; set; }

		/// <summary>
		/// The confidence score for negative sentiment.
		/// </summary>
		public double NegativeScore { get; set; }
	}
}
