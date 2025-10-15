namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents the response containing synthesized audio from text.
	/// </summary>
	public class TextToSpeechResponse
	{
		/// <summary>
		/// The Base64-encoded audio data.
		/// </summary>
		public string AudioBase64 { get; set; } = string.Empty;

		/// <summary>
		/// The MIME type of the audio file (e.g., "audio/wav").
		/// </summary>
		public string ContentType { get; set; } = "audio/wav";
	}
}
