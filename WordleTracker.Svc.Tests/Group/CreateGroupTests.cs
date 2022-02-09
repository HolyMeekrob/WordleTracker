using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class CreateGroupTests : DbTests
{
	private static User GetUser(string userId) => new()
	{
		Id = userId,
		Name = userId
	};

	[Fact]
	public async Task InvalidUserThrowsAnException()
	{
		var userId = "User Id";
		var groupName = "Group Name";

		var user = GetUser("Other Id");
		DbContext.Users.Add(user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		await Assert.ThrowsAsync<DbUpdateException>(() =>
			svc.CreateGroup(groupName, userId, new CancellationToken())
		);
	}

	[Fact]
	public async Task GroupHasCorrectName()
	{
		var userId = "User Id";
		var groupName = "Group Name";

		var user = GetUser(userId);
		DbContext.Users.Add(user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.CreateGroup(groupName, userId, new CancellationToken());

		Assert.NotNull(result);
		Assert.Equal(groupName, result.Name);
	}

	[Fact]
	public async Task GroupIsCreatedInDatabase()
	{
		var userId = "User Id";
		var groupName = "Group Name";

		var user = GetUser(userId);
		DbContext.Users.Add(user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.CreateGroup(groupName, userId, new CancellationToken());

		var dbGroup = await DbContext.Groups.FindAsync(result.Id);
		Assert.NotNull(dbGroup);
		Assert.Equal(groupName, dbGroup!.Name);
	}

	[Fact]
	public async Task CreatesOwnerMember()
	{
		var userId = "User Id";
		var groupName = "Group Name";

		var user = GetUser(userId);
		DbContext.Users.Add(user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.CreateGroup(groupName, userId, new CancellationToken());

		var members = await DbContext.GroupMembers
			.Where(member => member.GroupId == result.Id)
			.ToListAsync();

		Assert.Single(members);
		Assert.Equal(GroupRole.Owner, members.First().Role);
		Assert.Equal(userId, members.First().UserId);
	}
}
