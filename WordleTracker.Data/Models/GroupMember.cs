namespace WordleTracker.Data.Models
{
	public class GroupMember
	{
		public int GroupId { get; set; }
		public string UserId { get; set; } = null!;
		public GroupRole Role { get; set; }
		public DateTimeOffset CreatedDate { get; set; }
		public DateTimeOffset UpdatedDate { get; set; }

		public Group Group { get; set; } = null!;
		public User User { get; set; } = null!;
	}

	public enum GroupRole
	{
		Member,
		Admin,
		Owner
	}
}
