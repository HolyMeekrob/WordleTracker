using HashidsNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WordleTracker.Data.Models;
using WordleTracker.Svc;
using static WordleTracker.Core.Utilities.Functional;
using static WordleTracker.Web.Utilities.Identity;

namespace WordleTracker.Web.Pages;

public class GroupModel : PageModel
{
	private readonly ILogger<GroupModel> _logger;
	private readonly IHashids _hashids;
	private readonly GroupSvc _groupSvc;

	[BindProperty]
	public string Id { get; set; } = null!;
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

	public GroupModel(ILogger<GroupModel> logger, IHashids hashids, GroupSvc groupSvc)
	{
		_logger = logger;
		_hashids = hashids;
		_groupSvc = groupSvc;
	}

	public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
	{
		var groupId = GetGroupId(id);

		var details = await _groupSvc.GetGroupDetails(groupId, cancellationToken);

		if (details == null)
		{
			return NotFound();
		}

		Id = id;
		Name = details.Name;
		Members = details.Users.Select(Member.Create).ToList();

		return Me is null
			? StatusCode(StatusCodes.Status403Forbidden)
			: Page();
	}

	public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
	{
		if (ModelState.IsValid)
		{
			var groupId = GetGroupId();
			var result = await _groupSvc.AddUserToGroup(groupId, NewMemberId, GroupRole.Member, GetUserId(User), cancellationToken);

			if (!result.IsValid)
			{
				ModelState.AddModelError(nameof(NewMemberId), result.Message);
			}
			else
			{
				ModelState.SetModelValue(nameof(NewMemberId), string.Empty, string.Empty);
			}

			var details = await _groupSvc.GetGroupDetails(groupId, cancellationToken);

			if (details == null)
			{
				return NotFound();
			}

			Members = details.Users.Select(Member.Create).ToList();
		}

		return Page();
	}

	public async Task<IActionResult> OnPostGroupNameAsync(string id, string name, CancellationToken cancellationToken)
	{
		var groupId = GetGroupId(id);
		if (groupId == -1)
		{
			return NotFound();
		}

		var result = await _groupSvc.UpdateGroupName(groupId, GetUserId(User), name, cancellationToken);

		return result.IsValid
			? Content(name)
			: Forbid();
	}

	public async Task<IActionResult> OnDeleteUserAsync(string id, [FromBody] UserRequest user, CancellationToken cancellationToken)
	{
		var groupId = GetGroupId(id);
		if (groupId == -1)
		{
			return NotFound();
		}

		var result = await _groupSvc.RemoveUserFromGroup(groupId, user.UserId, GetUserId(User), cancellationToken);

		return result.IsValid
			? new EmptyResult()
			: Forbid();
	}

	private int GetGroupId() => GetGroupId(Id);
	private int GetGroupId(string hashedId)
	{
		var ids = _hashids.Decode(hashedId);
		return ids.Length == 1 ? ids.First() : -1;
	}

	public record Member(string Id, string Name, GroupRole Role, List<MemberResult> Results)
	{
		public int SolutionCount => Results.Count(result => result.IsSolved);
		public float SolutionRate => (float)SolutionCount / Results.Count;
		public float SolutionAverage => (float)Results
			.Where(result => result.IsSolved)
			.Sum(result => result.GuessCount) / SolutionCount;

		public Dictionary<int, int> ResultSplits =>
			Enumerable.Range(1, 6)
			.ToDictionary(Identity, n => Results.Count(member => member.GuessCount == n));

		public static Member Create(GroupUser user) => new
		(
			user.UserId,
			user.Name,
			user.Role,
			user.Results.Select(MemberResult.Create).ToList()
		);
	}

	public record MemberResult(DateOnly Day, bool IsSolved, bool HardMode, int GuessCount)
	{
		public static MemberResult Create(GroupResult result) => new
		(
			result.Date,
			result.IsSolved,
			result.HardMode,
			result.GuessCount
		);
	}

	public record UserRequest(string UserId);
}
