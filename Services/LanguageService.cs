using System.Text.Json;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;
using Azure.AI.TextAnalytics;
using Azure;


namespace FenecAI.API.Services
{
	/// <summary>
	/// Concrete implementation of <see cref="ILanguageService"/> that 
	/// interacts with the Azure AI Language API to perform text analysis operations.
	/// </summary>
	public class LanguageService : ILanguageService
	{
		private readonly IConfiguration _config;

		private readonly TextAnalyticsClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageService"/> class.
		/// Injects the configuration provider to access Azure service credentials and endpoints.
		/// </summary>
		/// <param name="config">Application configuration (used to retrieve endpoint and API key).</param>
		public LanguageService(IConfiguration config)
		{
			_config = config;
			var endpoint = new Uri(config["AzureAI:LanguageEndpoint"]!);
			var apiKey = new AzureKeyCredential(config["AzureAI:LanguageApiKey"]!);
			_client = new TextAnalyticsClient(endpoint, apiKey);
		}

		/// <summary>
		/// Detects the language of a given text using Azure AI Language service.
		/// Sends an HTTP POST request to the Azure endpoint with a "LanguageDetection" payload.
		/// </summary>
		/// <param name="request">Object containing the input text to analyze.</param>
		/// <returns>
		/// A <see cref="LanguageDetectionResponse"/> containing the ISO language code,
		/// full language name, and confidence score.
		/// </returns>
		/// <exception cref="HttpRequestException">Thrown when the API response indicates failure.</exception>
		public async Task<LanguageDetectionResponse> DetectLanguageHttpAsync(LanguageDetectionRequest request)
		{
			// Retrieve Azure AI Language endpoint and API key from configuration settings.
			var endpoint = _config["AzureAI:LanguageEndpoint"];
			var apiKey = _config["AzureAI:LanguageApiKey"];

			// Construct the REST API endpoint URL with the proper API version.
			var url = $"{endpoint}/language/:analyze-text?api-version=2024-11-01";

			// Build the JSON payload required by Azure AI Language API for language detection.
			var payload = new
			{
				kind = "LanguageDetection",
				parameters = new { modelVersion = "latest" },
				analysisInput = new
				{
					documents = new[]
					{
						// The API expects an array of documents; here we only send one text document.
						new { id = "1", text = request.Text }
					}
				}
			};

			// Create an HttpClient instance to communicate with Azure AI Language API.
			using var http = new HttpClient();

			// Add the subscription key header required for authentication.
			http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

			// Send the HTTP POST request with the payload as JSON.
			var response = await http.PostAsJsonAsync(url, payload);

			// Throw an exception if the request did not succeed (non-2xx status code).
			response.EnsureSuccessStatusCode();

			// Parse the JSON response into a JsonElement for dynamic access.
			var json = await response.Content.ReadFromJsonAsync<JsonElement>();

			// Navigate the JSON structure to extract the first (and only) document result.
			var document = json.GetProperty("results").GetProperty("documents")[0];

			// Extract the detected language details from the response.
			var detected = document.GetProperty("detectedLanguage");

			// Map the response data into a strongly-typed model.
			return new LanguageDetectionResponse
			{
				Language = detected.GetProperty("iso6391Name").GetString() ?? string.Empty,
				Description = detected.GetProperty("name").GetString() ?? string.Empty,
				Confidence = detected.GetProperty("confidenceScore").GetDouble()
			};
		}


		/// <summary>
		/// Extracts key phrases from the given text using Azure AI Language API.
		/// Identifies important concepts, entities, or topics within the text.
		/// </summary>
		/// <param name="request">The request containing the text to analyze.</param>
		/// <returns>A response object containing a list of extracted key phrases.</returns>
		public async Task<KeyPhraseExtractionResponse> ExtractKeyPhrasesHttpAsync(KeyPhraseExtractionRequest request)
		{
			// Retrieve the Azure AI Language endpoint and API key from configuration.
			var endpoint = _config["AzureAI:LanguageEndpoint"];
			var apiKey = _config["AzureAI:LanguageApiKey"];
			var url = $"{endpoint}/language/:analyze-text?api-version=2024-11-01";

			// Build the JSON payload for the Key Phrase Extraction request.
			var payload = new
			{
				kind = "KeyPhraseExtraction",
				parameters = new { modelVersion = "latest" },
				analysisInput = new
				{
					documents = new[]
					{
						// Only one document is analyzed here, but the API supports multiple.
						new { id = "1", text = request.Text }
					}
				}
			};

			// Create an HttpClient instance to call the Azure API.
			using var http = new HttpClient();
			http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

			// Send the request as JSON and await the response.
			var response = await http.PostAsJsonAsync(url, payload);
			response.EnsureSuccessStatusCode();

			// Parse the JSON response from Azure AI Language.
			var json = await response.Content.ReadFromJsonAsync<JsonElement>();

			// Navigate to the first document in the result.
			var document = json.GetProperty("results").GetProperty("documents")[0];

			// Extract the array of key phrases detected by the service.
			var keyPhrases = document.GetProperty("keyPhrases")
									 .EnumerateArray()
									 .Select(p => p.GetString() ?? string.Empty)
									 .Where(p => !string.IsNullOrWhiteSpace(p))
									 .ToList();

			// Return the key phrases in a strongly-typed response model.
			return new KeyPhraseExtractionResponse
			{
				KeyPhrases = keyPhrases
			};
		}


