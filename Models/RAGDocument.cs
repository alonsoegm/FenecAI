using System.Text.Json.Serialization;

namespace FenecAI.API.Models
{
	public class RAGDocument
	{
		[JsonPropertyName("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[JsonPropertyName("content")]
		public string Content { get; set; } = string.Empty;

		[JsonPropertyName("category")]
		public string Category { get; set; } = "general";

		// 👇 el nombre exacto del campo vectorial en tu índice
		[JsonPropertyName("contentVector")]
		public float[] ContentVector { get; set; } = Array.Empty<float>();
	}
}
