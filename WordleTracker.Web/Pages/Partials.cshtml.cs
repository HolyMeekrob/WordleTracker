using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WordleTracker.Svc;
using WordleTracker.Web.Pages.Shared.Partials;

namespace WordleTracker.Web.Pages;

public partial class PartialsModel : PageModel
{
	private readonly UserSvc _userSvc;

	public PartialsModel(UserSvc userSvc)
	{
		_userSvc = userSvc;
	}

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPostUpdateNameAsync(UserNameModel model, CancellationToken cancellationToken) =>
		await UpdateNameAsync(model, cancellationToken);
}
