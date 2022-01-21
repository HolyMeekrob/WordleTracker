using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using WordleTracker.Core.Configuration;

namespace WordleTracker.Web.Middleware;
public class AutoLoginMiddleware
{
	private readonly RequestDelegate _next;
	private readonly NamesOptions _names;
	private readonly int _adjectivesCount;
	private readonly int _nounsCount;

	public AutoLoginMiddleware(RequestDelegate next, IOptions<NamesOptions> namesConfig)
	{
		_next = next;
		_names = namesConfig.Value;
		_adjectivesCount = _names.Adjectives.Count();
		_nounsCount = _names.Nouns.Count();
	}

	public async Task InvokeAsync(HttpContext context)
	{
		if (!context.User?.Identity?.IsAuthenticated ?? true)
		{
			await SignInNewUser(context);
		}
		await _next(context);
	}

	private async Task SignInNewUser(HttpContext context)
	{
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, CreateUserName())
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

	private string CreateUserName()
	{
		var random = DateTimeOffset.UtcNow.Ticks;
		var adjective = _names.Adjectives[(int)(random % _adjectivesCount)];
		var noun = _names.Nouns[(int)(random % _nounsCount)];

		return $"{adjective}{noun}";
	}
}

public static class AutoLoginMiddlewareExtensions
{
	public static IApplicationBuilder UseAutoLogin(this IApplicationBuilder builder) =>
		builder.UseMiddleware<AutoLoginMiddleware>();
}
