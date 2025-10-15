using FenecAI.API.Models;

namespace FenecAI.API.Services.Interfaces
{
	public interface IImageService
	{

		/// <summary>
		/// Generates an image using the Azure OpenAI DALL·E model based on the provided prompt and options.
		/// </summary>
		/// <param name="request">An <see cref="ImageRequest"/> object containing the text prompt and size configuration.</param>
		/// <returns>
		/// A tuple containing:
		/// <list type="bullet">
		/// <item><description><b>ImageUrl:</b> The direct URI of the generated image hosted by Azure OpenAI.</description></item>
		/// <item><description><b>RevisedPrompt:</b> The model-refined version of the original prompt (used for better control and safety).</description></item>
		/// </list>
		/// </returns>
		/// <remarks>
		/// <para>
		/// The DALL·E 3 model automatically refines user prompts to ensure alignment with Azure’s Responsible AI guidelines.
		/// </para>
		/// <para>
		/// Interpretation tip:
		/// - The <b>RevisedPrompt</b> represents how the model internally rephrased your input for generation.  
		/// - Use <b>GeneratedImageSize</b> to control resolution (e.g., 512x512 or 1024x1024).  
		/// - The <b>Quality</b> and <b>Style</b> options affect realism and artistic rendering.
		/// </para>
		/// </remarks>
		/// <exception cref="Exception">Thrown if image generation fails due to invalid configuration or service error.</exception>
		Task<(string ImageUrl, string RevisedPrompt)> GenerateImageAsync(ImageRequest request);


	}
}
