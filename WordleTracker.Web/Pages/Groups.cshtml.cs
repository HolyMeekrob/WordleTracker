using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WordleTracker.Data.Models;
using WordleTracker.Svc;
using static WordleTracker.Web.Utilities.Identity;

namespace WordleTracker.Web.Pages;

public class GroupsModel : PageModel
{
	private readonly ILogger<GroupsModel> _logger;
	private readonly GroupSvc _groupSvc;

	public List<Group> Groups { get; set; } = null!;

	[BindProperty]
	[Required(ErrorMessage = "Group name cannot be blank")]
	[MaxLength(20, ErrorMessage = "Group name must not be longer than 20 characters")]
	[Display(Name = "New group")]
	public string Name { get; set; } = null!;

	public GroupsModel(ILogger<GroupsModel> logger, GroupSvc groupSvc)
	{
		_logger = logger;
		_groupSvc = groupSvc;
	}

	private async Task<List<Group>> GetGroups(CancellationToken cancellationToken) =>
		await _groupSvc
			.GetGroupsForUser(GetUserId(User))
			.Include(group => group.Members)
			.ToListAsync(cancellationToken);

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		Groups = await GetGroups(cancellationToken);
		return Page();
	}

	public async Task<IActionResult> OnPostUpdateGroupNameAsync(CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var group = await _groupSvc.CreateGroup(Name, GetUserId(User), cancellationToken);

		ModelState.Clear();
		// TODO: Redirect to group page
		Name = string.Empty;
		Groups = await GetGroups(cancellationToken);

		return Page();
	}
}
