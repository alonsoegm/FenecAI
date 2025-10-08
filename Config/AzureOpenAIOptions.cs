namespace FenecAI.API.Config
{
	// Strongly-typed configuration class for Azure OpenAI settings
	public class AzureOpenAIOptions
	{
		
			public string Endpoint { get; set; } = string.Empty;
			public string ApiKey { get; set; } = string.Empty;
			public string Deployment { get; set; } = "gpt-4.1"; // Default deployment name
	}
}
