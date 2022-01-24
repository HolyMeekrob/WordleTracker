using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using WordleTracker.Core.Configuration;
using WordleTracker.Svc;

namespace WordleTracker.Web.Middleware;
public class AutoLoginMiddleware
{
	private const int DigitSuffixLength = 5;
	private static readonly int s_maxRandomDigit = (int)Math.Pow(10, DigitSuffixLength);
	private static readonly string s_suffixFormat = string.Concat(Enumerable.Repeat("0", DigitSuffixLength));

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

	public async Task InvokeAsync(HttpContext context, UserSvc userSvc)
	{
		if (!context.User?.Identity?.IsAuthenticated ?? true)
		{
			await SignInNewUser(context, userSvc);
		}
		await _next(context);
	}

	private async Task SignInNewUser(HttpContext context, UserSvc userSvc)
	{
		var userName = CreateUserName();
		await userSvc.CreateUser(userName, userName, new CancellationToken());

		var claims = new[]
		{
			new Claim(ClaimTypes.Name, userName)
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
		var random = new Random();
		var adjective = _names.Adjectives[random.Next(_adjectivesCount)];
		var noun = _names.Nouns[random.Next(_nounsCount)];
		var suffix = random.Next(s_maxRandomDigit).ToString(s_suffixFormat);

		return $"{adjective}{noun}{suffix}";
	}
}

public static class AutoLoginMiddlewareExtensions
{
	public static IApplicationBuilder UseAutoLogin(this IApplicationBuilder builder) =>
		builder.UseMiddleware<AutoLoginMiddleware>();
}
