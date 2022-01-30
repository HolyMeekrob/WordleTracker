namespace WordleTracker.Core.Configuration;
public record DaysOptions
{
	public const string Section = "Days";

	public List<DailyWord> Words { get; init; } = null!;

	public record DailyWord
	{
		public int Id { get; init; }
		public string Date { get; init; } = null!;
		public string Word { get; init; } = null!;
	}
}
