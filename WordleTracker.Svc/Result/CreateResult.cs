using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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

	public async Task<ImmutableList<Result>> CreateResults(string userId, string sharedResults, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(userId))
		{
			throw new ArgumentException("User id cannot be empty or whitespace", nameof(userId));
		}

		var existingResults = (await DbContext.Results
			.AsNoTracking()
			.Where(result => result.UserId == userId)
			.ToListAsync(cancellationToken))
			.ToHashSet();

		var results = ResultParser
			.ParseAll(sharedResults)
			.Except(existingResults, new DayComparer())
			.ToImmutableList();
		results.ForEach(AssignUserId);

		DbContext.AddRange(results);
		await DbContext.SaveChangesAsync(cancellationToken);

		results.ForEach(Detach);

		return results;

		#region Helpers

		void AssignUserId(Result result) => result.UserId = userId;
		void Detach(Result result) => DbContext.Entry(result).State = EntityState.Detached;

		#endregion Helpers
	}

	private class DayComparer : IEqualityComparer<Result>
	{
		public bool Equals(Result? x, Result? y) => x?.DayId == y?.DayId;

		public int GetHashCode([DisallowNull] Result obj) => obj.DayId;
	}
}
