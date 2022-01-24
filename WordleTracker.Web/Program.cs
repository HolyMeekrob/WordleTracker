﻿using Microsoft.AspNetCore.Authentication.Cookies;
using WordleTracker.Data;
using WordleTracker.Svc;
using WordleTracker.Web.Extensions;
using WordleTracker.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
	var configFolder = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, "Configuration");
	config.AddJsonFile(Path.Combine(configFolder, "Names.json"), false, false);
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddConfig(builder.Configuration);

#region DI Registration

builder.Services.AddScoped<UserSvc, UserSvc>();

#endregion DI Registration

builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.ExpireTimeSpan = TimeSpan.FromDays(365);
		options.SlidingExpiration = true;
	});

builder.Services.UseDbContexts(builder.Configuration);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
else
{
	app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<WordleTrackerContext>();
	context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseCookiePolicy(new CookiePolicyOptions
{
	MinimumSameSitePolicy = SameSiteMode.Strict
});

app.UseRouting();

app.UseAuthentication();
app.UseAutoLogin();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
