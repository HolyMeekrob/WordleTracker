using System.Security.Claims;

namespace WordleTracker.Web.Utilities;

public static class Identity
{
	public static string GetUserId(ClaimsPrincipal user) =>
		user.FindFirstValue(ClaimTypes.NameIdentifier);
}
