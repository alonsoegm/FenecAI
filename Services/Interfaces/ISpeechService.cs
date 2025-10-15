using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	/// <summary>
	/// Defines operations for Azure Speech Service (Text-to-Speech, Speech-to-Text, etc.).
	/// </summary>
	public interface ISpeechService
	{
		/// <summary>
		/// Converts input text to synthesized speech and returns the resulting audio stream.
		/// </summary>
		Task<byte[]> ConvertTextToSpeechAsync(TextToSpeechRequest request);

		/// <summary>
		/// Converts an uploaded SSML (XML) file into synthesized speech audio (MP3).
		/// </summary>
		/// <param name="xmlFile">Uploaded XML file containing SSML markup.</param>
		/// <returns>Byte array representing the MP3 audio file.</returns>
		Task<byte[]> ConvertSsmlFileToSpeechAsync(IFormFile xmlFile);

		/// <summary>
		/// Converts an uploaded audio file into text using Azure Speech-to-Text.
		/// </summary>
		Task<string> ConvertSpeechToTextAsync(IFormFile audioFile);
	}
}
