namespace FenecAI.API.Models
{
	public class RAGQueryResponse
	{
		public string Answer { get; set; } = string.Empty;
		public List<string> Sources { get; set; } = new();
	}
}
