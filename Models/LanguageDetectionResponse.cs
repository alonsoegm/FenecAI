namespace FenecAI.API.Models
{
	public class LanguageDetectionResponse
	{
		public string Language { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;
		public double Confidence { get; set; }
	}
}
