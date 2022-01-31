using System.ComponentModel.DataAnnotations.Schema;

namespace WordleTracker.Data.Models;

public class Day
{
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public int Id { get; set; }
	public DateTimeOffset Date { get; set; }
	public string? Word { get; set; }

	public List<Result> Results { get; set; } = null!;
}
