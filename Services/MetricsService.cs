using Azure;
using Azure.AI.OpenAI;
using FenecAI.API.Models;
using OpenAI.Chat;
using System.Diagnostics;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Provides analytics for monitoring token usage, latency, and estimated cost of Azure OpenAI model calls.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This service enables cost-awareness and performance optimization by tracking:
	/// - Input, output, and total token counts
	/// - Estimated monetary cost per request
	/// - Response time in milliseconds
	/// </para>
	/// <para>
	/// These metrics help developers analyze how efficiently prompts are structured and how model parameters (e.g., temperature or max tokens)
	/// affect performance and cost. It aligns with the <b>“Monitor and optimize performance and cost”</b> sub-objective of the AI-102 certification.
	/// </para>
	/// </remarks>
	public class MetricsService
	{
		private readonly AzureOpenAIClient _openAIClient;
		private readonly IConfiguration _config;
		private readonly ILogger<MetricsService> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="MetricsService"/> class.
		/// </summary>
		/// <param name="config">Application configuration containing Azure OpenAI credentials and model names.</param>
		/// <param name="logger">Logger for tracking prompt metrics and diagnostics.</param>
		public MetricsService(IConfiguration config, ILogger<MetricsService> logger)
		{
			_config = config;
			_logger = logger;

			_openAIClient = new AzureOpenAIClient(
				new Uri(_config["AzureOpenAI:Endpoint"]),
				new AzureKeyCredential(_config["AzureOpenAI:ApiKey"]));
		}

		/// <summary>
		/// Executes a single prompt request and returns detailed metrics such as token usage, latency, and estimated cost.
		/// </summary>
		/// <param name="prompt">The text prompt to be analyzed.</param>
		/// <returns>
		/// A <see cref="MetricsResponse"/> object containing:
		/// <list type="bullet">
		/// <item><description><b>PromptTokens</b> — number of tokens in the input prompt.</description></item>
		/// <item><description><b>CompletionTokens</b> — number of tokens in the model’s output.</description></item>
		/// <item><description><b>TotalTokens</b> — total of input and output tokens.</description></item>
		/// <item><description><b>EstimatedCostUsd</b> — approximate cost in USD for this request.</description></item>
		/// <item><description><b>ResponseTimeMs</b> — total processing time for the API call.</description></item>
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>
		/// Interpretation tips:
		/// - <b>PromptTokens</b> typically scale with input complexity.  
		/// - <b>CompletionTokens</b> increase with longer or more creative responses.  
		/// - <b>EstimatedCostUsd</b> helps identify expensive prompts that could be optimized.  
		/// </para>
		/// </remarks>
		public async Task<MetricsResponse> AnalyzePromptAsync(string prompt)
		{
			// Select deployment name from configuration, fallback to GPT-4.
			var modelName = _config["AzureOpenAI:Deployment"] ?? "gpt-4";
			var chatClient = _openAIClient.GetChatClient(modelName);

			// Define a basic conversation for measurement.
			var messages = new List<ChatMessage>
			{
				new SystemChatMessage("You are a helpful assistant that analyzes prompt performance."),
				new UserChatMessage(prompt)
			};

			// Start measuring execution time.
			var stopwatch = Stopwatch.StartNew();
			var response = await chatClient.CompleteChatAsync(messages);
			stopwatch.Stop();

			// Extract usage statistics.
			var usage = response.Value.Usage;
			int promptTokens = usage.InputTokenCount;
			int completionTokens = usage.OutputTokenCount;
			int totalTokens = usage.TotalTokenCount;

			// Compute estimated cost.
			double cost = EstimateCost(modelName, promptTokens, completionTokens);

			var metrics = new MetricsResponse
			{
				Model = modelName,
				PromptTokens = promptTokens,
				CompletionTokens = completionTokens,
				TotalTokens = totalTokens,
				EstimatedCostUsd = cost,
				ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds,
				Message = "Metrics analyzed successfully."
			};

			_logger.LogInformation("Prompt: {Prompt} | Tokens: {Total} | Cost: ${Cost:F6}",
				prompt[..Math.Min(80, prompt.Length)], totalTokens, cost);

			return metrics;
		}

		/// <summary>
		/// Provides a rough cost estimation based on OpenAI token pricing (as of 2024).
		/// </summary>
		/// <param name="model">The model name (e.g., gpt-4, gpt-35-turbo).</param>
		/// <param name="promptTokens">Number of tokens used for input.</param>
		/// <param name="completionTokens">Number of tokens used for output.</param>
		/// <returns>
		/// A double value representing the estimated cost in USD for this request.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Current estimated pricing (per 1,000 tokens):
		/// - GPT-4: $0.01 for input / $0.03 for output  
		/// - GPT-3.5: $0.001 for input / $0.002 for output  
		/// </para>
		/// <para>
		/// Example:  
		/// 500 input tokens and 100 output tokens using GPT-4 ≈ $0.01 * 0.5 + $0.03 * 0.1 = $0.008.
		/// </para>
		/// </remarks>
		private double EstimateCost(string model, int promptTokens, int completionTokens)
		{
			// Default prices per 1,000 tokens (USD)
			double promptCost = 0.0;
			double completionCost = 0.0;

			if (model.Contains("gpt-4", StringComparison.OrdinalIgnoreCase))
			{
				promptCost = 0.01;      // $0.01 per 1k input tokens
				completionCost = 0.03;  // $0.03 per 1k output tokens
			}
			else if (model.Contains("gpt-35", StringComparison.OrdinalIgnoreCase))
			{
				promptCost = 0.001;     // $0.001 per 1k input tokens
				completionCost = 0.002; // $0.002 per 1k output tokens
			}

			double totalCost =
				(promptTokens / 1000.0 * promptCost) +
				(completionTokens / 1000.0 * completionCost);

			return Math.Round(totalCost, 6);
		}
	}
}
