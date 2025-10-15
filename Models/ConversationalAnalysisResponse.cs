namespace FenecAI.API.Models
{
	public class ConversationalAnalysisResponse
	{
		public string TopIntent { get; set; } = string.Empty;
		public double Confidence { get; set; }
		public Dictionary<string, double> Intents { get; set; } = new();
		public Dictionary<string, string> Entities { get; set; } = new();
	}
}
