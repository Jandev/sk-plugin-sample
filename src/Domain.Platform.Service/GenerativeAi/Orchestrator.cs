using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;

namespace Domain.Platform.Service.GenerativeAi
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
			var plan = await CreatePlan(request);

			logger.LogDebug(plan.ToJson());

			var executedPlan = await kernel.RunAsync(plan);
			return executedPlan.GetValue<string>() ?? string.Empty;
		}

		private async Task<Plan> CreatePlan(string request)
		{
			var configuration = new SequentialPlannerConfig();
			ExcludeFunctions(configuration);
			var planner = new SequentialPlanner(kernel, configuration);

			var plan = await planner.CreatePlanAsync(request);
			this.logger.LogInformation("Original plan: {plan}", plan.ToJson());

			return plan;
		}

		private static void ExcludeFunctions(SequentialPlannerConfig configuration)
		{
			// Remove the functions to read/write files, located in the `_GLOBAL_SKILLS_`.
			// These got added randomly whenever the LLM figured a file needs to be created or read.
			configuration.ExcludedFunctions.Add("Write");
			configuration.ExcludedFunctions.Add("Read");
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
