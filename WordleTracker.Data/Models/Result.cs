using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using static WordleTracker.Core.Utilities.Functional;

namespace WordleTracker.Data.Models;

public record Result
{
	public const string SharePattern =
		@"Wordle (?<id>\d+) (?<success>[1-6X])/6(?<newline>\r|\n|\r\n){2}(?:(?<nonfinal>[⬛🟨🟩]{5,10})\k<newline>){0,5}(?<final>[⬛🟨🟩]{5,10})";
	public int Id { get; init; }
	public ImmutableList<WordGuess> Guesses { get; init; } = ImmutableList<WordGuess>.Empty;
	public bool IsSolved { get; set; }

	private static readonly Regex s_shareRegex = new(SharePattern, RegexOptions.Compiled);

	public static Result Parse(string sharedResult)
	{
		var result = s_shareRegex.Match(sharedResult);

		if (!result.Success)
		{
			throw new FormatException("Wordle results do not match expected format");
		}

		return new Result
		{
			Id = int.Parse(result.Groups["id"].Value),
			IsSolved = int.TryParse(result.Groups["success"].Value, out var _),
			Guesses = result.Groups["nonfinal"].Captures.Select(capture => capture.ToString())
				.Append(result.Groups["final"].Value)
				.Select(WordGuess.Parse)
				.ToImmutableList()
		};
	}
}

public record WordGuess
{
	private static readonly Dictionary<string, LetterGuess> LetterGuessMap = new()
	{
		{ "⬛", LetterGuess.Missing },
		{ "🟨", LetterGuess.Misplaced },
		{ "🟩", LetterGuess.Correct }
	};

	public ImmutableList<LetterGuess> Guesses { get; init; } = ImmutableList<LetterGuess>.Empty;

	public static WordGuess Parse(string sharedGuess)
	{
		#region Validation functions

		var isCorrectLength = (IEnumerable<Rune> guess) => guess.Count() == 5;
		var hasCorrectCharacters = (IEnumerable<Rune> guess) => guess.All(rune => LetterGuessMap.Keys.Contains(rune.ToString()));

		var isInvalid = AnyFail(isCorrectLength, hasCorrectCharacters);

		#endregion Validation functions

		var runes = sharedGuess.EnumerateRunes().ToList();

		if (isInvalid(runes))
		{
			throw new FormatException("Wordle guess does not match expected format");
		}

		return new()
		{
			Guesses = runes.Select(rune => LetterGuessMap[rune.ToString()]).ToImmutableList()
		};
	}
}

public enum LetterGuess
{
	Missing,
	Misplaced,
	Correct
}
