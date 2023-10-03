using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Web.Http;

namespace Domain.Platform.Service.Controllers
{
	[Route(".well-known/ai-plugin.json")]
	[ApiController]
	public class AiPluginController : ControllerBase
	{
		private const string AiPluginSettingsNode = "aiPlugin";
		private readonly IConfiguration configuration;

		public AiPluginController(
			IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult Plugin()
		{
			var currentDomain = $"{this.Request.Scheme}://{Request.Host.Host}:{Request.Host.Port}";

			var aiPluginObject = configuration.GetSection(AiPluginSettingsNode).Get<Dictionary<string, object>>();
			string json = JsonConvert.SerializeObject(aiPluginObject);

			if (string.IsNullOrWhiteSpace(json))
			{
				return new InternalServerErrorResult();
			}

			json = json.Replace("{url}", currentDomain, StringComparison.OrdinalIgnoreCase);

			return new JsonResult(json);
		}
	}
}
