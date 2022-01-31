using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class UserSvc
{
	public async Task<User> CreateUser(string id, string name, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException("User id cannot be empty or whitespace", nameof(id));
		}

		var user = new User()
		{
			Id = id,
			Name = string.IsNullOrWhiteSpace(name) ? id : name,
		};

		DbContext.Add(user);
		await DbContext.SaveChangesAsync(cancellationToken);

		DbContext.Entry(user).State = EntityState.Detached;

		return user;
	}
}
