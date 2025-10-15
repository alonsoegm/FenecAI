using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	/// <summary>
	/// Defines the contract for Azure AI Language operations.
	/// </summary>
	public interface ILanguageService
	{
		/// <summary>
		/// Detects the language of a given text using Azure AI Language API.
		/// </summary>
		/// <param name="request">Input containing the text to analyze.</param>
		/// <returns>
		/// A response with the detected language code (ISO 639-1) 
		/// and confidence score.
		/// </returns>
		Task<LanguageDetectionResponse> DetectLanguageAsync(LanguageDetectionRequest request);

		/// <summary>
		/// Extracts key phrases from the given text using Azure AI Language.
		/// </summary>
		Task<KeyPhraseExtractionResponse> ExtractKeyPhrasesAsync(KeyPhraseExtractionRequest request);

		/// <summary>
		/// Performs sentiment analysis on the provided text.
		/// </summary>
		Task<SentimentAnalysisResponse> AnalyzeSentimentAsync(SentimentAnalysisRequest request);

		/// <summary>
		/// Extracts named entities (people, locations, organizations, etc.) from text.
		/// </summary>
		Task<EntityExtractionResponse> ExtractEntitiesAsync(EntityExtractionRequest request);

		/// <summary>
		/// Summarizes long text content using Azure AI Language.
		/// </summary>
		Task<TextSummarizationResponse> SummarizeTextAsync(TextSummarizationRequest request);

		/// <summary>
		/// Translates text to the specified language using Azure Translator API.
		/// </summary>
		Task<TranslationResponse> TranslateTextAsync(TranslationRequest request);
	}
}
