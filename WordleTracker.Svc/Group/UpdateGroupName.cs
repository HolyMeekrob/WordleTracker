using Microsoft.EntityFrameworkCore;
using WordleTracker.Core.Validation;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class GroupSvc
{
	public async Task<ValidationResult> UpdateGroupName(int groupId, string userId, string groupName, CancellationToken cancellationToken)
	{
		var groupMember = await DbContext
			.GroupMembers
			.Include(member => member.Group)
			.FirstOrDefaultAsync(
				member => member.GroupId == groupId && member.UserId == userId,
				cancellationToken);

		if (groupMember == null || !CanMemberUpdateGroupName(groupMember))
		{
			return ValidationResult.Failure("User does not have permissions to update the group name");
		}

		groupMember.Group.Name = groupName;

		await DbContext.SaveChangesAsync(cancellationToken);

		return ValidationResult.Success();
	}

	private static bool CanMemberUpdateGroupName(GroupMember member) =>
		CanRoleUpdateGroupName(member.Role);
	private static bool CanRoleUpdateGroupName(GroupRole role) =>
		role == GroupRole.Owner;
}
