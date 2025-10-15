using Azure.Storage.Blobs;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Provides methods for managing file uploads to Azure Blob Storage.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This service handles blob container initialization and file uploads to Azure Storage.
	/// It is primarily used in the document ingestion stage of the RAG pipeline.
	/// </para>
	/// <para>
	/// Typical use case:
	/// - Upload training documents or text files to a designated container.
	/// - Later, these files are processed by the <see cref="RAGService"/> for embedding and indexing.
	/// </para>
	/// </remarks>
	public class StorageService: IStorageService
	{
		private readonly BlobContainerClient _containerClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="StorageService"/> class and ensures
		/// the target Blob container exists.
		/// </summary>
		/// <param name="config">Application configuration containing Azure Storage connection string and container name.</param>
		/// <remarks>
		/// If the specified container does not exist, it will be created automatically.
		/// </remarks>
		public StorageService(IConfiguration config)
		{
			var connectionString = config["AzureStorage:ConnectionString"];
			var containerName = config["AzureStorage:ContainerName"];

			_containerClient = new BlobContainerClient(connectionString, containerName);

			// Ensure the blob container exists.
			_containerClient.CreateIfNotExists();
		}

		/// <summary>
		/// Uploads a single file to the configured Azure Blob Storage container.
		/// </summary>
		/// <param name="file">The file uploaded from the client request.</param>
		/// <returns>
		/// A <see cref="FileUploadResponse"/> containing:
		/// <list type="bullet">
		/// <item><description><b>FileName</b> — the original name of the uploaded file.</description></item>
		/// <item><description><b>BlobUri</b> — the publicly accessible URI of the uploaded blob.</description></item>
		/// <item><description><b>Message</b> — a confirmation message indicating upload success.</description></item>
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>
		/// Files are stored in the container specified by <c>AzureStorage:ContainerName</c> in <b>appsettings.json</b>.
		/// Existing blobs with the same name will be overwritten.
		/// </para>
		/// <para>
		/// Interpretation tip:
		/// - Use the <b>BlobUri</b> to verify upload success or to pass the document path to the RAG ingestion process.
		/// </para>
		/// </remarks>
		/// <exception cref="Exception">Thrown if the upload fails due to invalid configuration or connectivity issues.</exception>
		public async Task<FileUploadResponse> UploadFileAsync(IFormFile file)
		{
			var blobClient = _containerClient.GetBlobClient(file.FileName);

			await using var stream = file.OpenReadStream();
			await blobClient.UploadAsync(stream, overwrite: true);

			return new FileUploadResponse
			{
				FileName = file.FileName,
				BlobUri = blobClient.Uri.ToString(),
				Message = "File uploaded successfully."
			};
		}
	}
}
