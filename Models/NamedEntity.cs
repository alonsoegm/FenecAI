namespace FenecAI.API.Models
{
	/// <summary>
	/// Represents a single named entity detected in text.
	/// </summary>
	public class NamedEntity
	{
		/// <summary>
		/// The text value of the entity.
		/// </summary>
		public string Text { get; set; } = string.Empty;

		/// <summary>
		/// The category or type of entity (e.g., Person, Organization, Location, DateTime).
		/// </summary>
		public string Category { get; set; } = string.Empty;

		/// <summary>
		/// The confidence score of the detected entity.
		/// </summary>
		public double Confidence { get; set; }

		/// <summary>
		/// An optional subcategory providing more detail (e.g., Email, URL, Product).
		/// </summary>
		public string? Subcategory { get; set; }
	}

	/// <summary>
	/// Response model containing all entities extracted from text.
	/// </summary>
	public class EntityExtractionResponse
	{
		/// <summary>
		/// List of recognized named entities.
		/// </summary>
		public List<NamedEntity> Entities { get; set; } = new();
	}
}
