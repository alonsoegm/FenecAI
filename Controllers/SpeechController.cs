using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Controller for Azure Speech operations (Text-to-Speech, Speech-to-Text, etc.).
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Speech AI - Services")]
	[Produces("application/json")]
	public class SpeechController : ControllerBase
	{
		private readonly ISpeechService _speechService;

		public SpeechController(ISpeechService speechService)
		{
			_speechService = speechService;
		}

		/// <summary>
		/// Converts the given text into synthesized speech using Azure Speech Service.
		/// </summary>
		/// <param name="request">Text and optional voice/language configuration.</param>
		/// <returns>Base64-encoded audio representing the spoken version of the input text.</returns>
		[HttpPost("text-to-speech-file")]
		[SwaggerOperation(
			Summary = "Convert text to speech (WAV).",
			Description = "Converts the provided text into speech using Azure Cognitive Services and returns it as a downloadable wav file."
		)]
		[Consumes("application/json")]
		[Produces("audio/mpeg")]
		public async Task<IActionResult> ConvertTextToSpeechFile([FromBody] TextToSpeechRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest("Text cannot be empty.");

			var audioBytes = await _speechService.ConvertTextToSpeechAsync(request);

			return File(
				fileContents: audioBytes,
				contentType: "audio/mpeg",
				fileDownloadName: "speech_output.wav"
			);
		}

		/// <summary>
		/// Converts an uploaded XML (SSML) file into a downloadable wav audio using Azure Speech Service.
		/// </summary>
		/// <param name="xmlFile">The XML (SSML) file uploaded by the user.</param>
		/// <returns>An wav audio file for download and playback.</returns>
		[HttpPost("ssml-to-speech")]
		[SwaggerOperation(
			Summary = "Convert SSML (XML) file to speech (WAV).",
			Description = "Uploads an XML file containing SSML markup and generates a downloadable wav audio file using Azure Speech Service."
		)]
		[Consumes("multipart/form-data")]
		[Produces("audio/mpeg")]
		public async Task<IActionResult> ConvertSsmlToSpeech([FromForm] SpeechXmlRequest request)
		{
			if (request.XmlFile == null || request.XmlFile.Length == 0)
				return BadRequest("Please upload a valid XML file.");


			var audioBytes = await _speechService.ConvertSsmlFileToSpeechAsync(request.XmlFile);

			return File(
				fileContents: audioBytes,
				contentType: "audio/mpeg",
				fileDownloadName: "ssml_output.wav"
			);
		}

		/// <summary>
		/// Converts an uploaded audio file (WAV) to text using Azure Speech Service.
		/// </summary>
		/// <param name="audioFile">The audio file to transcribe.</param>
		/// <returns>Recognized text from the audio.</returns>
		[HttpPost("speech-to-text")]
		[SwaggerOperation(
			Summary = "Convert speech audio to text.",
			Description = "Uploads an audio file (WAV/wav) and transcribes it into text using Azure Speech Service."
		)]
		[Consumes("multipart/form-data")]
		[Produces("application/json")]
		public async Task<IActionResult> ConvertSpeechToText([FromForm] SpeechToTextRequest file)
		{
			if (file.AudioFile == null || file.AudioFile.Length == 0)
				return BadRequest("Please upload a valid audio file.");

			if (!file.AudioFile.FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
				return BadRequest("Only WAV files are supported. Please upload a .wav file.");

			var text = await _speechService.ConvertSpeechToTextAsync(file.AudioFile);

			return Ok(new { recognizedText = text });
		}


	}
}
