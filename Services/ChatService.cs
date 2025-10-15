using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using FenecAI.API.Config;
using FenecAI.API.Models;
using Microsoft.Extensions.Options;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Provides interaction logic with the Azure OpenAI Chat model.
	/// Handles both standard and streaming completions.
	/// </summary>
	public class ChatService : IChatService
	{
		// Holds the configuration settings for the Azure OpenAI resource.
		private readonly AzureOpenAIOptions _options;

		/// <summary>
		/// Initializes a new instance of <see cref="ChatService"/> with injected configuration options.
		/// </summary>
		/// <param name="options">Azure OpenAI configuration settings loaded from appsettings.json.</param>
		public ChatService(IOptions<AzureOpenAIOptions> options)
		{
			_options = options.Value;
		}

		/// <summary>
		/// Sends a prompt to the Azure OpenAI chat model and retrieves the full generated response.
		/// </summary>
		/// <param name="request">User request containing the prompt and generation parameters.</param>
		/// <returns>
		/// A <see cref="ChatResponse"/> object containing the model output, token usage, and model name.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The method initializes an <see cref="AzureOpenAIClient"/>, builds the conversation messages,
		/// applies temperature, top-p, and token limits, and asynchronously requests a completion.
		/// </para>
		/// <para>
		/// Interpretation tip: 
		/// - <b>Temperature</b> controls randomness (higher = more creative).  
		/// - <b>TopP</b> filters token probability (higher = more diverse output).  
		/// - <b>MaxTokens</b> limits the response length.
		/// </para>
		/// </remarks>
		public async Task<ChatResponse> GenerateChatCompletionAsync(ChatRequest request)
		{
			// Initialize Azure OpenAI client with endpoint and API key.
			var client = new AzureOpenAIClient(
				new Uri(_options.Endpoint),
				new AzureKeyCredential(_options.ApiKey));

			// Retrieve the specific chat model deployment from Azure.
			var chatClient = client.GetChatClient(_options.Deployment);

			// Define the chat conversation context.
			var messages = new List<ChatMessage>
			{
				new SystemChatMessage("You are a helpful personal trainer using David Goggins’ motivational style."),
				new UserChatMessage(request.Prompt)
			};

			// Configure generation behavior.
			var options = new ChatCompletionOptions
			{
				Temperature = request.Temperature,
				TopP = request.TopP,
				MaxOutputTokenCount = request.MaxTokens
			};

			// Execute the chat completion request asynchronously.
			var response = await chatClient.CompleteChatAsync(messages, options);

			// Build and return the structured result.
			return new ChatResponse
			{
				Output = response.Value.Content[0].Text,
				TokensUsed = response.Value.Usage.OutputTokenDetails.ReasoningTokenCount,
				ModelName = _options.Deployment
			};
		}

		/// <summary>
		/// Streams the chat response in real-time, token by token, as the model generates it.
		/// </summary>
		/// <param name="request">User request containing the prompt and generation parameters.</param>
		/// <returns>
		/// An asynchronous stream (<see cref="IAsyncEnumerable{T}"/>) yielding text fragments as they are produced.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This method is useful for low-latency experiences such as chat UIs or live feedback systems.  
		/// The client can process the text progressively instead of waiting for the entire response.
		/// </para>
		/// </remarks>
		public async IAsyncEnumerable<string> StreamChatAsync(ChatRequest request)
		{
			// Initialize Azure OpenAI client with endpoint and API key.
			var client = new AzureOpenAIClient(
				new Uri(_options.Endpoint),
				new AzureKeyCredential(_options.ApiKey));

			var chatClient = client.GetChatClient(_options.Deployment);

			// Define the conversation context using a different assistant persona.
			var messages = new List<ChatMessage>
			{
				new SystemChatMessage("You are a helpful personal trainer using Ronnie Coleman’s high-energy coaching style."),
				new UserChatMessage(request.Prompt)
			};

			// Configure generation parameters.
			var options = new ChatCompletionOptions
			{
				Temperature = request.Temperature,
				TopP = request.TopP,
				MaxOutputTokenCount = request.MaxTokens
			};

			// Stream tokens progressively from the model.
			await foreach (var update in chatClient.CompleteChatStreamingAsync(messages, options))
			{
				// Each update may contain text fragments or metadata.
				foreach (var contentPart in update.ContentUpdate)
				{
					if (!string.IsNullOrEmpty(contentPart.Text))
						yield return contentPart.Text;
				}
			}
		}
	}
}
