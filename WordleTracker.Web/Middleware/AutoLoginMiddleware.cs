﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using WordleTracker.Core.Configuration;
using WordleTracker.Data.Models;
using WordleTracker.Svc;

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
		var userId = await CreateUser(userSvc);
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, userId)
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

	private async Task<string> CreateUser(UserSvc userSvc)
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

		return userId;
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
