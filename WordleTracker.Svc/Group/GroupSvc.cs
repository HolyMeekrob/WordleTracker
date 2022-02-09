using WordleTracker.Data;

namespace WordleTracker.Svc;

public partial class GroupSvc : BaseSvc
{
	public GroupSvc(WordleTrackerContext dbContext) : base(dbContext)
	{
	}
}
