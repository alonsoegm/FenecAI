using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using FenecAI.API.Models;
using FenecAI.API.Services.Interfaces;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Handles document analysis operations using Azure Document Intelligence.
	/// </summary>
	public class DocumentService : IDocumentService
	{
		private readonly IConfiguration _config;

		public DocumentService(IConfiguration config)
		{
			_config = config;
		}

		/// <summary>
		/// Extracts raw text from a document using the "prebuilt-read" model.
		/// </summary>
		public async Task<DocumentReadResponse> ReadDocumentAsync(IFormFile file)
		{
			if (file == null || file.Length == 0)
				throw new ArgumentException("Please upload a valid file.");

			var endpoint = new Uri(_config["AzureAI:DocumentEndpoint"]!);
			var key = new AzureKeyCredential(_config["AzureAI:DocumentApiKey"]!);

			var client = new DocumentAnalysisClient(endpoint, key);

			// Copy the file to a stream
			await using var stream = file.OpenReadStream();

			// 🧠 Use prebuilt-read model for OCR extraction
			AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(
				WaitUntil.Completed,
				"prebuilt-read",
				stream
			);

			var result = operation.Value;

			var response = new DocumentReadResponse();

			foreach (var page in result.Pages)
			{
				var pageText = string.Join(" ", page.Lines.Select(l => l.Content));
				response.Pages.Add(pageText);
			}

			response.FullText = string.Join("\n", response.Pages);
			return response;
		}

		/// <summary>
		/// Extracts structured invoice fields using Azure Document Intelligence "prebuilt-invoice" model.
		/// </summary>
		public async Task<InvoiceAnalysisResponse> AnalyzeInvoiceAsync(IFormFile file)
		{
			if (file == null || file.Length == 0)
				throw new ArgumentException("Please upload a valid invoice file.");

			var endpoint = new Uri(_config["AzureAI:DocumentEndpoint"]!);
			var key = new AzureKeyCredential(_config["AzureAI:DocumentApiKey"]!);
			var client = new DocumentAnalysisClient(endpoint, key);

			await using var stream = file.OpenReadStream();

			// 🧾 Analyze invoice using prebuilt model
			AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(
				WaitUntil.Completed,
				"prebuilt-invoice",
				stream
			);

			var result = operation.Value;
			var invoice = result.Documents.FirstOrDefault();

			if (invoice == null)
				throw new Exception("No invoice data was detected.");

			var response = new InvoiceAnalysisResponse();

			// Fix for CS0019: Replace the null-coalescing operator (??) with a ternary conditional operator to handle the nullable double case properly.

			double GetDouble(string fieldName)
			{
				return invoice.Fields.ContainsKey(fieldName) && invoice.Fields[fieldName].Value != null
					? invoice.Fields[fieldName].Value.AsDouble()
					: 0.0;
			}

			string GetString(string fieldName)
			{
				return invoice.Fields.ContainsKey(fieldName)
					? invoice.Fields[fieldName].Value.ToString() ?? string.Empty
					: string.Empty;
			}

			// 🧩 Map key invoice fields
			response.VendorName = GetString("VendorName");
			response.CustomerName = GetString("CustomerName");
			response.InvoiceId = GetString("InvoiceId");
			response.InvoiceDate = GetString("InvoiceDate");
			response.DueDate = GetString("DueDate");
			response.Subtotal = GetDouble("Subtotal");
			response.TotalTax = GetDouble("TotalTax");
			response.TotalAmount = GetDouble("TotalAmount");

			// Capture confidence from TotalAmount or top-level document confidence
			response.Confidence = invoice.Fields.ContainsKey("TotalAmount") && invoice.Fields["TotalAmount"].Confidence.HasValue
			  ? (double)invoice.Fields["TotalAmount"].Confidence.GetValueOrDefault()
			  : 0.0;


			// Save all fields into RawFields
			foreach (var field in invoice.Fields)
			{
				response.RawFields[field.Key] = field.Value.Content ?? "";
			}

			return response;
		}
	}
}
