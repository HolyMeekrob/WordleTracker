namespace WordleTracker.Data.Models;

public class User : ITrackCreation, ITrackModification
{
	public string Id { get; set; } = null!;
	public string Name { get; set; } = null!;
	public DateTimeOffset CreatedDate { get; set; }
	public DateTimeOffset UpdatedDate { get; set; }

	public List<Result> Results { get; set; } = null!;
	public ICollection<Group> Groups { get; set; } = null!;
	public List<GroupMember> GroupMemberships { get; set; } = null!;
}
