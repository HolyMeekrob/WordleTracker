using HashidsNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WordleTracker.Core.Configuration;
using WordleTracker.Data;
using WordleTracker.Svc;
using WordleTracker.Web.Extensions;
using WordleTracker.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
	var configFolder = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, "Configuration");
	config
		.AddJsonFile(Path.Combine(configFolder, "Names.json"), false, false)
		.AddJsonFile(Path.Combine(configFolder, "Days.json"), false, false);
});

// Add services to the container.

builder.Services
	.AddRazorPages()
	.AddRazorOptions(options =>
	{
		options.PageViewLocationFormats.Add("/Pages/Shared/Partials/_{0}.cshtml");
		options.PageViewLocationFormats.Add("/Pages/Shared/Partials/{0}/_{0}.cshtml");
	});

builder.Services.AddConfig(builder.Configuration);

#region DI Registration

builder.Services.AddSingleton<IHashids>(_ => new Hashids(salt: builder.Configuration["HashidsSalt"], minHashLength: 10));
builder.Services.AddScoped<GroupSvc, GroupSvc>();
builder.Services.AddScoped<ResultSvc, ResultSvc>();
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
	var config = scope.ServiceProvider.GetRequiredService<IOptions<DaysOptions>>();

	context.Database.Migrate();
	DatabaseInitializer.Initialize(context, config);
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
