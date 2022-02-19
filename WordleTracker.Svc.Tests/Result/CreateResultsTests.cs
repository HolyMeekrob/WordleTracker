using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using Xunit;

namespace WordleTracker.Svc.Tests;

public class CreateResultsTests : DbTests
{
	private readonly string[] ValidShare = new[]
	{
		"⬛⬛⬛⬛⬛",
		"⬛⬛⬛🟨🟨",
		"🟨⬛⬛⬛🟨",
		"⬛🟨🟨⬛🟨",
		"⬛🟩🟩🟩🟩",
		"🟩🟩🟩🟩🟩"
	};

	private string GetValidInput(int numGuesses, int day) =>
@$"Wordle {day} {numGuesses}/6

{string.Join(Environment.NewLine, ValidShare.Skip(6 - numGuesses))}";

	private string GetValidInputs(params (int NumGuesses, int Day)[] specs) =>
		string.Join(Environment.NewLine, specs.Select(spec => GetValidInput(spec.NumGuesses, spec.Day)));

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
		var dayId = 1;
		var day = GetDay(dayId);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.CreateResults(string.Empty, GetValidInput(6, dayId), new CancellationToken())
		);
	}

	[Theory]
	[InlineData(" ")]
	[InlineData("\t")]
	public async Task WhitespaceUserIdThrowsAnException(string userId)
	{
		var dayId = 1;
		var day = GetDay(dayId);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<ArgumentException>(() =>
			svc.CreateResults(userId, GetValidInput(6, dayId), new CancellationToken())
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

		var results = await svc.CreateResults(userId, string.Empty, new CancellationToken());
		Assert.Empty(results);
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

		var results = await svc.CreateResults(userId, share, new CancellationToken());

		Assert.Empty(results);
	}

	[Fact]
	public async Task NoGuessesThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} 0/6";

		var results = await svc.CreateResults(userId, share, new CancellationToken());

		Assert.Empty(results);
	}

	[Fact]
	public async Task SevenGuessesThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} 7/6

