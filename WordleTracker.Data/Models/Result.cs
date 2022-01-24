using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static WordleTracker.Core.Utilities.Functional;

namespace WordleTracker.Data.Models;

public class Result
{
	#region Properties
	public string UserId { get; set; } = null!;
	public int DayId { get; set; }
	public DateTimeOffset CreatedDate { get; set; }
	public ImmutableList<WordGuess> Guesses { get; set; } = null!;
	public User User { get; set; } = null!;
	public Day Day { get; set; } = null!;

	public bool IsSolved => IsCorrect(Guesses.Last());

	#endregion Properties

	#region Share parsing

	public const string SharePattern =
		@"Wordle (?<id>\d+) (?<success>[1-6X])/6(?<newline>\r|\n|\r\n){2}(?:(?<nonfinal>[⬛🟨🟩]{5,10})\k<newline>){0,5}(?<final>[⬛🟨🟩]{5,10})";

	private static readonly Regex s_shareRegex = new(SharePattern, RegexOptions.Compiled);

	public static Result Parse(string userId, string sharedResult)
	{
		var result = s_shareRegex.Match(sharedResult);

		if (!result.Success)
		{
			throw new FormatException("Wordle results do not match expected format");
		}

		var hasSuccessDigit = int.TryParse(result.Groups["success"].Value, out var expectedGuesses);
		expectedGuesses = hasSuccessDigit ? expectedGuesses : 6;

		if (expectedGuesses != result.Groups["nonfinal"].Captures.Count() + 1)
		{
			throw new FormatException("Number of guesses does not match indicated count");
		}

		return new Result()
		{
			UserId = userId,
			DayId = int.Parse(result.Groups["id"].Value),
			CreatedDate = DateTimeOffset.UtcNow,
			Guesses = result
				.Groups["nonfinal"]
				.Captures
				.Select(capture => capture.ToString())
				.Append(result.Groups["final"].Value)
				.Select(WordGuess.Parse)
				.ToImmutableList()
		};
	}

	#endregion Share parsing

	#region Helpers

	private static bool IsLetterCorrect(LetterGuess guess) => guess == LetterGuess.Correct;
	private static bool IsCorrect(WordGuess guess) => guess.Guesses.All(IsLetterCorrect);

	#endregion Helpers
}

public class WordGuess
{
	private static readonly Dictionary<string, LetterGuess> s_letterGuessMap = new()
	{
		{ "⬛", LetterGuess.Missing },
		{ "🟨", LetterGuess.Misplaced },
		{ "🟩", LetterGuess.Correct }
	};

	public ImmutableList<LetterGuess> Guesses { get; }

	[JsonConstructor]
	public WordGuess(IEnumerable<LetterGuess> guesses)
	{
		Guesses = guesses.ToImmutableList();
	}

	public static WordGuess Parse(string sharedGuess)
	{
		#region Validation functions

		var isCorrectLength = (IEnumerable<Rune> guess) => guess.Count() == 5;
		var hasCorrectCharacters = (IEnumerable<Rune> guess) => guess.All(rune => s_letterGuessMap.Keys.Contains(rune.ToString()));

		var isInvalid = AnyFail(isCorrectLength, hasCorrectCharacters);

		#endregion Validation functions

		var runes = sharedGuess.EnumerateRunes().ToList();

		if (isInvalid(runes))
		{
			throw new FormatException("Wordle guess does not match expected format");
		}

		return new(
			runes.Select(rune => s_letterGuessMap[rune.ToString()]).ToImmutableList()
		);
	}
}

public enum LetterGuess
{
	Missing,
	Misplaced,
	Correct
}
