namespace WordleTracker.Data.Models;

public class Group : ITrackCreation, ITrackModification
{
	public int Id { get; set; }

	public string Name { get; set; } = null!;
	public DateTimeOffset CreatedDate { get; set; }
	public DateTimeOffset UpdatedDate { get; set; }

	public ICollection<User> Members { get; set; } = null!;
	public List<GroupMember> Memberships { get; set; } = new();
}
