using Microsoft.EntityFrameworkCore;
using WordleTracker.Core.Validation;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class GroupSvc
{
	public async Task<ValidationResult> RemoveUserFromGroup(int groupId, string memberId, string adminId, CancellationToken cancellationToken)
	{
		var groupMembers = await DbContext
			.GroupMembers
			.Where(member => member.GroupId == groupId)
			.ToListAsync(cancellationToken);

		var admin = groupMembers.FirstOrDefault(member => member.UserId == adminId);
		var user = groupMembers.FirstOrDefault(member => member.UserId == memberId);

		if (admin == null)
		{
			return ValidationResult.Failure("Admin is not a member of the group");
		}

		if (user == null)
		{
			return ValidationResult.Failure("User is not a member of the group");
		}

		if (AdminCannotRemoveUser(admin, user))
		{
			return ValidationResult.Failure("Admin does not have permission to remove user");
		}

		DbContext.Remove(user);
		await DbContext.SaveChangesAsync(cancellationToken);

		return ValidationResult.Success();
	}

	private static bool AdminCannotRemoveUser(GroupMember admin, GroupMember user) =>
		admin.Role <= user.Role;
}
