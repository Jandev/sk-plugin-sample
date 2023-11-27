using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Domain.Platform.Service.GenerativeAi.NativeSkills
{
	/// <summary>
	/// Skill site actions
	/// </summary>
	internal class DownloadContent
	{
		private readonly IHttpClientFactory httpClientFactory;
		private readonly ILogger logger;

		public DownloadContent(
			IHttpClientFactory httpClientFactory,
			ILogger logger)
		{
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
		}

		[SKFunction, Description("Retrieve the body from a specified website URL.")]
		public async Task<string> GetBody(string websiteUrl)
		{
			logger.LogDebug("Retrieving the content for `{websiteUrl}`.", websiteUrl);
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
