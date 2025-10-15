using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IStorageService
	{
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
		Task<FileUploadResponse> UploadFileAsync(IFormFile file);
	}
}