		/// <summary>
		/// Detects the language of a given text using Azure AI Language service.
		/// </summary>
		/// <param name="request">Object containing the input text to analyze.</param>
		/// <returns>
		/// A <see cref="LanguageDetectionResponse"/> containing the ISO language code,
		/// full language name, and confidence score.
		/// </returns>
		public async Task<LanguageDetectionResponse> DetectLanguageAsync(LanguageDetectionRequest request)
		{
			var result = await _client.DetectLanguageAsync(request.Text);

			return new LanguageDetectionResponse
			{
				Language = result.Value.Iso6391Name,
				Description = result.Value.Name,
				Confidence = result.Value.ConfidenceScore
			};
		}

		/// <summary>
		/// Extracts key phrases from the given text using Azure AI Language API.
		/// Identifies important concepts, entities, or topics within the text.
		/// </summary>
		/// <param name="request">The request containing the text to analyze.</param>
		/// <returns>A response object containing a list of extracted key phrases.</returns>
		public async Task<KeyPhraseExtractionResponse> ExtractKeyPhrasesAsync(KeyPhraseExtractionRequest request)
		{
			var response = await _client.ExtractKeyPhrasesAsync(request.Text);

			return new KeyPhraseExtractionResponse
			{
				KeyPhrases = response.Value.ToList()
			};
		}

		/// <summary>
		/// Analyzes the sentiment of the given text using Azure AI Language SDK.
		/// Determines whether the text expresses a positive, neutral, or negative tone.
		/// </summary>
		/// <param name="request">The text to analyze for sentiment.</param>
		/// <returns>A <see cref="SentimentAnalysisResponse"/> containing sentiment and confidence scores.</returns>
		public async Task<SentimentAnalysisResponse> AnalyzeSentimentAsync(SentimentAnalysisRequest request)
		{
			// Validate input text
			if (string.IsNullOrWhiteSpace(request.Text))
				throw new ArgumentException("Text cannot be null or empty.", nameof(request.Text));

			// Analyze sentiment using the Azure SDK
			DocumentSentiment result = await _client.AnalyzeSentimentAsync(request.Text);

			// Map results to a strongly-typed response model
			return new SentimentAnalysisResponse
			{
				Sentiment = result.Sentiment.ToString(),
				PositiveScore = result.ConfidenceScores.Positive,
				NeutralScore = result.ConfidenceScores.Neutral,
				NegativeScore = result.ConfidenceScores.Negative
			};
		}

		/// <summary>
		/// Extracts named entities (such as people, locations, organizations, and dates)
		/// from the given text using Azure AI Language SDK.
		/// </summary>
		/// <param name="request">Request object containing the text to analyze.</param>
		/// <returns>
		/// A <see cref="EntityExtractionResponse"/> containing recognized entities with category and confidence.
		/// </returns>
		public async Task<EntityExtractionResponse> ExtractEntitiesAsync(EntityExtractionRequest request)
		{
			// Validate input text.
			if (string.IsNullOrWhiteSpace(request.Text))
				throw new ArgumentException("Text cannot be null or empty.", nameof(request.Text));

			// Call the Azure AI SDK to recognize named entities.
			Response<CategorizedEntityCollection> response = await _client.RecognizeEntitiesAsync(request.Text);

			// Map results to a strongly-typed model.
			var entities = response.Value.Select(e => new NamedEntity
			{
				Text = e.Text,
				Category = e.Category.ToString(),
				Subcategory = e.SubCategory,
				Confidence = e.ConfidenceScore
			}).ToList();

			return new EntityExtractionResponse
			{
				Entities = entities
			};
		}

