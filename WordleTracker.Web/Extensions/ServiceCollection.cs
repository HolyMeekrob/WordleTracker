using Microsoft.EntityFrameworkCore;
using WordleTracker.Core.Configuration;
using WordleTracker.Data;

namespace WordleTracker.Web.Extensions;

public static class ServiceCollectionExtensions
{
	private const string SqliteConnectionKey = "Sqlite";

	public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config) =>
		services.Configure<NamesOptions>(config.GetSection(NamesOptions.Section));

	public static IServiceCollection UseDbContexts(this IServiceCollection services, IConfiguration config) =>
		services.AddDbContext<WordleTrackerContext>(options =>
			options.UseSqlite(config.GetConnectionString(SqliteConnectionKey)));
}
