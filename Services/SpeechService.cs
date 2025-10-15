// File: Services/SpeechService.cs
using Microsoft.CognitiveServices.Speech;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;
using Microsoft.CognitiveServices.Speech.Audio;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Provides operations for Azure Speech Service, including Text-to-Speech synthesis.
	/// </summary>
	public class SpeechService : ISpeechService
	{
		private readonly IConfiguration _config;

		public SpeechService(IConfiguration config)
		{
			_config = config;
		}

		/// <summary>
		/// Converts the provided text into synthesized speech using Azure Speech SDK.
		/// </summary>
		/// <param name="request">The text, language, and voice configuration.</param>
		/// <returns>A <see cref="TextToSpeechResponse"/> containing Base64-encoded audio data.</returns>
		// File: Services/SpeechService.cs
		public async Task<byte[]> ConvertTextToSpeechAsync(TextToSpeechRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				throw new ArgumentException("Text cannot be empty.", nameof(request.Text));

			// Retrieve Azure Speech credentials
			var endpoint = _config["AzureAI:CognitiveEndpoint"];
			var key = _config["AzureAI:CognitiveKey"];

			// Initialize Speech configuration
			var speechConfig = SpeechConfig.FromEndpoint(new Uri(endpoint), key);
			speechConfig.SpeechSynthesisLanguage = request.Language ?? "en-US";
			speechConfig.SpeechSynthesisVoiceName = request.VoiceName ?? "en-US-JennyNeural";

			// 🎧 Output format: wav
			speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);

			// Create synthesizer
			using var synthesizer = new SpeechSynthesizer(speechConfig, null);

			// Perform speech synthesis
			var result = await synthesizer.SpeakTextAsync(request.Text);

			if (result.Reason == ResultReason.SynthesizingAudioCompleted)
			{
				return result.AudioData;
			}
			else if (result.Reason == ResultReason.Canceled)
			{
				var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
				throw new Exception($"Speech synthesis canceled: {cancellation.Reason}. Details: {cancellation.ErrorDetails}");
			}

			throw new Exception("Speech synthesis failed for unknown reasons.");
		}


		/// <summary>
		/// Converts the provided SSML (XML) file into an MP3 audio stream using Azure Speech Service.
		/// </summary>
		/// <param name="xmlFile">Uploaded XML file containing SSML markup.</param>
		/// <returns>Byte array representing the synthesized MP3 audio.</returns>
		public async Task<byte[]> ConvertSsmlFileToSpeechAsync(IFormFile xmlFile)
		{
			if (xmlFile == null || xmlFile.Length == 0)
				throw new ArgumentException("Invalid or empty XML file.");

			// Read XML content from the uploaded file
			string xmlContent;
			using (var reader = new StreamReader(xmlFile.OpenReadStream()))
			{
				xmlContent = await reader.ReadToEndAsync();
			}

			// Get Azure Speech credentials
			var endpoint = _config["AzureAI:CognitiveEndpoint"];
			var key = _config["AzureAI:CognitiveKey"];

			// Initialize Speech configuration
			var speechConfig = SpeechConfig.FromEndpoint(new Uri(endpoint), key);
			speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);

			// Create synthesizer
			using var synthesizer = new SpeechSynthesizer(speechConfig, null);
			var result = await synthesizer.SpeakSsmlAsync(xmlContent);

			// Handle success or cancellation
			if (result.Reason == ResultReason.SynthesizingAudioCompleted)
			{
				return result.AudioData;
			}
			else if (result.Reason == ResultReason.Canceled)
			{
				var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
				throw new Exception($"Speech synthesis canceled: {cancellation.Reason} - {cancellation.ErrorDetails}");
			}

			throw new Exception("Speech synthesis failed for unknown reasons.");
		}


		/// <summary>
		/// Transcribes an uploaded audio file into text using Azure Speech-to-Text.
		/// </summary>
		public async Task<string> ConvertSpeechToTextAsync(IFormFile audioFile)
		{
			if (audioFile == null || audioFile.Length == 0)
				throw new ArgumentException("Please upload a valid audio file.");

			// Save to temporary file
			var tempFile = Path.GetTempFileName();

			try
			{
				await using (var stream = new FileStream(tempFile, FileMode.Create))
				{
					await audioFile.CopyToAsync(stream);
				}

				var endpoint = _config["AzureAI:CognitiveEndpoint"];
				var key = _config["AzureAI:CognitiveKey"];

				var speechConfig = SpeechConfig.FromEndpoint(new Uri(endpoint), key);
				speechConfig.SpeechRecognitionLanguage = "en-US";

				// Create recognizer using the file
				string recognizedText = string.Empty;
				using (var audioInput = AudioConfig.FromWavFileInput(tempFile))
				using (var recognizer = new SpeechRecognizer(speechConfig, audioInput))
				{
					var result = await recognizer.RecognizeOnceAsync();

					if (result.Reason == ResultReason.RecognizedSpeech)
					{
						recognizedText = result.Text;
					}
					else if (result.Reason == ResultReason.Canceled)
					{
						var details = CancellationDetails.FromResult(result);
						throw new Exception($"Recognition canceled: {details.Reason} - {details.ErrorDetails}");
					}
					else
					{
						throw new Exception("Speech recognition failed for unknown reasons.");
					}
				}

				return recognizedText;
			}
			finally
			{
				// ✅ Ensure file is deleted only when it’s no longer locked
				if (File.Exists(tempFile))
				{
					try
					{
						File.Delete(tempFile);
					}
					catch
					{
						// Ignore cleanup errors to avoid crashing
					}
				}
			}
		}

	}
}
