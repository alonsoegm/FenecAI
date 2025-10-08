using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Models;
using FenecAI.API.Services;

namespace FenecAI.API.Controllers
{
	/// <summary>
	/// Provides endpoints for managing file uploads to Azure Blob Storage.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This controller allows users to upload one or more files directly to Azure Blob Storage.
	/// The uploaded files can later be used in RAG (Retrieval-Augmented Generation) ingestion or other AI processes.
	/// </para>
	/// </remarks>
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Infrastructure - File Storage")]
	[Produces("application/json")]
	public class FileController : ControllerBase
	{
		private readonly StorageService _storageService;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileController"/> class.
		/// </summary>
		/// <param name="storageService">Service responsible for handling Azure Blob Storage operations.</param>
		public FileController(StorageService storageService)
		{
			_storageService = storageService;
		}

		/// <summary>
		/// Uploads one or more files to Azure Blob Storage.
		/// </summary>
		/// <param name="files">
		/// The list of files to upload.  
		/// Files must be provided as <b>multipart/form-data</b>.
		/// </param>
		/// <returns>
		/// A list of <see cref="FileUploadResponse"/> objects, each containing:
		/// - <b>FileName:</b> original file name  
		/// - <b>BlobUri:</b> Azure Blob public URI  
		/// - <b>Message:</b> upload confirmation message  
		/// </returns>
		/// <remarks>
		/// <para>
		/// Maximum upload size: <b>50 MB</b> per request.
		/// Supported file types include text files (.txt, .pdf, .csv) commonly used for RAG ingestion.
		/// </para>
		/// <para>
		/// Example (via Swagger or Postman):
		/// <code>
		/// POST /api/File/upload
		/// Content-Type: multipart/form-data
		/// files: [select one or more files]
		/// </code>
		/// </para>
		/// </remarks>
		[HttpPost("upload")]
		[SwaggerOperation(
			Summary = "Upload files to Azure Blob Storage",
			Description = "Uploads one or more files (up to 50 MB total) into the configured Azure Blob Storage container."
		)]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(List<FileUploadResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
		[RequestSizeLimit(50 * 1024 * 1024)] // 50 MB max upload
		public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files)
		{
			if (files == null || files.Count == 0)
			{
				return BadRequest(new
				{
					message = "No files were provided.",
					example = "Select one or more .txt or .pdf files to upload."
				});
			}

			var results = new List<FileUploadResponse>();

			foreach (var file in files)
			{
				try
				{
					var result = await _storageService.UploadFileAsync(file);
					results.Add(result);
				}
				catch (Exception ex)
				{
					results.Add(new FileUploadResponse
					{
						FileName = file.FileName,
						Message = $"❌ Upload failed: {ex.Message}"
					});
				}
			}

			return Ok(new
			{
				success = true,
				uploadedFiles = results
			});
		}
	}
}
