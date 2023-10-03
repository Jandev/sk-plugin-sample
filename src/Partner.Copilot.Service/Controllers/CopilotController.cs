using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Partner.Copilot.Service.GenerativeAi;
using System.Net;

namespace Partner.Copilot.Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CopilotController : ControllerBase
	{
		private readonly IOrchestrator orchestrator;

		public CopilotController(
			IOrchestrator orchestrator
			)
		{
			this.orchestrator = orchestrator;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[OpenApiOperation(operationId: "Ask", tags: new[] { "Copilot" }, Description = "Request a question to the Copilot.")]
		[OpenApiParameter(name: "request", Description = "An object with the `Ask` property containing a natural language request.", Required = true, In = ParameterLocation.Query)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "An answer to the request.")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
		public async Task<IActionResult> Post([FromBody] Request request)
		{
			var result = await orchestrator.Invoke(request.Ask);

			return Ok(result);
		}

		public class Request
		{
			public string Ask { get; set; }
		}
	}
}
