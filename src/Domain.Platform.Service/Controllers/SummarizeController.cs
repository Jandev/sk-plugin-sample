using Domain.Platform.Service.GenerativeAi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;

namespace Domain.Platform.Service.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SummarizeController : ControllerBase
	{
		private readonly IOrchestrator orchestrator;
		private readonly ILogger<SummarizeController> logger;

		public SummarizeController(
			IOrchestrator orchestrator,
			ILogger<SummarizeController> logger)
		{
			this.orchestrator = orchestrator;
			this.logger = logger;
		}

		[HttpGet(Name = nameof(Website))]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[OpenApiOperation(operationId: "Website", tags: new[] { "Summarize" }, Description = "Creates a summary from the specified website URL.")]
		[OpenApiParameter(name: "websiteUrl", Description = "The full URL from a website, including the schema.", Required = true, In = ParameterLocation.Query)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The summary from the specified website.")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
		public async Task<ActionResult<string>> Website(string websiteUrl)
		{
			var response = await this.orchestrator.Invoke(websiteUrl);
			this.logger.LogDebug(response);
			return Ok(response);
		}
	}
}
