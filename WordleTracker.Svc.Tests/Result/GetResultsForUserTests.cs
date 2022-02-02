using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;
public class GetResultsForUserTests : DbTests
{
	private static User GetUser(string userId) => new()
	{
		Id = userId,
		Name = userId
	};

	private static Day GetDay(int dayId) => new()
	{
		Id = dayId,
		Date = DateTimeOffset.UtcNow,
	};

	[Fact]
	public async Task EmptyStringUserIdReturnsEmptyList()
	{
		var svc = new ResultSvc(DbContext);

		var user = GetUser("User Name");
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		var result = new Result
		{
			DayId = day.Id,
			UserId = user.Id,
			HardMode = false,
			Guesses = new[]
			{
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Correct, 5).ToImmutableList() }
			}.ToImmutableList()
		};
		DbContext.Add(result);

		await DbContext.SaveChangesAsync();

		Assert.Empty(svc.GetResultsForUser(string.Empty));
	}

	[Fact]
	public async Task NonExistentUserReturnsEmptyList()
	{
		var svc = new ResultSvc(DbContext);

		var user = GetUser("User Name");
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		var result = new Result
		{
			DayId = day.Id,
			UserId = user.Id,
			HardMode = false,
			Guesses = new[]
			{
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Correct, 5).ToImmutableList() }
			}.ToImmutableList()
		};
		DbContext.Add(result);

		await DbContext.SaveChangesAsync();

		Assert.Empty(svc.GetResultsForUser("Fake Name"));
	}

	[Fact]
	public async Task UserWithNoResultsReturnsEmptyList()
	{
		var svc = new ResultSvc(DbContext);

		var user = GetUser("User Name");
		DbContext.Add(user);

		await DbContext.SaveChangesAsync();

		Assert.Empty(svc.GetResultsForUser(user.Id));
	}

	[Fact]
	public async Task UserWithResultsReturnsAllResults()
	{
		var svc = new ResultSvc(DbContext);

		var user = GetUser("User Name");
		var otherUser = GetUser("Second User");
		DbContext.Add(user);
		DbContext.Add(otherUser);

		DbContext.Add(GetDay(1));
		DbContext.Add(GetDay(5));
		DbContext.Add(GetDay(10));

		var result1 = new Result
		{
			DayId = 1,
			UserId = user.Id,
			HardMode = false,
			Guesses = new[]
			{
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Correct, 5).ToImmutableList() }
			}.ToImmutableList()
		};

		var result2 = new Result
		{
			DayId = 5,
			UserId = user.Id,
			HardMode = false,
			Guesses = new[]
			{
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Missing, 5).ToImmutableList() },
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Correct, 5).ToImmutableList() }
			}.ToImmutableList()
		};

		var result3 = new Result
		{
			DayId = 10,
			UserId = user.Id,
			HardMode = false,
			Guesses = new[]
			{
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Missing, 5).ToImmutableList() },
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Missing, 5).ToImmutableList() },
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Missing, 5).ToImmutableList() },
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Missing, 5).ToImmutableList() },
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Misplaced, 5).ToImmutableList() },
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Misplaced, 5).ToImmutableList() }
			}.ToImmutableList()
		};

		var result4 = new Result
		{
			DayId = 5,
			UserId = otherUser.Id,
			HardMode = false,
			Guesses = new[]
			{
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Missing, 5).ToImmutableList() },
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Misplaced, 5).ToImmutableList() },
				new WordGuess { Guesses = Enumerable.Repeat(LetterGuess.Correct, 5).ToImmutableList() }
			}.ToImmutableList()
		};

		DbContext.Add(result1);
		DbContext.Add(result2);
		DbContext.Add(result3);
		DbContext.Add(result4);

		await DbContext.SaveChangesAsync();

		var results = await svc.GetResultsForUser(user.Id).ToListAsync();

		Assert.Equal(3, results.Count);
		Assert.True(results.Any(result => result.DayId == 1), "Results include day 1");
		Assert.True(results.Any(result => result.DayId == 5), "Results include day 5");
		Assert.True(results.Any(result => result.DayId == 10), "Results include day 10");
	}
}
