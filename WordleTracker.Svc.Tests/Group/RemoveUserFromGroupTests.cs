using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class RemoveUserFromGroupTests : DbTests
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
	public async Task NonExistentAdminFails()
	{
		var admin = GetUser("Other Admin");

		var userId = "User Id";
		var user = GetUser(userId);

		var group = GetGroup("Group Name");

		var adminMembership = GetGroupMembership(group, admin, GroupRole.Owner);
		var userMembership = GetGroupMembership(group, user, GroupRole.Member);

		DbContext.AddRange(admin, user, group, adminMembership, userMembership);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.RemoveUserFromGroup(group.Id, userId, "Admin Id", new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == userId);

		Assert.False(result.IsValid);
		Assert.NotNull(dbResult);
	}

	[Fact]
	public async Task NonExistentUserFails()
	{
		var adminId = "Admin Id";
		var admin = GetUser(adminId);

		var userId = "User Id";
		var user = GetUser("Other Id");

		var group = GetGroup("Group Name");

		var adminMembership = GetGroupMembership(group, admin, GroupRole.Owner);
		var userMembership = GetGroupMembership(group, user, GroupRole.Member);

		DbContext.AddRange(admin, user, group, adminMembership, userMembership);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.RemoveUserFromGroup(group.Id, userId, adminId, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == userId);

		Assert.False(result.IsValid);
		Assert.Null(dbResult);
	}

	[Fact]
	public async Task NonExistentGroupFails()
	{
		var adminId = "Admin Id";
		var admin = GetUser(adminId);

		var userId = "User Id";
		var user = GetUser(userId);

		var group = GetGroup("Other Group");

		var adminMembership = GetGroupMembership(group, admin, GroupRole.Owner);
		var userMembership = GetGroupMembership(group, user, GroupRole.Member);

		DbContext.AddRange(admin, user, group, adminMembership, userMembership);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.RemoveUserFromGroup(group.Id + 1, userId, adminId, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == userId);

		Assert.False(result.IsValid);
		Assert.NotNull(dbResult);
	}

	[Fact]
	public async Task AdminNotBeingInGroupFails()
	{
		var adminId = "Admin Id";
		var admin = GetUser(adminId);

		var userId = "User Id";
		var user = GetUser(userId);

		var group = GetGroup("Group Name");

		var userMembership = GetGroupMembership(group, user, GroupRole.Member);

		DbContext.AddRange(admin, user, group, userMembership);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.RemoveUserFromGroup(group.Id, userId, adminId, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == userId);

		Assert.False(result.IsValid);
		Assert.NotNull(dbResult);
	}

	[Fact]
	public async Task UserNotBeingInGroupFails()
	{
		var adminId = "Admin Id";
		var admin = GetUser(adminId);

		var userId = "User Id";
		var user = GetUser(userId);

		var group = GetGroup("Group Name");

		var adminMembership = GetGroupMembership(group, admin, GroupRole.Owner);

		DbContext.AddRange(admin, user, group, adminMembership);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.RemoveUserFromGroup(group.Id, userId, adminId, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == userId);

		Assert.False(result.IsValid);
		Assert.Null(dbResult);
	}

	[Theory]
	[InlineData(GroupRole.Member, GroupRole.Member)]
	[InlineData(GroupRole.Member, GroupRole.Admin)]
	[InlineData(GroupRole.Member, GroupRole.Owner)]
	[InlineData(GroupRole.Admin, GroupRole.Admin)]
	[InlineData(GroupRole.Admin, GroupRole.Owner)]
	[InlineData(GroupRole.Owner, GroupRole.Owner)]
	public async Task AdminNotHavingPermissionToRemoveUserFails(GroupRole adminRole, GroupRole userRole)
	{
		var adminId = "Admin Id";
		var admin = GetUser(adminId);

		var userId = "User Id";
		var user = GetUser(userId);

		var group = GetGroup("Group Name");

		var adminMembership = GetGroupMembership(group, admin, adminRole);
		var userMembership = GetGroupMembership(group, user, userRole);

		DbContext.AddRange(admin, user, group, adminMembership, userMembership);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.RemoveUserFromGroup(group.Id, userId, adminId, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == userId);

		Assert.False(result.IsValid);
		Assert.NotNull(dbResult);
	}

	[Theory]
	[InlineData(GroupRole.Owner, GroupRole.Member)]
	[InlineData(GroupRole.Owner, GroupRole.Admin)]
	[InlineData(GroupRole.Admin, GroupRole.Member)]
	public async Task RemovingAUserSucceeds(GroupRole adminRole, GroupRole userRole)
	{
		var adminId = "Admin Id";
		var admin = GetUser(adminId);

		var userId = "User Id";
		var user = GetUser(userId);

		var group = GetGroup("Group Name");

		var adminMembership = GetGroupMembership(group, admin, adminRole);
		var userMembership = GetGroupMembership(group, user, userRole);

		DbContext.AddRange(admin, user, group, adminMembership, userMembership);
		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);

		var result = await svc.RemoveUserFromGroup(group.Id, userId, adminId, new CancellationToken());
		var dbResult = await DbContext
			.GroupMembers
			.FirstOrDefaultAsync(member => member.GroupId == group.Id && member.UserId == userId);

		Assert.True(result.IsValid);
		Assert.Null(dbResult);
	}
}
