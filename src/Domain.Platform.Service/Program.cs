using Domain.Platform.Service;
using Domain.Platform.Service.GenerativeAi;
using Domain.Platform.Service.GenerativeAi.NativeSkills;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
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
				.WithAzureTextEmbeddingGenerationService(
							openAiSettings.EmbeddingsDeploymentId,
							openAiSettings.ServiceCompletionEndpoint,
							openAiSettings.ServiceKey
				)
			.Build();

			AddSemanticSkills();
			AddNativeSkills();

			return kernel;

			void AddSemanticSkills()
			{
				logger.LogInformation("Importing semantic skills");
				string skillsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "GenerativeAi", "SemanticSkills");
				kernel.ImportSemanticSkillFromDirectory(skillsPath, "Domain");
			}

			void AddNativeSkills()
			{
				logger.LogInformation("Importing native skills");
				kernel.ImportSkill(new Microsoft.SemanticKernel.Skills.Core.TextSkill());
				kernel.ImportSkill(new DownloadContent(s.GetRequiredService<IHttpClientFactory>()), "MySkills");
			}
		});
}