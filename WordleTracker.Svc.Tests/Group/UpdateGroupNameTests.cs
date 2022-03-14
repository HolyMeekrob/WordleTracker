using System.Threading;
using System.Threading.Tasks;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class UpdateGroupNameTests : DbTests
{
	public static Group GetGroup(string name) => new()
	{
		Name = name
	};

	public static User GetUser(string id) => new()
	{
		Id = id,
		Name = id
	};

	private static GroupMember GetMember(Group group, User user, GroupRole role) => new()
	{
		Group = group,
		User = user,
		Role = role
	};

	[Fact]
	public async Task FailsIfGroupDoesNotExist()
	{
		var userId = "User Id";
		var user = GetUser(userId);

		DbContext.Add(user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.UpdateGroupName(123, userId, "Group Name", new CancellationToken());

		Assert.False(result.IsValid);
	}

	[Fact]
	public async Task FailsIfUserDoesNotExist()
	{
		var originalName = "Original Name";
		var group = GetGroup(originalName);

		DbContext.Add(group);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.UpdateGroupName(group.Id, "User Id", "Updated Name", new CancellationToken());
		var dbGroup = await DbContext.Groups.FindAsync(group.Id);

		Assert.False(result.IsValid);
		Assert.Equal(originalName, dbGroup!.Name);
	}

	[Fact]
	public async Task FailsIfUserIsNotInGroup()
	{
		var userId = "User Id";
		var user = GetUser(userId);

		var originalName = "Original Name";
		var group = GetGroup(originalName);

		DbContext.AddRange(user, group);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.UpdateGroupName(group.Id, userId, "Updated Name", new CancellationToken());
		var dbGroup = await DbContext.Groups.FindAsync(group.Id);

		Assert.False(result.IsValid);
		Assert.Equal(originalName, dbGroup!.Name);
	}

	[Theory]
	[InlineData(GroupRole.Member)]
	[InlineData(GroupRole.Admin)]
	public async Task FailsIfMemberCannotUpdateName(GroupRole role)
	{
		var userId = "User Id";
		var user = GetUser(userId);

		var originalName = "Original Name";
		var group = GetGroup(originalName);

		var member = GetMember(group, user, role);

		DbContext.AddRange(user, group, member);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.UpdateGroupName(group.Id, userId, "Updated Name", new CancellationToken());
		var dbGroup = await DbContext.Groups.FindAsync(group.Id);

		Assert.False(result.IsValid);
		Assert.Equal(originalName, dbGroup!.Name);
	}

	[Fact]
	public async Task SucceedsIfMemberCanUpdateName()
	{
		var userId = "User Id";
		var user = GetUser(userId);

		var originalName = "Original Name";
		var updatedName = "Update Name";
		var group = GetGroup(originalName);

		var member = GetMember(group, user, GroupRole.Owner);

		DbContext.AddRange(user, group, member);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.UpdateGroupName(group.Id, userId, updatedName, new CancellationToken());
		var dbGroup = await DbContext.Groups.FindAsync(group.Id);

		Assert.True(result.IsValid);
		Assert.Equal(updatedName, dbGroup!.Name);
	}

}
