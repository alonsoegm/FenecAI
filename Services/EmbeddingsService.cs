using Azure;
using Azure.AI.OpenAI;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Services
{
	/// <summary>  
	/// Provides methods for generating embeddings and calculating semantic similarity between text inputs.  
	/// </summary>  
	/// <remarks>  
	/// <para>  
	/// Embeddings convert text into high-dimensional numeric vectors that capture semantic meaning.  
	/// Similar texts will have embedding vectors that are close together in vector space,  
	/// enabling similarity search, clustering, and semantic reasoning.  
	/// </para>  
	/// </remarks>  
	public class EmbeddingsService : IEmbeddingsService
	{
		private readonly AzureOpenAIClient _openAIClient;
		private readonly IConfiguration _config;
		private readonly ILogger<EmbeddingsService> _logger;

		/// <summary>  
		/// Initializes a new instance of the <see cref="EmbeddingsService"/> class.  
		/// </summary>  
		/// <param name="config">Application configuration containing Azure OpenAI credentials.</param>  
		/// <param name="logger">Logger for recording embedding generation and similarity calculations.</param>  
		public EmbeddingsService(IConfiguration config, ILogger<EmbeddingsService> logger)
		{
			_config = config;
			_logger = logger;

			_openAIClient = new AzureOpenAIClient(
				new Uri(_config["AzureOpenAI:Endpoint"]),
				new AzureKeyCredential(_config["AzureOpenAI:ApiKey"]));
		}

		/// <summary>  
		/// Generates an embedding vector for a given text using the Azure OpenAI deployment specified in configuration.  
		/// </summary>  
		/// <param name="text">The text input for which to generate an embedding vector.</param>  
		/// <returns>  
		/// A single <see cref="float"/> array representing the text embedding.  
		/// Each element corresponds to a dimension in the semantic vector space.  
		/// </returns>  
		/// <remarks>  
		/// <para>  
		/// This method uses the model defined in <c>AzureOpenAI:EmbeddingDeployment</c>,  
		/// typically <b>text-embedding-3-large</b>, which produces 3,072-dimensional vectors.  
		/// </para>  
		/// <para>  
		/// Interpretation tip:  
		/// - Each vector encodes meaning rather than syntax.  
		/// - Vectors from semantically similar texts will have high cosine similarity.  
		/// </para>  
		/// </remarks>  
		/// <exception cref="Exception">Thrown when the Azure OpenAI service call fails.</exception>  
		public async Task<float[]> GenerateEmbeddingAsync(string text)
		{
			try
			{
				// Retrieve the embedding client for the configured deployment.  
				var embeddingClient = _openAIClient.GetEmbeddingClient(_config["AzureOpenAI:EmbeddingDeployment"]);

				// Request embedding generation for the input text.  
				var result = await embeddingClient.GenerateEmbeddingsAsync(new List<string> { text });

				// Convert the embedding result to a float array.  
				var vector = result.Value[0].ToFloats().ToArray();

				_logger.LogInformation("Generated embedding with {Length} dimensions.", vector.Length);
				return vector;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating embedding for text input.");
				throw;
			}
		}

		/// <summary>  
		/// Calculates the cosine similarity between two embedding vectors.  
		/// </summary>  
		/// <param name="v1">The first embedding vector.</param>  
		/// <param name="v2">The second embedding vector.</param>  
		/// <returns>  
		/// A double value between <b>-1</b> and <b>1</b> representing similarity:  
		/// <list type="bullet">  
		/// <item><description><b>1.0</b> → identical direction (very similar meaning)</description></item>  
		/// <item><description><b>0.0</b> → orthogonal (no relation)</description></item>  
		/// <item><description><b>-1.0</b> → opposite meaning (rare in embeddings)</description></item>  
		/// </list>  
		/// </returns>  
		/// <remarks>  
		/// Cosine similarity measures the angle between vectors rather than their magnitude,  
		/// making it ideal for comparing semantic representations of text.  
		/// </remarks>  
		public double CosineSimilarity(float[] v1, float[] v2)
		{
			double dot = 0.0, mag1 = 0.0, mag2 = 0.0;

			for (int i = 0; i < v1.Length; i++)
			{
				dot += v1[i] * v2[i];
				mag1 += v1[i] * v1[i];
				mag2 += v2[i] * v2[i];
			}

			if (mag1 == 0 || mag2 == 0)
				return 0.0;

			return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
		}
	}
}
