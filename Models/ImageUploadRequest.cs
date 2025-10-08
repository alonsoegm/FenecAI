using System.ComponentModel.DataAnnotations;

public class ImageUploadRequest
{
	[Required]
	public IFormFile Image { get; set; } = null!;
}
