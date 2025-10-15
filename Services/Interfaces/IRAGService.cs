using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IRAGService
	{
		/// <summary>
		/// Reads all <c>.txt</c> files from Blob Storage, splits them into smaller chunks,
		/// generates embeddings, and indexes them in Azure AI Search for retrieval.
		/// </summary>
		/// <returns>
		/// The total number of text chunks processed and indexed.
		/// </returns>
		/// <remarks>
		/// <para>
		/// <b>Ingestion Workflow:</b><br/>
		/// 1. Read all text documents from the configured blob container.<br/>
		/// 2. Split content into smaller, semantically meaningful chunks.<br/>
		/// 3. Generate embeddings for each chunk using <b>text-embedding-3-large</b>.<br/>
		/// 4. Upload the chunk content and vector embeddings into the Azure AI Search index.
		/// </para>
		/// </remarks>
		Task<int> IngestDocumentsAsync();

		/// <summary>
		/// Executes a retrieval-augmented question-answering flow:
		/// retrieves semantically relevant documents from Azure AI Search and
		/// uses GPT to generate a grounded answer based on that context.
		/// </summary>
		/// <param name="question">The user’s natural language question.</param>
		/// <param name="topK">The number of top results to retrieve from the vector search (default = 5).</param>
		/// <returns>
		/// A <see cref="RAGQueryResponse"/> containing the grounded answer and the list of retrieved context sources.
		/// </returns>
		/// <remarks>
		/// <para>
		/// <b>Workflow:</b><br/>
		/// 1. Generate an embedding vector for the user’s question.<br/>
		/// 2. Perform a vector search on the indexed document embeddings.<br/>
		/// 3. Aggregate the most relevant text chunks as context.<br/>
		/// 4. Pass the context to GPT to produce a factual and contextualized answer.
		/// </para>
		/// <para>
		/// <b>Interpretation tip:</b> The <c>Sources</c> list contains the document chunks used to build the response.
		/// </para>
		/// </remarks>
		Task<RAGQueryResponse> QueryAsync(string question, int topK = 5);
	}
}
