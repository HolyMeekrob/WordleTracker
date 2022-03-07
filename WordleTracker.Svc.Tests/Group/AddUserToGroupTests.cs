using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class AddUserToGroupTests : DbTests
{
	private static User GetUser(string id) => new()
	{
		Id = id,
		Name = id
	};

	private static Group GetGroup(string name) => new()
	{
		Name = name
	};

	private static GroupMember GetGroupMembership(Group group, User member, GroupRole role) => new()
	{
		Group = group,
		User = member,
		Role = role
	};

	[Fact]
	public async Task InvalidUserIdFails()
	{
		var group = GetGroup("Group Name");

		var admin = GetUser("Admin Id");
		var adminMembership = GetGroupMembership(group, admin, GroupRole.Owner);

		var otherUser = GetUser("Other Id");

		var userId = "User Id";

		DbContext.AddRange(group, admin, adminMembership, otherUser);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.AddUserToGroup(group.Id, userId, GroupRole.Admin, admin.Id, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == userId);

		Assert.False(result.IsValid);
		Assert.Null(dbResult);
	}

	[Fact]
	public async Task InvalidGroupIdFails()
	{
		var otherGroup = GetGroup("Group Name");

		var admin = GetUser("Admin Id");
		var adminMembership = GetGroupMembership(otherGroup, admin, GroupRole.Owner);

		var user = GetUser("User Id");

		DbContext.AddRange(otherGroup, admin, adminMembership, user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.AddUserToGroup(otherGroup.Id + 1, user.Id, GroupRole.Admin, admin.Id, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == otherGroup.Id + 1 && member.UserId == user.Id);

		Assert.False(result.IsValid);
		Assert.Null(dbResult);
	}

	[Theory]
	[InlineData(GroupRole.Owner)]
	[InlineData(GroupRole.Admin)]
	[InlineData(GroupRole.Member)]
	public async Task AddingAUserWithEqualMembershipFails(GroupRole role)
	{
		var group = GetGroup("Group Name");

		var admin = GetUser("Admin Id");
		var adminMembership = GetGroupMembership(group, admin, role);

		var user = GetUser("User Id");

		DbContext.AddRange(group, admin, adminMembership, user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.AddUserToGroup(group.Id, user.Id, role, admin.Id, new CancellationToken());
		var dbResult = await DbContext.GroupMembers.FindAsync(group.Id, user.Id);

		Assert.False(result.IsValid);
		Assert.Null(dbResult);
	}

	[Theory]
	[InlineData(GroupRole.Admin, GroupRole.Owner)]
	[InlineData(GroupRole.Member, GroupRole.Owner)]
	[InlineData(GroupRole.Member, GroupRole.Admin)]
	public async Task AddingAUserWithElevatedMembershipFails(GroupRole adminRole, GroupRole userRole)
	{
		var group = GetGroup("Group Name");

		var admin = GetUser("Admin Id");
		var adminMembership = GetGroupMembership(group, admin, adminRole);

		var user = GetUser("User Id");

		DbContext.AddRange(group, admin, adminMembership, user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.AddUserToGroup(group.Id, user.Id, userRole, admin.Id, new CancellationToken());
		var dbResult = await DbContext.GroupMembers.FindAsync(group.Id, user.Id);

		Assert.False(result.IsValid);
		Assert.Null(dbResult);
	}

	public async Task AddingAUserAlreadyInTheGroupFails()
	{
		var group = GetGroup("Group Name");

		var admin = GetUser("Admin Id");
		var adminMembership = GetGroupMembership(group, admin, GroupRole.Owner);

		var user = GetUser("User Id");
		var userMembership = GetGroupMembership(group, user, GroupRole.Member);

		DbContext.AddRange(group, admin, adminMembership, user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.AddUserToGroup(group.Id, user.Id, GroupRole.Member, admin.Id, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == user.Id);

		Assert.False(result.IsValid);
		Assert.NotNull(dbResult);
	}

	[Fact]
	public async Task ReturnsNewMember()
	{
		var group = GetGroup("Group Name");

		var admin = GetUser("Admin Id");
		var adminMembership = GetGroupMembership(group, admin, GroupRole.Owner);

		var user = GetUser("Other Id");
		var role = GroupRole.Admin;

		DbContext.AddRange(group, admin, adminMembership, user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.AddUserToGroup(group.Id, user.Id, role, admin.Id, new CancellationToken());

		Assert.True(result.IsValid);
		Assert.Equal(user.Id, result.Value.UserId);
		Assert.Equal(group.Id, result.Value.GroupId);
		Assert.Equal(role, result.Value.Role);
	}

	[Fact]
	public async Task AddsUserToGroup()
	{
		var group = GetGroup("Group Name");

		var admin = GetUser("Admin Id");
		var adminMembership = GetGroupMembership(group, admin, GroupRole.Admin);

		var user = GetUser("Other Id");
		var role = GroupRole.Member;

		DbContext.AddRange(group, admin, adminMembership, user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		await svc.AddUserToGroup(group.Id, user.Id, role, admin.Id, new CancellationToken());
		var result = DbContext.GroupMembers.Where(member => member.UserId == user.Id);

		Assert.Single(result);
		Assert.Equal(group.Id, result.First().GroupId);
	}

	[Theory]
	[InlineData(GroupRole.Member)]
	[InlineData(GroupRole.Admin)]
	public async Task AssignsCorrectRole(GroupRole role)
	{
		var group = GetGroup("Group Name");

		var admin = GetUser("Admin Id");
		var adminMembership = GetGroupMembership(group, admin, role + 1);

		var user = GetUser("Other Id");

		DbContext.AddRange(group, admin, adminMembership, user);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		await svc.AddUserToGroup(group.Id, user.Id, role, admin.Id, new CancellationToken());
		var result = DbContext.GroupMembers.Where(member => member.UserId == user.Id);

		Assert.Single(result);
		Assert.Equal(role, result.First().Role);
	}
}
