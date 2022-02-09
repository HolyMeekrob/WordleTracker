using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class GetGroupsForUserTests : DbTests
{
	private static User GetUser(string userId) => new()
	{
		Id = userId,
		Name = userId
	};

	private static Group GetGroup(string name) => new()
	{
		Name = name
	};

	private static GroupMember GetMember(Group group, User user, GroupRole role) => new()
	{
		Group = group,
		User = user,
		Role = role
	};

	[Fact]
	public async Task NonExistentUserReturnsEmptyList()
	{
		var user = GetUser("User Id");
		var group = GetGroup("Group Name");
		var member = GetMember(group, user, GroupRole.Admin);

		DbContext.AddRange(user, group, member);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.GetGroupsForUser("Other User").ToListAsync();

		Assert.Empty(result);
	}

	[Fact]
	public async Task UserWithNoGroupsReturnsEmptyList()
	{
		var user = GetUser("User Id");
		var groupUser = GetUser("GroupUser User");
		var group = GetGroup("Group Name");
		var member = GetMember(group, groupUser, GroupRole.Admin);

		DbContext.AddRange(user, groupUser, group, member);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.GetGroupsForUser(user.Id).ToListAsync();

		Assert.Empty(result);
	}

	[Fact]
	public async Task UserWithOneGroupReturnsGroup()
	{
		var user = GetUser("User Id");
		var group = GetGroup("Group Name");
		var member = GetMember(group, user, GroupRole.Admin);

		DbContext.AddRange(user, group, member);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.GetGroupsForUser(user.Id).ToListAsync();

		Assert.Single(result);
		Assert.Equal(group.Id, result.First().Id);
	}

	[Fact]
	public async Task UserWithMultipleGroupsReturnsAllGroups()
	{
		var user = GetUser("User Id");

		var group1 = GetGroup("Group One");
		var member1 = GetMember(group1, user, GroupRole.Admin);

		var group2 = GetGroup("Group Two");
		var member2 = GetMember(group2, user, GroupRole.Owner);

		var group3 = GetGroup("Group Three");
		var member3 = GetMember(group3, user, GroupRole.Member);

		DbContext.AddRange(user, group1, group2, group3, member1, member2, member3);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.GetGroupsForUser(user.Id).ToListAsync();

		Assert.Equal(3, result.Count);
		Assert.Single(result, group => group.Id == group1.Id);
		Assert.Single(result, group => group.Id == group2.Id);
		Assert.Single(result, group => group.Id == group3.Id);
	}
}
