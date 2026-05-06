using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebScrapper.ScraperApi.Configuration;
using WebScrapper.ScraperApi.Repositories;
using WebScrapper.ScraperApi.Repositories.Interfaces;
using WebScrapper.ScraperApi.Services;
using WebScrapper.ScraperApi.Services.Interfaces;

namespace WebScrapper.ScraperApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScraperServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DbSettings>(configuration.GetSection(DbSettings.Key));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.Key));
        services.Configure<TelegramSettings>(configuration.GetSection(TelegramSettings.Key));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var dbSettings = sp.GetRequiredService<IOptions<DbSettings>>().Value;
            return new MongoClient(dbSettings.MongoUrl);
        });
        services.AddSingleton(sp =>
        {
            var dbSettings = sp.GetRequiredService<IOptions<DbSettings>>().Value;
            return sp.GetRequiredService<IMongoClient>().GetDatabase(dbSettings.DatabaseName);
        });

        services.AddScoped<IAdsRepository, AdsRepository>();
        services.AddScoped<IScrapJobsRepository, ScrapJobsRepository>();
        services.AddScoped<IScrapRunsRepository, ScrapRunsRepository>();
        services.AddScoped<IWebsiteMetadataRepository, WebsiteMetadataRepository>();

        services.AddScoped<INotificationRepository, SmtpNotificationRepository>();
        services.AddHttpClient<INotificationRepository, TelegramNotificationRepository>();

        services.AddScoped<IAdsService, AdsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IScrapJobsService, ScrapJobsService>();
        services.AddScoped<IScrapRunsService, ScrapRunsService>();
        services.AddScoped<IScrapService, ScrapService>();
        services.AddScoped<IWebsiteMetadataService, WebsiteMetadataService>();

        services.AddSingleton<IScrapEventBroadcaster, SignalRScrapEventBroadcaster>();

        return services;
    }
}
