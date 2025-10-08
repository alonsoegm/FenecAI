namespace FenecAI.API.Models
{
	// Represents the AI response returned to the client
	public class ChatResponse
	{
		public string Output { get; set; } = string.Empty;
		public int TokensUsed { get; set; }
		public string ModelName { get; set; } = string.Empty;
	}
}
