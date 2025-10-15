namespace FenecAI.API.Services.Interfaces
{
	public interface IEmbeddingsService
	{
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
		Task<float[]> GenerateEmbeddingAsync(string text);

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
		double CosineSimilarity(float[] v1, float[] v2);
	}
}
