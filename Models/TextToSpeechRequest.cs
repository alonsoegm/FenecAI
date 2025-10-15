namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents a request to convert text into speech.
	/// </summary>
	public class TextToSpeechRequest
	{
		/// <summary>
		/// The text that will be synthesized into speech.
		/// </summary>
		public required string Text { get; set; }

		/// <summary>
		/// The language code (e.g., "en-US", "es-ES").
		/// </summary>
		public string Language { get; set; } = "en-US";

		/// <summary>
		/// Optional voice name, e.g., "en-US-JennyNeural".
		/// </summary>
		public string VoiceName { get; set; } = "en-US-JennyNeural";
	}
}
