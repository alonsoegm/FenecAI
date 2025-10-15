using Microsoft.OpenApi.Models;
using FenecAI.API.Config;
using FenecAI.API.Services;
using FenecAI.API.Models.Settings;
using FenecAI.API.Services.Vision;
using FenecAI.API.Services.Interfaces;

namespace FenecAI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Configuration
			builder.Services.Configure<AzureOpenAIOptions>(
				builder.Configuration.GetSection("AzureOpenAI"));
			builder.Services.Configure<AzureVisionSettings>(
				builder.Configuration.GetSection("AzureVision"));

			// Services

			builder.Services.AddScoped<IImageAnalysisService, ImageAnalysisService>();
			builder.Services.AddScoped<IMetricsService, MetricsService>();
			builder.Services.AddScoped<IEmbeddingsService, EmbeddingsService>();
			builder.Services.AddScoped<IContextSafetyService, ContextSafetyService>();
			builder.Services.AddScoped<IImageService, ImageService>();
			builder.Services.AddScoped<IRAGService, RAGService>();
			builder.Services.AddScoped<IStorageService, StorageService>();
			builder.Services.AddScoped<IChatService, ChatService>();
			builder.Services.AddScoped<ILanguageService, LanguageService>();
			builder.Services.AddScoped<ISpeechService, SpeechService>();
			builder.Services.AddScoped<IConversationService, ConversationService>();
			builder.Services.AddScoped<IQnAService, QnAService>();
			builder.Services.AddScoped<IDocumentService, DocumentService>();

			// Controllers + Swagger
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "FenecAI API",
					Version = "v1",
					Description = "Comprehensive Generative AI API aligned with Azure AI-102 exam objectives"
				});
				c.EnableAnnotations();
			});



			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "FenecAI API v1");
				});
			}

			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapControllers();
			app.Run();
		}
	}
}
