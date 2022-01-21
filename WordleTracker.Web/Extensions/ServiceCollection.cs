using WordleTracker.Core.Configuration;

namespace WordleTracker.Web.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config) =>
		services.Configure<NamesOptions>(config.GetSection(NamesOptions.Section));
}
