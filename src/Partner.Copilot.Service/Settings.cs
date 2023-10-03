namespace Partner.Copilot.Service
{
	public class Settings
	{
		public class OpenAi
		{
			public string ServiceCompletionEndpoint { get; set; } = string.Empty;
			public string ServiceKey { get; set; } = string.Empty;
			public string ServiceDeploymentId { get; set; } = string.Empty;
			public string ServiceModelName { get; set; } = string.Empty;
			public string EmbeddingsDeploymentId { get; set; } = string.Empty;
		}
	}
}
