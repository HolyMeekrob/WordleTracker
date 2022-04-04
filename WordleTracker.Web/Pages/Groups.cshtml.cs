using System.ComponentModel.DataAnnotations;
using HashidsNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WordleTracker.Data.Models;
using WordleTracker.Svc;
using static WordleTracker.Web.Utilities.Identity;

namespace WordleTracker.Web.Pages;

public class GroupsModel : PageModel
{
	private readonly ILogger<GroupsModel> _logger;
	private readonly IHashids _hashids;
	private readonly GroupSvc _groupSvc;

	public ILookup<GroupRole, GroupInfo> Groups { get; set; } = null!;

	[BindProperty]
	[Required(ErrorMessage = "Group name cannot be blank")]
	[MaxLength(20, ErrorMessage = "Group name must not be longer than 20 characters")]
	[Display(Name = "New group")]
	public string Name { get; set; } = null!;

	public GroupsModel(ILogger<GroupsModel> logger, IHashids hashids, GroupSvc groupSvc)
	{
		_logger = logger;
		_hashids = hashids;
		_groupSvc = groupSvc;
	}

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		var userId = GetUserId(User);

		Groups = (await _groupSvc
			.GetGroupList(userId, cancellationToken))
			.ToLookup(group => group.Role, group => new GroupInfo(_hashids.Encode(group.Id), group.Name, group.Size));

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

	public record GroupInfo(string Id, string Name, int Size);
}
