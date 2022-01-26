using System.Security.Claims;
using Microsoft.Extensions.Options;
using WordleTracker.Core.Configuration;
using WordleTracker.Data.Models;
using WordleTracker.Svc;
using static WordleTracker.Web.Utilities.Authentication;

namespace WordleTracker.Web.Middleware;
public class AutoLoginMiddleware
{
	private const int NameSelectionAttempts = 5;
	private const int DigitSuffixLength = 5;

	private static readonly int s_maxRandomDigit = (int)Math.Pow(10, DigitSuffixLength);
	private static readonly string s_suffixFormat = string.Concat(Enumerable.Repeat("0", DigitSuffixLength));

	private readonly RequestDelegate _next;
	private readonly ILogger<AutoLoginMiddleware> _logger;
	private readonly NamesOptions _names;
	private readonly int _adjectivesCount;
	private readonly int _nounsCount;

	public AutoLoginMiddleware(RequestDelegate next, IOptions<NamesOptions> namesConfig, ILogger<AutoLoginMiddleware> logger)
	{
		_next = next;
		_logger = logger;
		_names = namesConfig.Value;
		_adjectivesCount = _names.Adjectives.Count;
		_nounsCount = _names.Nouns.Count;
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
		var user = await CreateUser(userSvc);

		await SignIn(context, user);

		context.Items[ClaimTypes.NameIdentifier] = user.Id;
		context.Items[ClaimTypes.Name] = user.Id;
	}

	private async Task<User> CreateUser(UserSvc userSvc)
	{
		var attempts = 0;
		var userId = string.Empty;
		User? user = null;

		while (user == null && attempts < NameSelectionAttempts)
		{
			try
			{
				++attempts;
				userId = CreateUserId();
				user = await userSvc.CreateUser(userId, userId, new CancellationToken());
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"Attempted to insert duplicate user id {userId}");

				if (attempts >= NameSelectionAttempts)
				{
					throw new Exception($"Failed to generate a unique user id after { attempts } attempts", ex);
				}
			}
		}

		return user!;
	}

	private string CreateUserId()
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
