namespace FenecAI.API.Models
{
	public class FaceResultDto
	{
		public string FaceId { get; set; } = string.Empty;
		public double? Age { get; set; }
		public string? Gender { get; set; }
		public double? Smile { get; set; }
		public string? Emotion { get; set; } // simplified summary
		public bool? Glasses { get; set; }
		public string? Hair { get; set; }
		public BoundingBoxDto? BoundingBox { get; set; }
	}

	public class BoundingBoxDto
	{
		public double Left { get; set; }
		public double Top { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
	}
}
