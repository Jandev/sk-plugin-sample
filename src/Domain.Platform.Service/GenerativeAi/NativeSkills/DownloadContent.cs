using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Domain.Platform.Service.GenerativeAi.NativeSkills
{
	/// <summary>
	/// Skill site actions
	/// </summary>
	internal class DownloadContent
	{
		private const string WebsiteUrlParameterValue = "websiteUrl";
		private readonly IHttpClientFactory httpClientFactory;

		public DownloadContent(IHttpClientFactory httpClientFactory)
		{
			this.httpClientFactory = httpClientFactory;
		}

		[SKFunction, Description("Retrieve the body from a specified website URL.")]
		[SKParameter(WebsiteUrlParameterValue, "Retrieves the content of the body from an HTML page.")]
		public async Task<string> GetBody(SKContext context)
		{
			var websiteUrl = context.Variables[WebsiteUrlParameterValue];
			var content = await GetContent(websiteUrl);

			return content;

		}

		private async Task<string> GetContent(string url)
		{
			var content = await ReadContent(url);
			Match match = Regex.Match(content, "<body[^>]*>(.*?)</body>", RegexOptions.Singleline);
			string strippedContent;
			if (match.Success)
			{
				string foundContent = match.Groups[1].Value;
				strippedContent = foundContent.Substring(0, foundContent.Length > 1800 ? 1800 : foundContent.Length);
			}
			else
			{
				throw new Exception();
			}

			return strippedContent;
		}

		private async Task<string> ReadContent(string url)
		{
			using var client = httpClientFactory.CreateClient();
			var response = await client.GetAsync(url);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStringAsync();
			}
			else
			{
				throw new Exception($"Failed to download content from {url}. Status code: {response.StatusCode}");
			}
		}
	}
}
