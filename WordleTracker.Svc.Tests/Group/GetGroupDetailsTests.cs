using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class GetGroupDetailsTests : DbTests
{
	private static Group GetGroup(string name) => new()
	{
		Name = name
	};

	private static User GetUser(string name) => new()
	{
		Id = name,
		Name = name
	};

	private static Day GetDay(int dayNumber) => new()
	{
		Id = dayNumber,
		Date = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(dayNumber)
	};

	private static Result GetResult(string userId, int dayId, int guessCount) => new()
	{
		UserId = userId,
		DayId = dayId,
		HardMode = false,
		Guesses = Enumerable
			.Range(0, guessCount - 1)
			.Select(_ => new WordGuess() { Guesses = Enumerable.Repeat(LetterGuess.Missing, 5).ToImmutableList() })
			.Append(new WordGuess() { Guesses = Enumerable.Repeat(LetterGuess.Correct, 5).ToImmutableList() })
			.ToImmutableList()
	};

	private static GroupMember GetMember(int groupId, string userId, GroupRole role) => new()
	{
		GroupId = groupId,
		UserId = userId,
		Role = role
	};

	[Fact]
	public async Task GroupDoesNotExistReturnsNull()
	{
		var group = GetGroup("Unused Group");
		DbContext.Groups.Add(group);

		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);
		var result = await svc.GetGroupDetails(group.Id + 1, new CancellationToken());

		Assert.Null(result);
	}

	[Fact]
	public async Task GroupDoesNotHaveMembers()
	{
		var group = GetGroup("Empty Group");
		DbContext.Groups.Add(group);

		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);
		var result = await svc.GetGroupDetails(group.Id, new CancellationToken());

		Assert.NotNull(result);
		Assert.Equal(group.Id, result!.Id);
		Assert.Equal(group.Name, result.Name);
		Assert.Empty(group.Memberships);
	}

	[Fact]
	public async Task GroupHasMembersWithoutResults()
	{
		var group = GetGroup("Group Name");
		DbContext.Groups.Add(group);

		var ownerUser = GetUser("Owner");
		var memberUser = GetUser("Member");
		DbContext.Users.AddRange(ownerUser, memberUser);

		await DbContext.SaveChangesAsync();

		var owner = GetMember(group.Id, ownerUser.Id, GroupRole.Owner);
		var member = GetMember(group.Id, memberUser.Id, GroupRole.Member);
		DbContext.GroupMembers.AddRange(owner, member);

		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);
		var result = await svc.GetGroupDetails(group.Id, new CancellationToken());

		Assert.NotNull(result);
		Assert.Equal(group.Id, result!.Id);
		Assert.Equal(group.Name, result.Name);
		Assert.Equal(2, result.Users.Count);
		Assert.Single(result.Users, user => user.UserId == ownerUser.Id);
		Assert.Single(result.Users, user => user.UserId == memberUser.Id);
	}

	[Fact]
	public async Task GroupHasMembersWithResults()
	{
		var group = GetGroup("Group Name");
		DbContext.Groups.Add(group);

		var ownerUser = GetUser("Owner");
		var memberUser = GetUser("Member");
		DbContext.Users.AddRange(ownerUser, memberUser);

		var day1 = GetDay(1);
		var day2 = GetDay(2);
		var day3 = GetDay(3);
		DbContext.Days.AddRange(day1, day2, day3);

		await DbContext.SaveChangesAsync();

		var ownerResult1 = GetResult(ownerUser.Id, day1.Id, 3);
		var ownerResult2 = GetResult(ownerUser.Id, day2.Id, 4);
		var ownerResult3 = GetResult(ownerUser.Id, day3.Id, 5);
		var memberResult1 = GetResult(memberUser.Id, day1.Id, 2);
		var memberResult2 = GetResult(memberUser.Id, day3.Id, 6);
		DbContext.Results.AddRange(ownerResult1, ownerResult2, ownerResult3, memberResult1, memberResult2);

		var owner = GetMember(group.Id, ownerUser.Id, GroupRole.Owner);
		var member = GetMember(group.Id, memberUser.Id, GroupRole.Member);
		DbContext.GroupMembers.AddRange(owner, member);

		await DbContext.SaveChangesAsync();

		var svc = new GroupSvc(DbContext);
		var result = await svc.GetGroupDetails(group.Id, new CancellationToken());

		Assert.NotNull(result);
		Assert.Equal(group.Id, result!.Id);
		Assert.Equal(group.Name, result.Name);
		Assert.Equal(2, result.Users.Count);
		Assert.Single(result.Users, user => user.UserId == ownerUser.Id);
		Assert.Single(result.Users, user => user.UserId == memberUser.Id);
		Assert.Equal(3, result.Users.First(user => user.UserId == ownerUser.Id).Results.Count);
		Assert.Equal(2, result.Users.First(user => user.UserId == memberUser.Id).Results.Count);
	}
}
