using Microsoft.OpenApi.Models;
using FenecAI.API.Config;
using FenecAI.API.Services;

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

			// Services
			builder.Services.AddSingleton<ChatService>();
			builder.Services.AddSingleton<StorageService>();
			builder.Services.AddSingleton<RAGService>();
			builder.Services.AddScoped<ImageService>();
			builder.Services.AddScoped<ContextSafetyService>();
			builder.Services.AddScoped<EmbeddingsService>();
			builder.Services.AddScoped<MetricsService>();


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
