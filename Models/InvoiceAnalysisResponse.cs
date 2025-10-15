namespace FenecAI.API.Models
{
	public class InvoiceAnalysisResponse
	{
		public string VendorName { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string InvoiceId { get; set; } = string.Empty;
		public string InvoiceDate { get; set; } = string.Empty;
		public string DueDate { get; set; } = string.Empty;
		public double Subtotal { get; set; }
		public double TotalTax { get; set; }
		public double TotalAmount { get; set; }
		public double Confidence { get; set; }
		public Dictionary<string, string> RawFields { get; set; } = new();
	}
}
