using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace WordleTracker.Data.Models;

public class Result : ITrackCreation
{
	public string UserId { get; set; } = null!;
	public int DayId { get; set; }
	public bool HardMode { get; set; }
	public DateTimeOffset CreatedDate { get; set; }
	public ImmutableList<WordGuess> Guesses { get; set; } = null!;
	public User User { get; set; } = null!;
	public Day Day { get; set; } = null!;

	public bool IsSolved => Guesses.Last().IsCorrect;
}

public class WordGuess
{
	public ImmutableList<LetterGuess> Guesses { get; set; } = null!;

	[JsonIgnore]
	public bool IsCorrect => Guesses.All(letter => letter == LetterGuess.Correct);
}

public enum LetterGuess
{
	Missing,
	Misplaced,
	Correct
}
