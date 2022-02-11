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

	public ILookup<GroupRole, Group> Groups { get; set; } = null!;

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

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		var userId = GetUserId(User);
		Groups = (await _groupSvc
			.GetGroupsForUser(userId)
			.Include(group => group.Memberships)
			.ToListAsync(cancellationToken))
			.ToLookup(group => group.Memberships
				.First(member => member.UserId == userId)
				.Role
			);

		return Page();
	}

	public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var group = await _groupSvc.CreateGroup(Name, GetUserId(User), cancellationToken);
		return RedirectToPage("Group", new { id = group.Id });
	}
}
