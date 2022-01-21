namespace WordleTracker.Core.Configuration
{
	public record NamesOptions
	{
		public const string Section = "Names";

		public List<string> Adjectives { get; set; } = new();
		public List<string> Nouns { get; set; } = new();
	}
}