		/// <summary>
		/// Summarizes the given text using Azure AI Language's Extractive Summarization.
		/// Extracts the most relevant sentences that best represent the content.
		/// </summary>
		/// <param name="request">The text to summarize.</param>
		/// <returns>
		/// A <see cref="TextSummarizationResponse"/> containing the summarized content.
		/// </returns>
		public async Task<TextSummarizationResponse> SummarizeTextAsync(TextSummarizationRequest request)
		{
			// Validate input
			if (string.IsNullOrWhiteSpace(request.Text))
				throw new ArgumentException("Text cannot be null or empty.", nameof(request.Text));

			// Create input document collection (the SDK works with multiple documents)
			var documents = new List<string> { request.Text };

			// Ensure the following code is updated to include the correct namespace for 'ExtractSummaryOptions'
			var options = new ExtractSummaryOptions()
			{
				MaxSentenceCount = 2
			};

			// Send the request to Azure
			AnalyzeActionsOperation operation = await _client.StartAnalyzeActionsAsync(
				documents,
				new TextAnalyticsActions
				{
					ExtractSummaryActions = new List<ExtractSummaryAction> { new ExtractSummaryAction { MaxSentenceCount = options.MaxSentenceCount } }
				}
			);

			// Await the completion of the asynchronous operation
			await operation.WaitForCompletionAsync();

			// Retrieve and aggregate the summary results
			var summaryBuilder = new System.Text.StringBuilder();
			int sentenceCount = 0;

			await foreach (AnalyzeActionsResult documentsInPage in operation.Value)
			{
				foreach (ExtractSummaryActionResult summaryActionResult in documentsInPage.ExtractSummaryResults)
				{
					foreach (ExtractSummaryResult documentResults in summaryActionResult.DocumentsResults)
					{
						foreach (SummarySentence sentence in documentResults.Sentences)
						{
							summaryBuilder.AppendLine(sentence.Text.Trim());
							sentenceCount++;
						}
					}
				}
			}

			// Return structured summary response
			return new TextSummarizationResponse
			{
				Summary = summaryBuilder.ToString().Trim(),
				SentenceCount = sentenceCount
			};
		}

		/// <summary>
		/// Translates text from its detected source language to a specified target language
		/// using Azure Translator Text API.
		/// </summary>
		/// <param name="request">The request object containing input text and target language.</param>
		/// <returns>
		/// A <see cref="TranslationResponse"/> with detected source language and translated output.
		/// </returns>
		public async Task<TranslationResponse> TranslateTextAsync(TranslationRequest request)
		{
			// Validate the input text
			if (string.IsNullOrWhiteSpace(request.Text))
				throw new ArgumentException("Text cannot be empty.", nameof(request.Text));

			if (string.IsNullOrWhiteSpace(request.TargetLanguage))
				throw new ArgumentException("Target language cannot be empty.", nameof(request.TargetLanguage));

			// Retrieve configuration settings
			var endpoint = _config["AzureAI:TranslatorEndpoint"];
			var apiKey = _config["AzureAI:TranslatorApiKey"];
			var region = _config["AzureAI:TranslatorRegion"]; // Required in global Azure Translator

			// Construct the request URL
			var url = $"{endpoint}/translate?api-version=3.0&to={request.TargetLanguage}";

			// Create request payload
			var payload = new[]
			{
				new { Text = request.Text }
			};

			// Configure HTTP client
			using var http = new HttpClient();
			http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
			http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);

			// Send the request
			var response = await http.PostAsJsonAsync(url, payload);
			response.EnsureSuccessStatusCode();

			// Parse the JSON response
			var json = await response.Content.ReadFromJsonAsync<JsonElement>();

			// Extract relevant data from Azure Translator's response
			var firstTranslation = json[0].GetProperty("translations")[0];
			var detectedLanguage = json[0].GetProperty("detectedLanguage").GetProperty("language").GetString();
			var translatedText = firstTranslation.GetProperty("text").GetString();
			var targetLanguage = firstTranslation.GetProperty("to").GetString();

			return new TranslationResponse
			{
				DetectedLanguage = detectedLanguage ?? string.Empty,
				TranslatedText = translatedText ?? string.Empty,
				TargetLanguage = targetLanguage ?? request.TargetLanguage
			};
		}


	}
}
