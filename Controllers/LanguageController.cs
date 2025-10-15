using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Services.Interfaces;
using FenecAI.API.Models;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Controller responsible for handling all Azure AI Language API operations.
	/// Provides endpoints for text analysis, such as language detection.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Language AI - Services")]
	[Produces("application/json")]
	public class LanguageController : ControllerBase
	{
		private readonly ILanguageService _languageService;

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageController"/> class.
		/// Injects the <see cref="ILanguageService"/> dependency to access Azure AI Language features.
		/// </summary>
		/// <param name="languageService">Service abstraction for interacting with Azure AI Language APIs.</param>
		public LanguageController(ILanguageService languageService)
		{
			_languageService = languageService;
		}

		/// <summary>
		/// Detects the language of a given text using Azure AI Language API.
		/// </summary>
		/// <param name="request">The request payload containing the text to analyze.</param>
		/// <returns>
		/// Returns a <see cref="LanguageDetectionResponse"/> with the detected language, 
		/// ISO 639-1 code, and confidence score.
		/// </returns>
		/// <response code="200">Returns the detected language with confidence score.</response>
		/// <response code="400">Returned when the request text is empty or invalid.</response>
		/// <response code="500">Returned when an unexpected server error occurs.</response>
		[HttpPost("detect")]
		[SwaggerOperation(
			Summary = "Detect the language of a given text using Azure AI Language.",
			Description = "Analyzes input text and identifies the detected language and confidence score."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(LanguageDetectionResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DetectLanguage([FromBody] LanguageDetectionRequest request)
		{
			// Validate that the incoming text is not empty or whitespace.
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest("Text cannot be empty.");

			// Call the language detection service to process the request.
			var result = await _languageService.DetectLanguageAsync(request);

			// Return the successful response with language detection data.
			return Ok(result);
		}

		/// <summary>
		/// Extracts key phrases from the provided text using Azure AI Language.
		/// </summary>
		/// <param name="request">Request object containing the input text.</param>
		/// <returns>
		/// Returns a list of key phrases representing the most relevant terms or ideas from the text.
		/// </returns>
		/// <response code="200">Returns the list of extracted key phrases.</response>
		/// <response code="400">Returned when the input text is null or empty.</response>
		/// <response code="500">Returned when an internal error occurs.</response>
		[HttpPost("key-phrases")]
		[SwaggerOperation(
			Summary = "Extract key phrases from text using Azure AI Language.",
			Description = "Identifies important concepts and entities in the provided text."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(KeyPhraseExtractionResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> ExtractKeyPhrases([FromBody] KeyPhraseExtractionRequest request)
		{
			// Validate the request text before processing.
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest("Text cannot be empty.");

			// Call the service to perform key phrase extraction.
			var result = await _languageService.ExtractKeyPhrasesAsync(request);

			// Return the extracted key phrases.
			return Ok(result);
		}

		/// <summary>
		/// Analyzes the sentiment of the provided text using Azure AI Language SDK.
		/// </summary>
		/// <param name="request">Request containing the text to analyze.</param>
		/// <returns>
		/// A <see cref="SentimentAnalysisResponse"/> with overall sentiment and confidence scores.
		/// </returns>
		/// <response code="200">Sentiment analysis completed successfully.</response>
		/// <response code="400">Returned when the input text is empty or invalid.</response>
		/// <response code="500">Returned when an internal error occurs.</response>
		[HttpPost("sentiment")]
		[SwaggerOperation(
			Summary = "Analyze the sentiment of text using Azure AI Language.",
			Description = "Determines whether the text expresses a positive, neutral, or negative tone."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(SentimentAnalysisResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> AnalyzeSentiment([FromBody] SentimentAnalysisRequest request)
		{
			// Basic validation
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest("Text cannot be empty.");

			// Perform sentiment analysis
			var result = await _languageService.AnalyzeSentimentAsync(request);

			// Return the result
			return Ok(result);
		}

		/// <summary>
		/// Extracts named entities (people, organizations, dates, etc.) from the input text.
		/// </summary>
		/// <param name="request">Input text to analyze.</param>
		/// <returns>A list of recognized entities with category and confidence score.</returns>
		/// <response code="200">Entities successfully extracted.</response>
		/// <response code="400">Invalid or empty input text.</response>
		/// <response code="500">Server error during entity extraction.</response>
		[HttpPost("entities")]
		[SwaggerOperation(
			Summary = "Extract named entities from text using Azure AI Language.",
			Description = "Identifies entities such as people, locations, organizations, and more."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(EntityExtractionResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> ExtractEntities([FromBody] EntityExtractionRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest("Text cannot be empty.");

			var result = await _languageService.ExtractEntitiesAsync(request);
			return Ok(result);
		}

		/// <summary>
		/// Summarizes the input text using Azure AI Language (Extractive Summarization).
		/// </summary>
		/// <param name="request">Request containing the text to summarize.</param>
		/// <returns>
		/// A <see cref="TextSummarizationResponse"/> with summarized content and sentence count.
		/// </returns>
		/// <response code="200">Text summarized successfully.</response>
		/// <response code="400">Invalid or empty input text.</response>
		/// <response code="500">Internal server error during summarization.</response>
		[HttpPost("summarize")]
		[SwaggerOperation(
			Summary = "Summarize text using Azure AI Language.",
			Description = "Extracts the most important sentences to generate a concise summary."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(TextSummarizationResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> SummarizeText([FromBody] TextSummarizationRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest("Text cannot be empty.");

			var result = await _languageService.SummarizeTextAsync(request);
			return Ok(result);
		}

		/// <summary>
		/// Translates text to the specified target language using Azure Translator API.
		/// </summary>
		/// <param name="request">The translation request containing input text and target language.</param>
		/// <returns>A translated text response with detected language and target output.</returns>
		/// <response code="200">Text successfully translated.</response>
		/// <response code="400">Invalid or empty input text.</response>
		/// <response code="500">Server error while calling Translator API.</response>
		[HttpPost("translate")]
		[SwaggerOperation(
			Summary = "Translate text using Azure Translator.",
			Description = "Automatically detects the source language and translates the text into the specified target language."
		)]
		[Consumes("application/json")]
		[ProducesResponseType(typeof(TranslationResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> TranslateText([FromBody] TranslationRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				return BadRequest("Text cannot be empty.");

			if (string.IsNullOrWhiteSpace(request.TargetLanguage))
				return BadRequest("Target language cannot be empty.");

			var result = await _languageService.TranslateTextAsync(request);
			return Ok(result);
		}
	}
}
