using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WordleTracker.Data.Models;
using WordleTracker.Svc;
using static WordleTracker.Web.Utilities.Identity;

namespace WordleTracker.Web.Pages;

public class GroupModel : PageModel
{
	private readonly ILogger<GroupModel> _logger;
	private readonly GroupSvc _groupSvc;

	[BindProperty]
	public int Id { get; set; }
	[BindProperty]
	public string Name { get; set; } = null!;
	public List<Member> Members { get; set; } = null!;

	[BindProperty]
	public string NewMemberId { get; set; } = null!;

	private Member? Me => Members.FirstOrDefault(member => member.Id == GetUserId(User));
	public List<GroupRole> AssignableRoles =>
		Enum.GetValues<GroupRole>()
		.Where(role => role < Me!.Role)
		.ToList();

	public bool CanEditName => Me!.Role == GroupRole.Owner;

	public GroupModel(ILogger<GroupModel> logger, GroupSvc groupSvc)
	{
		_logger = logger;
		_groupSvc = groupSvc;
	}

	public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
	{
		var group = await _groupSvc.GetGroupById(id, cancellationToken);

		if (group == null)
		{
			return NotFound();
		}

		Id = id;
		Name = group.Name;
		Members = group.Memberships.Select(Member.Create).ToList();

		return Me is null
			? StatusCode(StatusCodes.Status403Forbidden)
			: Page();
	}

	public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var result = await _groupSvc.AddUserToGroup(Id, NewMemberId, GroupRole.Member, GetUserId(User), cancellationToken);

			if (!result.IsValid)
			{
				ModelState.AddModelError(nameof(NewMemberId), result.Message);
			}
			else
			{
				ModelState.SetModelValue(nameof(NewMemberId), string.Empty, string.Empty);
			}

			var group = await _groupSvc.GetGroupById(Id, cancellationToken);

			if (group == null)
			{
				return NotFound();
			}

			Members = group.Memberships.Select(Member.Create).ToList();
		}

		return Page();
	}

	public async Task<IActionResult> OnPostGroupNameAsync(int id, string name, CancellationToken cancellationToken)
	{
		var result = await _groupSvc.UpdateGroupName(id, GetUserId(User), name, cancellationToken);

		return result.IsValid
			? Content(name)
			: Forbid();
	}

	public async Task<IActionResult> OnDeleteUserAsync(int id, [FromBody] UserRequest user, CancellationToken cancellationToken)
	{
		var result = await _groupSvc.RemoveUserFromGroup(id, user.UserId, GetUserId(User), cancellationToken);

		return result.IsValid
			? new EmptyResult()
			: Forbid();
	}

	public class Member
	{
		public string Id { get; init; } = null!;
		public string Name { get; init; } = null!;
		public GroupRole Role { get; init; }
		public List<MemberResult> Results { get; init; } = null!;

		public int SolutionCount => Results.Count(result => result.IsSolved);
		public float SolutionRate => (float)SolutionCount / Results.Count;
		public float SolutionAverage => (float)Results
			.Where(result => result.IsSolved)
			.Sum(result => result.GuessCount) / SolutionCount;

		public static Member Create(GroupMember member) => new()
		{
			Id = member.UserId,
			Name = member.User.Name,
			Role = member.Role,
			Results = member.User.Results.Select(MemberResult.Create).ToList()
		};
	}

	public class MemberResult
	{
		public DateOnly Day { get; init; }
		public bool IsSolved { get; init; }
		public int GuessCount { get; init; }

		public static MemberResult Create(Result result) => new()
		{
			Day = DateOnly.FromDateTime(result.Day.Date.UtcDateTime),
			IsSolved = result.IsSolved,
			GuessCount = result.Guesses.Count
		};
	}

	public record UserRequest(string UserId);
}
