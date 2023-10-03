using Microsoft.AspNetCore.Mvc;
using System.Web.Http;

namespace Domain.Platform.Service.Controllers
{
	[Route(".well-known/ai-plugin.json")]
	[ApiController]
	public class AiPluginController : ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IResult Plugin()
		{
			var currentDomain = $"{this.Request.Scheme}://{Request.Host.Host}:{Request.Host.Port}";

			var staticFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ai-plugin.json");
			var json = System.IO.File.ReadAllText(staticFilePath);

			json = json.Replace("{url}", currentDomain, StringComparison.OrdinalIgnoreCase);

			return Results.Content(json, "application/json");
		}
	}
}
