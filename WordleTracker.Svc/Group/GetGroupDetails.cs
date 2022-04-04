using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class GroupSvc
{
	public async Task<GroupDetails?> GetGroupDetails(int groupId, CancellationToken cancellationToken)
	{
		var group = await DbContext
			.Groups
			.AsNoTracking()
			.Select(group => new
			{
				group.Id,
				group.Name,
				Members = group.Memberships.Select(membership => new
				{
					membership.UserId,
					membership.Role,
					membership.User.Name
				})
			})
			.FirstOrDefaultAsync(group => group.Id == groupId, cancellationToken);

		if (group == null)
		{
			return null;
		}

		var userIds = group.Members.Select(member => member.UserId).ToList();

		var results = (await DbContext
			.Results
			.AsNoTracking()
			.Where(result => userIds.Contains(result.UserId))
			.Select(result => new
			{
				result.UserId,
				result.Day.Date,
				GuessCount = result.Guesses.Count,
				result.HardMode,
				result.IsSolved
			})
			.ToListAsync(cancellationToken))
			.ToLookup(result => result.UserId, result => new GroupResult(
				DateOnly.FromDateTime(result.Date.UtcDateTime),
				result.IsSolved,
				result.HardMode,
				result.GuessCount
			));

		return new GroupDetails(
			groupId,
			group.Name,
			group.Members
				.Select(member => new GroupUser(
					member.UserId,
					member.Name,
					member.Role,
					results[member.UserId].ToList()))
			.ToList()
		);
	}
}

public record GroupDetails(int Id, string Name, List<GroupUser> Users);

public record GroupUser(string UserId, string Name, GroupRole Role, List<GroupResult> Results);

public record GroupResult(DateOnly Date, bool IsSolved, bool HardMode, int GuessCount);
