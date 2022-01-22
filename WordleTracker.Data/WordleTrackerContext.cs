using Microsoft.EntityFrameworkCore;

namespace WordleTracker.Data
{
	public class WordleTrackerContext : DbContext
	{
		public WordleTrackerContext(DbContextOptions options) : base(options)
		{
		}
	}
}
