using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;
public partial class UserSvc
{
	public async Task<User> UpdateName(string id, string name, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("User name cannot be empty or whitespace", nameof(name));
		}

		var user = DbContext.Find<User>(id);

		if (user == null)
		{
			throw new KeyNotFoundException($"UserSvc.UpdateName: User with id {id} was not found");
		}

		user.Name = name;
		// TODO: Automate this
		user.UpdatedDate = DateTime.UtcNow;
		await DbContext.SaveChangesAsync(cancellationToken);

		DbContext.Entry(user).State = EntityState.Detached;
		return user;
	}
}

