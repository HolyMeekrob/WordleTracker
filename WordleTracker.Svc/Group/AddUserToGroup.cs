using Microsoft.EntityFrameworkCore;
using WordleTracker.Core.Validation;
using WordleTracker.Data.Models;

namespace WordleTracker.Svc;

public partial class GroupSvc
{
	public async Task<ValidationResult<GroupMember>> AddUserToGroup(int groupId, string userId, GroupRole role, string adminId, CancellationToken cancellationToken)
	{
		try
		{
			var members = await DbContext.GroupMembers.ToListAsync(cancellationToken);

			var admin = members.FirstOrDefault(member => member.UserId == adminId);
			var user = members.FirstOrDefault(member => member.UserId == userId);

			if (admin == null)
			{
				return ValidationResult<GroupMember>.Failure("Admin does not belong to the given group.");
			}

			if (user != null)
			{
				return ValidationResult<GroupMember>.Failure("User is already a member of the group.");
			}

			if (admin.Role <= role)
			{
				return ValidationResult<GroupMember>.Failure($"User does not have permissions to add a {role} to the group.");
			}

			var membership = new GroupMember
			{
				GroupId = groupId,
				UserId = userId,
				Role = role
			};

			DbContext.GroupMembers.Add(membership);
			await DbContext.SaveChangesAsync(cancellationToken);

			DbContext.Entry(membership).State = EntityState.Detached;

			return ValidationResult<GroupMember>.Success(membership);
		}
		catch (DbUpdateException)
		{
			return ValidationResult<GroupMember>.Failure($"Unable to add user {userId} to group.");
		}
	}
}
