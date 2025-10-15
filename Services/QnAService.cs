using Azure;
using Azure.AI.Language.QuestionAnswering;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Service that interacts with Azure Language Question Answering.
	/// </summary>
	public class QnAService : IQnAService
	{
		private readonly IConfiguration _config;

		public QnAService(IConfiguration config)
		{
			_config = config;
		}

		public async Task<QnAResponse> GetAnswerAsync(QnARequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Question))
				throw new ArgumentException("Question cannot be empty.");

			var endpoint = new Uri(_config["AzureAI:LanguageEndpoint"]!);
			var key = new AzureKeyCredential(_config["AzureAI:LanguageApiKey"]!);
			var projectName = _config["AzureAI:QnAProjectName"];
			var deploymentName = _config["AzureAI:QnADeploymentName"];

			var client = new QuestionAnsweringClient(endpoint, key);

			Response<AnswersResult> response = await client.GetAnswersAsync(
				request.Question,
				new QuestionAnsweringProject(projectName, deploymentName)
			);

			var answers = response.Value.Answers;
			var top = answers.FirstOrDefault();

			return new QnAResponse
			{
				Answer = top?.Answer ?? "I'm sorry, I don’t have an answer for that yet.",
				Confidence = top?.Confidence ?? 0.0,
				AlternateAnswers = answers
					.Skip(1)
					.Select(a => a.Answer ?? string.Empty)
					.Where(a => !string.IsNullOrWhiteSpace(a))
					.ToList()
			};
		}
	}
}