⬛⬛⬛⬛⬛
⬛⬛⬛⬛⬛
⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		var results = await svc.CreateResults(userId, share, new CancellationToken());
		Assert.Empty(results);
	}

	[Fact]
	public async Task ADenominatorOtherThanSixThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} 5/5

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		var results = await svc.CreateResults(userId, share, new CancellationToken());

		Assert.Empty(results);
	}

	[Fact]
	public async Task FourLetterGuessThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResults(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task SixLetterGuessThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨⬛
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResults(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task AnIncorrectFinalGuessOnACorrectResultThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} 4/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResults(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task NumberOfGuessesNotMatchingCountThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} 4/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResults(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task CorrectGuessPriorToLastGuessThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
🟩🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResults(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task IncorrectResultWithCorrectGuessThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 123;
		var day = GetDay(123);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var share = @$"Wordle {dayId} X/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		await Assert.ThrowsAsync<FormatException>(() =>
			svc.CreateResults(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task MissingDayThrowsAnException()
	{
		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		await DbContext.SaveChangesAsync();

		var share = GetValidInput(6, 1);

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<DbUpdateException>(() =>
			svc.CreateResults(userId, share, new CancellationToken())
		);
	}

	[Fact]
	public async Task MissingUserThrowsAnException()
	{
		var dayId = 1;
		var day = GetDay(dayId);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var share = GetValidInput(6, dayId);

		var svc = new ResultSvc(DbContext);

		await Assert.ThrowsAsync<DbUpdateException>(() =>
			svc.CreateResults("User Id", share, new CancellationToken())
		);
	}

	[Fact]
	public async Task ValidIncorrectResultInsertsTheResult()
	{
		var svc = new ResultSvc(DbContext);
		var userId = "User Id";
		var dayId = 123;
		var share = @$"Wordle {dayId} X/6

⬛⬛⬛⬛⬛
⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
⬛🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(dayId);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResults(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.First().UserId, result.First().DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(dayId, dbResult!.DayId);
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
		var dayId = 123;
		var share = @$"Wordle {123} X/6*

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

		var result = await svc.CreateResults(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.First().UserId, result.First().DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(dayId, dbResult!.DayId);
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
		var dayId = 123;
		var share = @$"Wordle {dayId} 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(dayId);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResults(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.First().UserId, result.First().DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(dayId, dbResult!.DayId);
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
		var dayId = 99;
		var share = @$"Wordle {dayId} 1/6

🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(dayId);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResults(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.First().UserId, result.First().DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(dayId, dbResult!.DayId);
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
		var dayId = 123;
		var share = @$"Wordle {dayId} 5/6*

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(dayId);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = await svc.CreateResults(userId, share, new CancellationToken());
		var dbResult = await DbContext.FindAsync<Result>(result.First().UserId, result.First().DayId);

		Assert.NotNull(dbResult);
		Assert.Equal(dayId, dbResult!.DayId);
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
		var dayId = 123;
		var share = @$"Wordle {dayId} 5/6

⬛⬛⬛🟨🟨
🟨⬛⬛⬛🟨
⬛🟨🟨⬛🟨
⬛🟩🟩🟩🟩
🟩🟩🟩🟩🟩";

		var user = GetUser(userId);
		DbContext.Add(user);

		var day = GetDay(dayId);
		DbContext.Add(day);

		await DbContext.SaveChangesAsync();

		var result = (await svc.CreateResults(userId, share, new CancellationToken())).First();

		Assert.NotNull(result);
		Assert.Equal(dayId, result.DayId);
		Assert.Equal(userId, result.UserId);
		Assert.Equal(5, result.Guesses.Count);
		Assert.True(result.IsSolved);
		Assert.False(result.HardMode);
	}

	[Fact]
	public async Task SuccessfulResultsAreReturned()
	{
		var token = new CancellationToken();

		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 24;
		DbContext.AddRange(GetDay(dayId - 1), GetDay(dayId), GetDay(dayId + 1));

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		var results = await svc.CreateResults(userId, GetValidInputs((3, dayId - 1), (4, dayId), (5, dayId + 1)), token);

		Assert.Equal(3, results.Count);
	}

	[Fact]
	public async Task DuplicateDaysAreNotReturned()
	{
		var token = new CancellationToken();

		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 24;
		var day = GetDay(dayId);
		DbContext.AddRange(GetDay(dayId - 1), GetDay(dayId), GetDay(dayId + 1));

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await svc.CreateResult(userId, GetValidInput(6, dayId), token);

		var results = await svc.CreateResults(userId, GetValidInputs((3, dayId - 1), (4, dayId), (5, dayId + 1)), token);

		var dbResult = await DbContext.Results.FirstOrDefaultAsync(result => result.DayId == dayId && result.UserId == userId);

		Assert.Equal(2, results.Count);
		Assert.Equal(6, dbResult!.Guesses.Count);
	}

	[Fact]
	public async Task SuccessfulResultsAreInserted()
	{
		var token = new CancellationToken();

		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 24;
		var day = GetDay(dayId);
		DbContext.AddRange(GetDay(dayId - 1), GetDay(dayId), GetDay(dayId + 1));

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await svc.CreateResults(userId, GetValidInputs((3, dayId - 1), (4, dayId), (5, dayId + 1)), token);

		var results = await DbContext.Results
			.Where(result => result.UserId == userId)
			.ToListAsync();

		Assert.Equal(3, results.Count);
	}


	[Fact]
	public async Task DuplicateDaysAreNotInserted()
	{
		var token = new CancellationToken();

		var userId = "User Id";
		var user = GetUser(userId);
		DbContext.Add(user);

		var dayId = 24;
		var day = GetDay(dayId);
		DbContext.AddRange(GetDay(dayId - 1), GetDay(dayId), GetDay(dayId + 1));

		await DbContext.SaveChangesAsync();

		var svc = new ResultSvc(DbContext);

		await svc.CreateResult(userId, GetValidInput(6, dayId), token);

		await svc.CreateResults(userId, GetValidInputs((3, dayId - 1), (4, dayId), (5, dayId + 1)), token);

		var result = await DbContext.Results
			.Where(result => result.UserId == userId && result.DayId == dayId)
			.FirstAsync();

		Assert.Equal(6, result.Guesses.Count);
	}
}
