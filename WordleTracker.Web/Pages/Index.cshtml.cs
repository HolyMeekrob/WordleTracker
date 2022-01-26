using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WordleTracker.Data.Models;
using WordleTracker.Svc;

namespace WordleTracker.Web.Pages;

public class IndexModel : PageModel
{
	private readonly ILogger<IndexModel> _logger;
	private readonly UserSvc _userSvc;

	// The formatted string from the Wordle share feature
	[BindProperty]
	[RegularExpression(Result.SharePattern)]
	[Required]
	public string Share { get; set; } = string.Empty;

	public string SuccessMessage { get; set; } = string.Empty;

	public IndexModel(ILogger<IndexModel> logger, UserSvc userSvc)
	{
		_logger = logger;
		_userSvc = userSvc;
	}

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (!ModelState.IsValid)
		{
			return await Task.FromResult<IActionResult>(Page());
		}
		else
		{
			SuccessMessage = "Hooray!";
			var result = Result.Parse(User.Identity!.Name!, Share);
			return await Task.FromResult<IActionResult>(Page());
		}
	}
}
