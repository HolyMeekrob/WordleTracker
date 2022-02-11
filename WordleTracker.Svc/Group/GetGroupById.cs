using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class GroupSvc
{
	public async Task<Group> GetGroupById(int id, CancellationToken cancellationToken) =>
		await DbContext.Groups
			.AsNoTracking()
			.Include(group => group.Memberships)
			.ThenInclude(membership => membership.User)
			.ThenInclude(user => user.Results)
			.ThenInclude(result => result.Day)
			.FirstAsync(group => group.Id == id, cancellationToken);
}
