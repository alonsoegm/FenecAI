using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Tags("Document Intelligence - Read (OCR)")]
	[Produces("application/json")]
	public class DocumentController : ControllerBase
	{
		private readonly IDocumentService _documentService;

		public DocumentController(IDocumentService documentService)
		{
			_documentService = documentService;
		}

		/// <summary>
		/// Extracts text from a PDF or image using Azure AI Document Intelligence Read model.
		/// </summary>
		/// <param name="file">PDF or image file.</param>
		/// <returns>Extracted text by page and full content.</returns>
		[HttpPost("read")]
		[SwaggerOperation(
			Summary = "Extract text from a document (OCR).",
			Description = "Uploads a PDF or image and uses the prebuilt-read model to extract text using Azure AI Document Intelligence."
		)]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(DocumentReadResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> ReadDocument([FromForm] DocumentReadRequest request)
		{
			if (request.PdfFile == null || request.PdfFile.Length == 0)
				return BadRequest("Please upload a valid document.");

			var result = await _documentService.ReadDocumentAsync(request.PdfFile);
			return Ok(result);
		}

		/// <summary>
		/// Extracts structured data (vendor, total, date, etc.) from an invoice document.
		/// </summary>
		/// <param name="file">Invoice file in PDF or image format.</param>
		/// <returns>Structured invoice fields with confidence scores.</returns>
		[HttpPost("invoice")]
		[SwaggerOperation(
			Summary = "Analyze an invoice document.",
			Description = "Uploads a PDF or image of an invoice and extracts structured fields using Azure Document Intelligence prebuilt-invoice model."
		)]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(InvoiceAnalysisResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> AnalyzeInvoice([FromForm] DocumentReadRequest request)
		{
			if (request.PdfFile == null || request.PdfFile.Length == 0)
				return BadRequest("Please upload a valid document.");

			var result = await _documentService.AnalyzeInvoiceAsync(request.PdfFile);
			return Ok(result);
		}
	}
}
