using WordleTracker.Data;

namespace WordleTracker.Svc;

public abstract class BaseSvc
{
	protected readonly WordleTrackerContext DbContext;

	public BaseSvc(WordleTrackerContext dbContext)
	{
		DbContext = dbContext;
	}
}
