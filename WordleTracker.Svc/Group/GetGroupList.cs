using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class GroupSvc
{
	public async Task<List<GroupInfo>> GetGroupList(string userId, CancellationToken cancellationToken)
	{
		var groups = await DbContext
			.Users
			.Where(user => user.Id == userId)
			.SelectMany(user => user.Groups.Select(group => new { group.Id, group.Name }))
			.ToListAsync(cancellationToken);

		var groupIds = groups.Select(group => group.Id).ToList();

		var memberships = (await DbContext
			.GroupMembers
			.Where(membership => groupIds.Contains(membership.GroupId))
			.Select(membership => new { membership.GroupId, membership.UserId, membership.Role })
			.ToListAsync(cancellationToken))
			.ToLookup(membership => membership.GroupId);

		return groups
			.Select(group => new GroupInfo(
				group.Id,
				group.Name,
				memberships[group.Id].Count(),
				memberships[group.Id].First(user => user.UserId == userId).Role))
			.ToList();
	}
}

public record GroupInfo(int Id, string Name, int Size, GroupRole Role);
