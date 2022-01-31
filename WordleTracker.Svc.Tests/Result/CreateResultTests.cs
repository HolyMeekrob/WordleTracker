using System;
using System.Threading;
using System.Threading.Tasks;
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

	private static User GetUser(string userId) =>
		new()
		{
			Id = userId,
			Name = userId,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		};

	private static Day GetDay(int dayId) =>
		new()
		{
			Id = dayId,
			Date = DateTimeOffset.UtcNow,
		};

	[Fact]
	public async Task EmptyUserIdThrowsAnException()
	{
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
		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.CreateResult(userId, SampleValidInput, new CancellationToken())
		);
	}

	[Fact]
	public async Task EmptyShareThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", string.Empty, new CancellationToken())
		);
	}

	[Theory]
	[InlineData(" ")]
	[InlineData("\t")]
	public async Task WhitespaceShareThrowsAnException(string share)
	{
		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task NoGuessesThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 0/6";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task SevenGuessesThrowsAnException()
	{
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
			svc.CreateResult("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task ADenominatorOtherThanSixThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 5/5

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
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
		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨⬛
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task AnIncorrectFinalGuessOnACorrectResultThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 4/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task NumberOfGuessesNotMatchingCountThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 4/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task CorrectGuessPriorToLastGuessThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
🟩🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task IncorrectResultWithCorrectGuessThrowsAnException()
	{
		var svc = new ResultSvc(DbContext);

		var share = @"Wordle 123 X/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResult("User Id", share, new CancellationToken())
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
