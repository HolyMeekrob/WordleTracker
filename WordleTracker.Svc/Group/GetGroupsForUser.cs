using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class GroupSvc
{
	public IQueryable<Group> GetGroupsForUser(string userId) =>
		DbContext.Users.Where(u => u.Id == userId)
			.SelectMany(u => u.Groups)
			.AsNoTracking();
}
