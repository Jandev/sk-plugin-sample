using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Partner.Copilot.Service;
using Partner.Copilot.Service.GenerativeAi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AddOptions(builder.Services);
RegisterServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddOptions(IServiceCollection s)
{
	s.AddOptions<Settings.OpenAi>()
				.Configure<IConfiguration>((settings, configuration) =>
				{
					configuration.GetSection(nameof(Settings.OpenAi)).Bind(settings);
				});
}

static void RegisterServices(IServiceCollection s)
{
	s.AddHttpClient();
	s.AddTransient<IOrchestrator, Orchestrator>();
	s.AddSingleton(
		typeof(IKernel),
		s =>
		{
			var openAiOptions = s.GetRequiredService<IOptions<Settings.OpenAi>>();
			var logger = s.GetRequiredService<ILogger<IKernel>>();
			var openAiSettings = openAiOptions.Value;

			var skillCollectionToLoad = new string[] { "website" };

			var kernel = new KernelBuilder()
				.WithAzureTextCompletionService(
							openAiSettings.ServiceModelName,
							openAiSettings.ServiceCompletionEndpoint,
							openAiSettings.ServiceKey
				)
				.WithAzureOpenAITextEmbeddingGenerationService(
							openAiSettings.EmbeddingsDeploymentId,
							openAiSettings.ServiceCompletionEndpoint,
							openAiSettings.ServiceKey
				)
			.Build();

			AddSemanticSkills();
			AddNativeSkills();
			AddOpenAiPlugins();

			return kernel;

			void AddSemanticSkills()
			{
				logger.LogInformation("Importing semantic skills");
				string skillsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "GenerativeAi", "SemanticSkills");
			}

			void AddNativeSkills()
			{
				logger.LogInformation("Importing native skills");
				kernel.ImportFunctions(new Microsoft.SemanticKernel.Plugins.Core.TextPlugin());
			}

			void AddOpenAiPlugins()
			{
				//const string pluginManifestUrl = "https://localhost:7044/.well-known/ai-plugin.json";
				//kernel.ImportAIPluginAsync("DomainSkill", new Uri(pluginManifestUrl)).ConfigureAwait(false);
			}
		});
}