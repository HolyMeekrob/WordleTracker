using Microsoft.Extensions.Options;
using WordleTracker.Core.Configuration;
using WordleTracker.Data.Models;
using static WordleTracker.Core.Configuration.DaysOptions;

namespace WordleTracker.Data
{
	/// <summary>
	/// Initializes a new database. Generally this means seeding with data.
	/// </summary>
	public static class DatabaseInitializer
	{
		public static void Initialize(WordleTrackerContext dbContext, IOptions<DaysOptions> config)
		{
			// Database has already been initialized
			if (dbContext.Days.Any())
			{
				return;
			}

			var days = config.Value.Words.Select(GetDay);

			dbContext.Days.AddRange(days);
			dbContext.SaveChanges();
		}

		private static Day GetDay(DailyWord word)
		{
			var date = DateTime.SpecifyKind(DateTime.Parse(word.Date), DateTimeKind.Utc);
			return new()
			{
				Id = word.Id,
				Word = word.Word,
				Date = new DateTimeOffset(date)
			};
		}
	}
}
