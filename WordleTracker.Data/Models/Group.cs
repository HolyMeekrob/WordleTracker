namespace WordleTracker.Data.Models
{
	public class Group
	{
#pragma warning disable IDE0044, CS0169 // Add readonly modifier, Field is unused
		// Cannot be read-only because this is set by Entity Framework
		private int _id;
#pragma warning restore IDE0044, CS0169 // Add readonly modifier, Field is unused

		public string Name { get; set; } = null!;
		public DateTimeOffset CreatedDate { get; set; }

		public ICollection<User> Members { get; set; } = null!;
		public List<GroupMember> Memberships { get; set; } = null!;
	}
}
