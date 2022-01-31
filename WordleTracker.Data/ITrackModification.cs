namespace WordleTracker.Data;

internal interface ITrackModification
{
	public DateTimeOffset UpdatedDate { get; set; }
}
