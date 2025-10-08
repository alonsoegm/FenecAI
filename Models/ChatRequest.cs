namespace FenecAI.API.Models
{

	//-- Temperature: Controls randomness in output. Lower values (0.2) make output more focused and deterministic,
	//while higher values (0.8) increase creativity and variability.
	//--TopP: Implements nucleus sampling. It considers the smallest set of tokens whose cumulative probability
	//exceeds the TopP value. For example, a TopP of 0.9 means only the top 90% probable tokens are considered, enhancing output diversity.
	//--MaxTokens: Sets the maximum length of the generated response in tokens. Higher values allow for longer,
	//more detailed outputs, while lower values keep responses concise.

	public class ChatRequest
	{
		public string Prompt { get; set; } = string.Empty; // User input prompt

		// Optional parameters to customize model behavior
		public float Temperature { get; set; } = 0.7f; // Randomness / creativity in output ==> Lower = more focused, Higher = more diverse
		public float TopP { get; set; } = 1.0f; // Nucleus sampling threshold ==> Lower = more focused, Higher = more diverse
		public int MaxTokens { get; set; } = 800; // Max response length in tokens ==> Higher = longer outputs, Lower = shorter outputs
	}
}

