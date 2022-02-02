using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using WordleTracker.Svc;
using static WordleTracker.Web.Utilities.Identity;

namespace WordleTracker.Web.Pages;

public class ResultsModel : PageModel
{
	private readonly ILogger<ResultsModel> _logger;
	private readonly ResultSvc _resultSvc;

	public List<Result> Results { get; set; } = null!;

	public ResultsModel(ILogger<ResultsModel> logger, ResultSvc resultSvc)
	{
		_logger = logger;
		_resultSvc = resultSvc;
	}

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		Results = await _resultSvc
			.GetResultsForUser(GetUserId(User))
			.OrderBy(result => result.DayId)
			.ToListAsync(cancellationToken);

		return Page();
	}
}
