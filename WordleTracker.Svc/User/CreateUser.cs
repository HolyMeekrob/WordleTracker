using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class UserSvc
{
	public async Task<User> CreateUser(string id, string name, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException(nameof(id), "User Id cannot be empty or whitespace");
		}

		var now = DateTimeOffset.UtcNow;

		var user = new User()
		{
			Id = id,
			Name = string.IsNullOrWhiteSpace(name) ? id : name,
			CreatedDate = now,
			UpdatedDate = now
		};

		DbContext.Add(user);
		await DbContext.SaveChangesAsync(cancellationToken);

		DbContext.Entry(user).State = EntityState.Detached;

		return user;
	}
}
