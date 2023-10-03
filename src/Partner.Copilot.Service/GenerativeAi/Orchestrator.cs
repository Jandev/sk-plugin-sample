using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;
using Microsoft.SemanticKernel.Skills.OpenAPI.Extensions;

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

			var executedPlan = await ExecutePlanAsync(plan);

			if (executedPlan.Variables.TryGetValue("stepCount", out string? stepCount))
			{
				this.logger.LogDebug("Steps Taken: " + stepCount);
			}
			if (executedPlan.Variables.TryGetValue("skillCount", out string? skillCount))
			{
				this.logger.LogDebug("Skills Used: " + skillCount);
			}

			return executedPlan.ToString();
		}

		private async Task AddOpenAiPlugins()
		{
			const string pluginManifestUrl = "https://localhost:7044/.well-known/ai-plugin.json";
			await kernel.ImportAIPluginAsync("DomainSkill", new Uri(pluginManifestUrl));
		}

		private async Task<Plan> CreatePlan(string request, bool useStepwisePlanner = true)
		{
			if (useStepwisePlanner)
			{
				Microsoft.SemanticKernel.Planning.Stepwise.StepwisePlannerConfig config = new()
				{
					MaxTokens = 2000,
					MaxIterations = 2,
				};
				config.ExcludedSkills.Add("_GLOBAL_FUNCTIONS_");

				var planner = new StepwisePlanner(kernel, config);
				var plan = planner.CreatePlan(request);
				return plan;
			}
			else
			{
				var configuration = new SequentialPlannerConfig();
				configuration.ExcludedFunctions.Add("WriteAsync");
				configuration.ExcludedFunctions.Add("ReadAsync");
				configuration.ExcludedSkills.Add("_GLOBAL_FUNCTIONS_");
				var planner = new SequentialPlanner(kernel, configuration);

				var plan = await planner.CreatePlanAsync(request);
				this.logger.LogDebug("Original plan: {plan}", plan.ToJson());

				return plan;
			}			
		}

		private async Task<SKContext> ExecutePlanAsync(
			Plan plan)
		{
			Microsoft.SemanticKernel.AI.TextCompletion.CompleteRequestSettings settings = new()
			{
				MaxTokens = 2000,
			};
			return await plan.InvokeAsync(kernel.CreateNewContext(), settings: settings);
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
