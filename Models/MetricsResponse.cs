namespace FenecAI.API.Models
{
	public class MetricsResponse
	{
		public string Model { get; set; } = string.Empty;
		public int PromptTokens { get; set; }
		public int CompletionTokens { get; set; }
		public int TotalTokens { get; set; }
		public double EstimatedCostUsd { get; set; }
		public double ResponseTimeMs { get; set; }
		public string Message { get; set; } = string.Empty;
	}
}
