using WordleTracker.Data;

namespace WordleTracker.Svc;

public partial class UserSvc : BaseSvc
{
	public UserSvc(WordleTrackerContext dbContext) : base(dbContext)
	{
	}
}

