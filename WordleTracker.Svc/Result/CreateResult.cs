using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class ResultSvc
{
	public async Task<Result> CreateResult(string userId, string sharedResult, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(userId))
		{
			throw new ArgumentException("User id cannot be empty or whitespace", nameof(userId));
		}

		var result = ResultParser.Parse(sharedResult);
		result.UserId = userId;

		DbContext.Add(result);
		await DbContext.SaveChangesAsync(cancellationToken);

		DbContext.Entry(result).State = EntityState.Detached;

		return result;
	}
}
