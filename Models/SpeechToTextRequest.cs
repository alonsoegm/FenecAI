namespace FenecAI.API.Models
{
	public class SpeechToTextRequest
	{
		public required IFormFile AudioFile { get; set; }
	}
}
