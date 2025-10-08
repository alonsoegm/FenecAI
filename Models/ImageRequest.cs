namespace FenecAI.API.Models
{
	public class ImageRequest
	{
		public string Prompt { get; set; } = string.Empty;
		public string Size { get; set; } = "1024x1024"; // optional
	}
}
