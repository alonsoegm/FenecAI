using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IDocumentService
	{
		/// <summary>
		/// Extracts raw text from a PDF or image using Azure Document Intelligence (Read model).
		/// </summary>
		Task<DocumentReadResponse> ReadDocumentAsync(IFormFile file);

		/// <summary>
		/// Extracts and analyzes key fields from an invoice document using Azure Document Intelligence (Invoice model).
		/// </summary>
		Task<InvoiceAnalysisResponse> AnalyzeInvoiceAsync(IFormFile file);
	}
}
