using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;
using System.Diagnostics;

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

			var executedPlan = await ExecutePlanAsync(kernel, plan, request);
						
			foreach (var state in executedPlan.State)
			{
				logger.LogDebug(state.Key + " : " + state.Value.Trim());
			}
			logger.LogDebug(plan.ToJson());

			return executedPlan.State["PLAN.RESULT"]?.Trim() ?? string.Empty;
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
			configuration.ExcludedFunctions.Add("WriteAsync");
			configuration.ExcludedFunctions.Add("ReadAsync");
		}

		private async Task<Plan> ExecutePlanAsync(
			IKernel kernel,
			Plan plan,
			string input = "",
			int maxSteps = 10)
		{
			Stopwatch sw = new();
			sw.Start();

			// loop until complete or at most N steps
			try
			{
				for (int step = 1; plan.HasNextStep && step < maxSteps; step++)
				{
					if (string.IsNullOrEmpty(input))
					{
						await plan.InvokeNextStepAsync(kernel.CreateNewContext());
						// or await kernel.StepAsync(plan);
					}
					else
					{
						plan = await kernel.StepAsync(input, plan);
					}

					if (!plan.HasNextStep)
					{
						this.logger.LogDebug($"Step {step} - COMPLETE!");
						this.logger.LogDebug(plan.State.ToString());
						break;
					}

					this.logger.LogDebug($"Step {step} - Results so far:");
					this.logger.LogDebug(plan.State.ToString());
				}
			}
			catch (Microsoft.SemanticKernel.Diagnostics.SKException e)
			{
				this.logger.LogDebug("Step - Execution failed:");
				this.logger.LogDebug(e.Message);
			}

			sw.Stop();
			this.logger.LogDebug($"Execution complete in {sw.ElapsedMilliseconds} ms!");
			return plan;
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
