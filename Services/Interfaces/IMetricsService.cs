using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IMetricsService
	{
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
		Task<MetricsResponse> AnalyzePromptAsync(string prompt);
	}
}
