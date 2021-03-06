using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class CreateResultTests : DbTests
{
	private const string SampleValidInput = @"Wordle 123 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

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
	public async Task EmptyUserIdThrowsAnException()
	{
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.CreateResult(string.Empty, SampleValidInput, new CancellationToken())
		);
	}

	[Theory]
	[InlineData(" ")]
	[InlineData("\t")]
	public async Task WhitespaceUserIdThrowsAnException(string userId)
	{
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.CreateResult(userId, SampleValidInput, new CancellationToken())
		);
	}

	[Fact]
	public async Task EmptyShareThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, string.Empty, new CancellationToken())
		);
	}

	[Theory]
	[InlineData(" ")]
	[InlineData("\t")]
	public async Task WhitespaceShareThrowsAnException(string share)
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task MissingUserThrowsAnException()
	{
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<DbUpdateException>(() =>
			svc.CreateResult("User Id", SampleValidInput, new CancellationToken())
		);
	}

	[Fact]
	public async Task MissingDayThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<DbUpdateException>(() =>
			svc.CreateResult(userId, SampleValidInput, new CancellationToken())
		);
	}

	[Fact]
	public async Task NoGuessesThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 0/6";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task SevenGuessesThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 7/6

⬛⬛⬛⬛⬛
⬛⬛⬛⬛⬛
⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task ADenominatorOtherThanSixThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 5/5

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task FourLetterGuessThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task SixLetterGuessThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨⬛
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task AnIncorrectFinalGuessOnACorrectResultThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 4/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task NumberOfGuessesNotMatchingCountThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 4/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task CorrectGuessPriorToLastGuessThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
🟩🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task IncorrectResultWithCorrectGuessThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 X/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task DuplicateDayForUserThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);
		var userId = "User Id";

		var shareOne = @"Wordle 123 X/6

⬛⬛⬛⬛⬛
⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
⬛🟩🟩🟩🟩";

		var shareTwo = @"Wordle 123 2/6

⬛⬛⬛🟨🟨
🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		var token = new CancellationToken();

		await DbContext.SaveChangesAsync();

		await svc.CreateResult(userId, shareOne, token);

		await Assert.ThrowsAsync<DbUpdateException>(() =>
			svc.CreateResult(userId, shareTwo, token)
		);
	}

	[Fact]
	public async Task ValidIncorrectResultInsertsTheResult()
	{
		var svc = new ResultSvc(DbContext);
		var userId = "User Id";
		var share = @"Wordle 123 X/6

⬛⬛⬛⬛⬛
⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
⬛🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResult(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.UserId, result.DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(123, dbResult!.DayId);
		Assert.Equal(userId, dbResult.UserId);
		Assert.Equal(6, dbResult.Guesses.Count);
		Assert.False(dbResult.IsSolved);
		Assert.False(dbResult.HardMode);
	}

	[Fact]
	public async Task ValidIncorrectHardModeResultInsertsTheResult()
	{
		var svc = new ResultSvc(DbContext);
		var userId = "User Id";
		var share = @"Wordle 123 X/6*

⬛⬛⬛⬛⬛
⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
⬛🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResult(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.UserId, result.DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(123, dbResult!.DayId);
		Assert.Equal(userId, dbResult.UserId);
		Assert.Equal(6, dbResult.Guesses.Count);
		Assert.False(dbResult.IsSolved);
		Assert.True(dbResult.HardMode);
	}

	[Fact]
	public async Task ValidMultipleGuessCorrectResultInsertsTheResult()
	{
		var svc = new ResultSvc(DbContext);
		var userId = "User Id";
		var share = @"Wordle 123 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResult(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.UserId, result.DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(123, dbResult!.DayId);
		Assert.Equal(userId, dbResult.UserId);
		Assert.Equal(5, dbResult.Guesses.Count);
		Assert.True(dbResult.IsSolved);
		Assert.False(dbResult.HardMode);
	}

	[Fact]
	public async Task ValidSingleGuessCorrectResultInsertsTheResult()
	{
		var svc = new ResultSvc(DbContext);
		var userId = "User Id";
		var share = @"Wordle 99 1/6

🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(99);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResult(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.UserId, result.DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(99, dbResult!.DayId);
		Assert.Equal(userId, dbResult.UserId);
		Assert.Single(dbResult.Guesses);
		Assert.True(dbResult.IsSolved);
		Assert.False(dbResult.HardMode);
	}

	[Fact]
	public async Task ValidHardModeCorrectResultInsertsTheResult()
	{
		var svc = new ResultSvc(DbContext);
		var userId = "User Id";
		var share = @"Wordle 123 5/6*

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResult(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.UserId, result.DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(123, dbResult!.DayId);
		Assert.Equal(userId, dbResult.UserId);
		Assert.Equal(5, dbResult.Guesses.Count);
		Assert.True(dbResult.IsSolved);
		Assert.True(dbResult.HardMode);
	}

	[Fact]
	public async Task ValidMultipleGuessCorrectResultReturnsTheResult()
	{
		var svc = new ResultSvc(DbContext);
		var userId = "User Id";
		var share = @"Wordle 123 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResult(userId, share, new CancellationToken());

		Assert.NotNull(result);
		Assert.Equal(123, result!.DayId);
		Assert.Equal(userId, result.UserId);
		Assert.Equal(5, result.Guesses.Count);
		Assert.True(result.IsSolved);
		Assert.False(result.HardMode);
	}
}
