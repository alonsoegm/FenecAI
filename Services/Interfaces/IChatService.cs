using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IChatService
	{
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
		Task<ChatResponse> GenerateChatCompletionAsync(ChatRequest request);

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
		IAsyncEnumerable<string> StreamChatAsync(ChatRequest request);
	}
}
