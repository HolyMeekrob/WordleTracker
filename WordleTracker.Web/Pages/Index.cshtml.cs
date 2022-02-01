using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Svc;
using static WordleTracker.Web.Utilities.Identity;

namespace WordleTracker.Web.Pages;

public class IndexModel : PageModel
{
	private readonly ILogger<IndexModel> _logger;
	private readonly ResultSvc _resultSvc;

	// The formatted string from the Wordle share feature
	[BindProperty]
	[RegularExpression(ResultParser.SharePattern)]
	[Required]
	public string Share { get; set; } = string.Empty;

	public string SuccessMessage { get; set; } = string.Empty;

	public IndexModel(ILogger<IndexModel> logger, ResultSvc resultSvc)
	{
		_logger = logger;
		_resultSvc = resultSvc;
	}

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			try
			{
				var result = await _resultSvc.CreateResult(GetUserId(User), Share, cancellationToken);
				SuccessMessage = $"Day {result.DayId} saved successfully";
			}
			catch (DbUpdateException)
			{
				ModelState.TryAddModelError(nameof(Share), "Result already exists");
			}
		}

		return Page();
	}
}
