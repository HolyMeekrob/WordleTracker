using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using WordleTracker.Data.Models;
using static WordleTracker.Core.Utilities.Functional;

namespace WordleTracker.Svc;

public static class ResultParser
{
	public const string SharePattern =
		@"(?:(?<results>Wordle (?<id>\d+) (?<success>[1-6X])/6(?<hardmode>\*?)(?<newline>\r|\n|\r\n){2}(?:(?<nonfinal>[⬛🟨🟩]{5,10})\k<newline>){0,5}(?<final>[⬛🟨🟩]{5,10}))\k<newline>*)+";

	private static readonly Regex s_shareRegex = new(SharePattern, RegexOptions.Compiled);
	private static readonly Dictionary<string, LetterGuess> s_letterGuessMap = new()
	{
		{ "⬛", LetterGuess.Missing },
		{ "🟨", LetterGuess.Misplaced },
		{ "🟩", LetterGuess.Correct }
	};

	public static ImmutableList<Result> ParseAll(string sharedResults) =>
		s_shareRegex
			.Match(sharedResults)
			.Groups["results"]
			.Captures
			.Select(Parse)
			.ToImmutableList();

	private static Result Parse(Capture result) => Parse(result.Value);
	public static Result Parse(string sharedResult)
	{
		var result = s_shareRegex.Match(sharedResult);

		if (!result.Success)
		{
			throw new FormatException("Wordle results do not match expected format");
		}

		var hasSuccessDigit = int.TryParse(result.Groups["success"].Value, out var expectedGuesses);
		expectedGuesses = hasSuccessDigit ? expectedGuesses : 6;

		var guesses = result
			.Groups["nonfinal"]
			.Captures
			.Select(capture => capture.ToString())
			.Append(result.Groups["final"].Value)
			.Select(ParseWord)
			.ToImmutableList();

		if (expectedGuesses != guesses.Count)
		{
			throw new FormatException("Number of guesses does not match indicated count");
		}

		if (guesses.Take(guesses.Count - 1).Any(guess => guess.IsCorrect))
		{
			throw new FormatException("Only the last guess can be correct");
		}

		if (hasSuccessDigit && !guesses.Last().IsCorrect)
		{
			throw new FormatException("Final guess must be correct");
		}

		if (!hasSuccessDigit && guesses.Any(guess => guess.IsCorrect))
		{
			throw new FormatException("No guesses must be correct");
		}

		return new Result
		{
			DayId = int.Parse(result.Groups["id"].Value),
			HardMode = result.Groups["hardmode"].Value == "*",
			Guesses = result
				.Groups["nonfinal"]
				.Captures
				.Select(capture => capture.ToString())
				.Append(result.Groups["final"].Value)
				.Select(ParseWord)
				.ToImmutableList()
		};
	}

	private static WordGuess ParseWord(string sharedGuess)
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

		return new()
		{
			Guesses = runes.Select(rune => s_letterGuessMap[rune.ToString()]).ToImmutableList()
		};
	}
}
