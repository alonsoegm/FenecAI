using Azure;
using Azure.AI.OpenAI;
using FenecAI.API.Models;
using OpenAI.Images;

namespace FenecAI.API.Services
{
	/// <summary>
	/// Provides functionality for generating images using the Azure OpenAI DALL·E 3 model.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This service transforms text prompts into AI-generated images through the DALL·E model deployed in Azure OpenAI.
	/// It supports different image resolutions, styles, and qualities based on user input.
	/// </para>
	/// <para>
	/// Typical use cases include: creating marketing visuals, concept art, or synthetic datasets for computer vision tasks.
	/// </para>
	/// </remarks>
	public class ImageService
	{
		private readonly AzureOpenAIClient _openAIClient;
		private readonly IConfiguration _config;
		private readonly ILogger<ImageService> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageService"/> class.
		/// </summary>
		/// <param name="config">Application configuration containing Azure OpenAI credentials.</param>
		/// <param name="logger">Logger used for tracking and diagnostics.</param>
		public ImageService(IConfiguration config, ILogger<ImageService> logger)
		{
			_config = config;
			_logger = logger;

			_openAIClient = new AzureOpenAIClient(
				new Uri(_config["AzureOpenAI:Endpoint"]),
				new AzureKeyCredential(_config["AzureOpenAI:ApiKey"]));
		}

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
		public async Task<(string ImageUrl, string RevisedPrompt)> GenerateImageAsync(ImageRequest request)
		{
			try
			{
				// Retrieve the DALL·E image generation client.
				var imageClient = _openAIClient.GetImageClient(
					_config["AzureOpenAI:ImageDeployment"] ?? "dall-e-3");

				// Define image generation options.
				var options = new ImageGenerationOptions
				{
					EndUserId = Guid.NewGuid().ToString(),
					Size = ParseGeneratedImageSize(request.Size) ?? GeneratedImageSize.W1024xH1024,
					Quality = GeneratedImageQuality.Standard,
					Style = GeneratedImageStyle.Vivid
				};

				_logger.LogInformation("Generating image for prompt: {Prompt}", request.Prompt);

				// Execute the generation request.
				var result = await imageClient.GenerateImageAsync(request.Prompt, options);

				// Extract the generated image result.
				var image = result.Value; // Result contains a single GeneratedImage object.

				// Return the image URL and the model’s refined prompt.
				return (ImageUrl: image.ImageUri.ToString(), image.RevisedPrompt);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating image for prompt: {Prompt}", request.Prompt);
				throw;
			}
		}

		/// <summary>
		/// Converts a user-specified string size (e.g., "512x512") into a valid <see cref="GeneratedImageSize"/> enumeration.
		/// </summary>
		/// <param name="size">The string representation of the desired image size.</param>
		/// <returns>
		/// A corresponding <see cref="GeneratedImageSize"/> enum value if valid, or <c>null</c> if unrecognized.
		/// </returns>
		/// <remarks>
		/// If an invalid size is provided, the service defaults to <b>1024x1024</b> resolution.
		/// </remarks>
		private GeneratedImageSize? ParseGeneratedImageSize(string size)
		{
			return size switch
			{
				"256x256" => GeneratedImageSize.W256xH256,
				"512x512" => GeneratedImageSize.W512xH512,
				"1024x1024" => GeneratedImageSize.W1024xH1024,
				"1024x1792" => GeneratedImageSize.W1024xH1792,
				"1792x1024" => GeneratedImageSize.W1792xH1024,
				_ => null
			};
		}
	}
}
