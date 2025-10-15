using Azure;
using Azure.AI.Language.Conversations;
using Azure.Core;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Service that connects to Azure Conversational Language Understanding (CLU)
	/// and returns predicted intent and entities for a given text.
	/// </summary>
	public class ConversationService : IConversationService
	{
		private readonly IConfiguration _config;

		public ConversationService(IConfiguration config)
		{
			_config = config;
		}

		public async Task<ConversationalAnalysisResponse> AnalyzeConversationAsync(ConversationalAnalysisRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Text))
				throw new ArgumentException("Text cannot be empty.");

			var endpoint = new Uri(_config["AzureAI:LanguageEndpoint"]!);
			var key = new AzureKeyCredential(_config["AzureAI:LanguageApiKey"]!);

			var client = new ConversationAnalysisClient(endpoint, key);

			var projectName = _config["AzureAI:CLUProjectName"];
			var deploymentName = _config["AzureAI:CLUDeploymentName"];

			// Prepare the input payload
			var input = new
			{
				kind = "Conversation",
				analysisInput = new
				{
					conversationItem = new
					{
						id = "1",
						text = request.Text,
						language = "en",
						modality = "text",
						participantId = "1"
					},
					isLoggingEnabled = false
				},
				parameters = new
				{
					projectName,
					deploymentName
				}
			};

			// Call CLU
			Response response = await client.AnalyzeConversationAsync(RequestContent.Create(input));

			// Parse the result
			var result = response.Content.ToDynamicFromJson();

			var prediction = result?.result?.prediction;

			// Get top intent
			string topIntent = prediction?.topIntent ?? string.Empty;

			// Initialize confidence
			double confidence = 0.0;
			var intents = new Dictionary<string, double>();

			// Parse intents (now an array)
			foreach (var intent in prediction?.intents ?? new dynamic[] { })
			{
				string category = intent.category;
				double score = intent.confidenceScore;
				intents[category] = score;

				// Capture top intent confidence
				if (category == topIntent)
					confidence = score;
			}

			// Parse entities (also array)
			var entities = new Dictionary<string, string>();
			foreach (var entity in prediction?.entities ?? new dynamic[] { })
			{
				string category = entity.category;
				string text = entity.text;
				entities[category] = text;
			}

			return new ConversationalAnalysisResponse
			{
				TopIntent = topIntent,
				Confidence = confidence,
				Intents = intents,
				Entities = entities
			};

		}
	}
}
