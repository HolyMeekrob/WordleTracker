using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class GroupSvc
{
	public async Task<Group> CreateGroup(string name, string adminId, CancellationToken cancellationToken)
	{
		var group = new Group
		{
			Name = name
		};

		var membership = new GroupMember
		{
			UserId = adminId,
			Group = group,
			Role = GroupRole.Owner,
		};

		DbContext.Groups.Add(group);
		DbContext.GroupMembers.Add(membership);
		await DbContext.SaveChangesAsync(cancellationToken);

		DbContext.Entry(group).State = EntityState.Detached;
		return group;
	}
}
