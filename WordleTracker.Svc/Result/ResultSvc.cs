using WordleTracker.Data;

namespace WordleTracker.Svc;

public partial class ResultSvc : BaseSvc
{
	public ResultSvc(WordleTrackerContext dbContext) : base(dbContext)
	{
	}
}
