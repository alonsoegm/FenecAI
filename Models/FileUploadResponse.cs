namespace FenecAI.API.Models
{
	public class FileUploadResponse
	{
		public string FileName { get; set; } = string.Empty;
		public string BlobUri { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
	}
}
