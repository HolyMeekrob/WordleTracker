namespace WordleTracker.Core.Configuration
{
	public record NamesOptions
	{
		public const string Section = "Names";

		public List<string> Adjectives { get; init; } = null!;
		public List<string> Nouns { get; init; } = null!;
	}
}
