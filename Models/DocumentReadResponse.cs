namespace FenecAI.API.Models
{
		// File: Models/DocumentReadResponse.cs
	public class DocumentReadResponse
	{
		public string FullText { get; set; } = string.Empty;
		public List<string> Pages { get; set; } = new();
	}
}
