using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WordleTracker.Data.Models;

namespace WordleTracker.Web.Utilities;

public static class Authentication
{
	public static async Task SignIn(HttpContext context, User user)
	{
		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id),
			new Claim(ClaimTypes.Name, user.Name)
		};

		var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

		var authProperties = new AuthenticationProperties
		{
			AllowRefresh = true,
			ExpiresUtc = new DateTime(2038, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			IssuedUtc = DateTimeOffset.UtcNow,
			IsPersistent = true
		};

		await context.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(claimsIdentity),
			authProperties);
	}

	public static async Task SignOut(HttpContext context)
	{
		await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
	}
}
