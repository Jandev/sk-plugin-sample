using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Functions.OpenAPI.OpenAI;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;

namespace Partner.Copilot.Service.GenerativeAi
{
	public class Orchestrator : IOrchestrator
	{
		private readonly IKernel kernel;
		private readonly ILogger<Orchestrator> logger;

		public Orchestrator(
			IKernel kernel,
			ILogger<Orchestrator> logger)
		{
			this.kernel = kernel;
			this.logger = logger;
		}

		/// <inheritdoc />
		public async Task<string> Invoke(string request)
		{
			await AddOpenAiPlugins();

			var plan = await CreatePlan(request);
			this.logger.LogDebug(plan.ToJson());

			var functionResult = await plan.InvokeAsync(this.kernel);

			if (functionResult.TryGetMetadataValue("stepCount", out string stepCount))
			{
				this.logger.LogDebug("Steps Taken: " + stepCount);
			}
			if (functionResult.TryGetMetadataValue("functionCount", out string functionCount))
			{
				this.logger.LogDebug("Functions Used: " + functionCount);
			}
			if (functionResult.TryGetMetadataValue("iterations", out string iterations))
			{
				this.logger.LogDebug("Iterations: " + iterations);
			}

			return functionResult.GetValue<string>()!;
		}

		private async Task AddOpenAiPlugins()
		{
			const string pluginManifestUrl = "https://localhost:7044/.well-known/ai-plugin.json";
			await kernel.ImportOpenAIPluginFunctionsAsync("DomainSkill", new Uri(pluginManifestUrl));
		}

		private async Task<Plan> CreatePlan(string request, bool useStepwisePlanner = true)
		{
			if (useStepwisePlanner)
			{
				StepwisePlannerConfig config = new()
				{
					MaxTokens = 2000,
					MaxIterations = 2,
				};
				config.ExcludedFunctions.Add("_GLOBAL_FUNCTIONS_");

				var planner = new StepwisePlanner(kernel, config);
				var plan = planner.CreatePlan(request);
				return plan;
			}
			else
			{
				var configuration = new SequentialPlannerConfig();
				configuration.ExcludedFunctions.Add("Write");
				configuration.ExcludedFunctions.Add("Read");
				configuration.ExcludedFunctions.Add("_GLOBAL_FUNCTIONS_");
				var planner = new SequentialPlanner(kernel, configuration);

				var plan = await planner.CreatePlanAsync(request);
				this.logger.LogDebug("Original plan: {plan}", plan.ToJson());

				return plan;
			}			
		}
	}

	public interface IOrchestrator
	{
		/// <summary>
		/// Entry point to the orchestrator, used to invoke the requested generative AI capabilities.
		/// </summary>
		/// <param name="request">The client request to be handled.</param>
		/// <returns>The response from the invoked generative AI.</returns>
		public Task<string> Invoke(string request);
	}
}
