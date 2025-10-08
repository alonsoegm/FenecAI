namespace FenecAI.API.Models
{
	public class RAGQueryRequest
	{
		public string Question { get; set; } = string.Empty;
		public int TopK { get; set; } = 3; // how many chunks to retrieve
	}
}
