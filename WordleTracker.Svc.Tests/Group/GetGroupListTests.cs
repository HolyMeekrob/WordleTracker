using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;
public class GetGroupListTests : DbTests
{
	private static User GetUser(string name) => new()
	{
		Id = name,
		Name = name
	};

	private static Group GetGroup(string name) => new()
	{
		Name = name
	};

	private static GroupMember GetMember(int groupId, string userId, GroupRole role) => new()
	{
		GroupId = groupId,
		UserId = userId,
		Role = role
	};

	[Fact]
	public async Task UserHasNoGroupsReturnsEmpty()
	{
		var userId = "User Name";
		var user = GetUser(userId);
		DbContext.Users.Add(user);

		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);
		var result = await svc.GetGroupList(userId, new CancellationToken());

		Assert.Empty(result);
	}

	[Fact]
	public async Task UserDoesNotExistReturnsEmpty()
	{
		var user = GetUser("Other User");
		DbContext.Users.Add(user);

		var group = GetGroup("Group Name");
		DbContext.Groups.Add(group);

		await DbContext.SaveChangesAsync();

		var membership = GetMember(group.Id, user.Id, GroupRole.Owner);
		DbContext.GroupMembers.Add(membership);

		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);
		var result = await svc.GetGroupList("User Name", new CancellationToken());

		Assert.Empty(result);
	}

	[Fact]
	public async Task UserHasGroupsReturnsGroups()
	{
		var user = GetUser("User Name");
		var userGroup1_1 = GetUser("User Group One");
		var userGroup2_1 = GetUser("User One Group Two");
		var userGroup2_2 = GetUser("User Two Group Two");
		DbContext.Users.AddRange(user, userGroup1_1, userGroup2_1, userGroup2_2);

		var group1 = GetGroup("Group One");
		var group2 = GetGroup("Group Two");
		var group3 = GetGroup("Group Three");
		DbContext.Groups.AddRange(group1, group2, group3);

		await DbContext.SaveChangesAsync();

		var memberships = new[]
		{
			GetMember(group1.Id, user.Id, GroupRole.Owner),
			GetMember(group1.Id, userGroup1_1.Id, GroupRole.Member),

			GetMember(group2.Id, user.Id, GroupRole.Member),
			GetMember(group2.Id, userGroup2_1.Id, GroupRole.Owner),
			GetMember(group2.Id, userGroup2_2.Id, GroupRole.Member),

			GetMember(group3.Id, userGroup1_1.Id, GroupRole.Owner)
		};

		DbContext.GroupMembers.AddRange(memberships);

		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);
		var result = await svc.GetGroupList("User Name", new CancellationToken());

		Assert.Equal(2, result.Count);

		var group1Result = result.First(group => group.Id == group1.Id);
		Assert.Equal(2, group1Result.Size);
		Assert.Equal(group1.Name, group1Result.Name);
		Assert.Equal(GroupRole.Owner, group1Result.Role);

		var group2Result = result.First(group => group.Id == group2.Id);
		Assert.Equal(3, group2Result.Size);
		Assert.Equal(group2.Name, group2Result.Name);
		Assert.Equal(GroupRole.Member, group2Result.Role);
	}
}
