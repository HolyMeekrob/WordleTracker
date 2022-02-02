using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class ResultSvc
{
	public IQueryable<Result> GetResultsForUser(string userId) =>
		DbContext.Results.Where(result => result.UserId == userId);
}
