using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WordleTracker.Data.Models;
using WordleTracker.Svc;

namespace WordleTracker.Web.Pages;

public class GroupModel : PageModel
{
	private readonly ILogger<GroupModel> _logger;
	private readonly GroupSvc _groupSvc;

	public string Name { get; set; } = null!;
	public List<Member> Members { get; set; } = null!;

	public GroupModel(ILogger<GroupModel> logger, GroupSvc groupSvc)
	{
		_logger = logger;
		_groupSvc = groupSvc;
	}

	public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
	{
		var group = await _groupSvc.GetGroupById(id, cancellationToken);
		Name = group.Name;
		Members = group.Memberships.Select(Member.Create).ToList();

		return Page();
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
}
