using System.Globalization;
using System.Reflection;
using Amazon.CloudWatchLogs;
using Amazon.CognitoIdentityProvider;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using CognitoPOC.Domain.Common;
using CognitoPOC.Domain.Common.Models;
using CognitoPOC.Domain.Models.UserAccounts;
using CognitoPOC.Domain.Models.UserAccounts.QueryFilters;
using CognitoPOC.Domain.Repositories.UserAccounts;
using CognitoPOC.Domain.Services;
using CognitoPOC.Infrastructure.Configurations;
using CognitoPOC.Infrastructure.Services;
using CognitoPOC.Infrastructure.Services.Authentication;
using CognitoPOC.Infrastructure.Persistence.Contexts;
using CognitoPOC.Infrastructure.Persistence.Interceptors;
using CognitoPOC.Infrastructure.Persistence.Repositories.UserAccounts;
using CognitoPOC.Infrastructure.Persistence.UnitsOfWork;
using ILogger = Serilog.ILogger;

namespace CognitoPOC.Infrastructure;

public static class Configuration
{
    private static string GetKey(string? @override)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var entryAssembly = Environment.GetEnvironmentVariable("LAMBDA_DOTNET_MAIN_ASSEMBLY")?[..^4] ??
                            Assembly.GetEntryAssembly()?.GetName().Name;
        return string.IsNullOrEmpty(@override) ? $"{entryAssembly}_{env}" : @override;
    }

    public static void LoadSecrets(this IConfigurationBuilder configuration)
    {
        var @override = Environment.GetEnvironmentVariable("APP_SECRETS_ID");
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if(env != "Local")
            configuration.AddSecretsManager(configurator: options =>
            {
                var pattern = GetKey(@override);
                Console.WriteLine(@$"Secrets: {pattern}");
                options.KeyGenerator = (_, s) 
                    => s.Replace($"{pattern}:", string.Empty);
                options.SecretFilter = entry => entry.Name == pattern;
            });
    }
    public static IServiceCollection ConfigureEndpoints(this IServiceCollection services)
    {
        var @override = Environment.GetEnvironmentVariable("APP_KEYS_ID");
        services
            .AddDataProtection()
            .PersistKeysToAWSSystemsManager(GetKey(@override));
        services
            .AddLocalization()
            .Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new("en"),
                    new("es")
                };

                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(context =>
                {
                    var languages = context.Request.Headers["Accept-Language"].ToString();
                    var currentLanguage = languages.Split(',').FirstOrDefault();
                    currentLanguage = string.IsNullOrEmpty(currentLanguage) ? "en" : currentLanguage;

                    return Task.FromResult(new ProviderCultureResult(currentLanguage, currentLanguage))!;
                }));
            })
            .AddCors();
        return services;
    }
    
    public static IServiceCollection ConfigureAutoMapper(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddAutoMapper((requestServices, action) =>
        {
            action.ConstructServicesUsing(requestServices.GetService);
        }, new[] { typeof(UserAccountObject).Assembly }, 
            ServiceLifetime.Scoped);
    }
    
    public static IServiceCollection ConfigurePersistence(this IServiceCollection services, string? connectionString)
    {
        return services
            .AddScoped<AuditInterceptor>()
            .AddDbContext<DomainContext>((provider, options) =>
            {
                options.AddInterceptors(provider.GetRequiredService<AuditInterceptor>());
                options.UseSqlServer(connectionString, 
                sqlOptions 
                    =>
                {
                    sqlOptions.EnableRetryOnFailure();
                    sqlOptions.CommandTimeout(120);
                }); 
            })
            .AddScoped<IUnitOfWork, DomainUnitOfWork>();
    }

    public static IServiceCollection ConfigureRepositories(this WebApplicationBuilder builder)
        => ConfigureRepositories(builder.Services, builder.Configuration);
    public static IServiceCollection ConfigureRepositories(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        return serviceCollection
                .AddScoped<IUserAccountCommandRepository, UserAccountsRepository>()
                .AddScoped<IUserAccountQueryRepository, UserAccountsReadOnlyRepository>()
                .AddSingleton<IQueryFilterTranslator<UserAccountObject, Guid>,UserAccountQueryFilterTranslator>()
                .AddScoped<IUserDeviceRepository, UserDeviceRepository>()
                .AddScoped<IUserDeviceQueryRepository, UserDeviceRepository>()
            ;
    }
    public static IServiceCollection ConfigureServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .AddOptions<CognitoConfiguration>()
            .Bind(configuration.GetSection("cognito"))
            ;
        serviceCollection
            .AddOptions<S3StorageConfiguration>()
            .Bind(configuration.GetSection("s3"))
            ;
        
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        return serviceCollection
                .AddSingleton<IAmazonCognitoIdentityProvider, AmazonCognitoIdentityProviderClient>()
                .AddScoped<IAuthenticationSignInService, AuthenticationSignInService>()
                .AddScoped<ISessionMetadataService, SessionMetadataService>()
                .AddScoped<IAuthenticationTotpService, AuthenticationTotpService>()
                .AddScoped<IAuthenticationPasswordService, AuthenticationPasswordService>()
                .AddScoped<IAuthenticationProfileService, AuthenticationProfileService>()
                .AddScoped<IAuthenticationSessionService, AuthenticationSessionService>()
                .AddScoped<IAuthenticationSignUpService, AuthenticationSignUpService>()
                .AddScoped<IAuthenticationRolesService, AuthenticationRolesService>()
                .AddSingleton<IAmazonS3, AmazonS3Client>()
                .AddSingleton<IResourceService, ResourceService>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            ;
    }
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        ConfigureServices(builder.Services, builder.Configuration);
    }

    public static void LoadLogging(this IServiceCollection serviceCollection, IConfiguration configuration, ILoggingBuilder logging)
    {
        logging.ClearProviders();
        var logGroup = Environment.GetEnvironmentVariable("APP_LOGGING_GROUP");
        if (string.IsNullOrEmpty(logGroup))
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var entryAssembly = Environment.GetEnvironmentVariable("LAMBDA_DOTNET_MAIN_ASSEMBLY")?[..^4] ??
                                Assembly.GetEntryAssembly()?.GetName().Name;
            logGroup = $"{entryAssembly}/{env}";
        }
        
        var cloudWatchClient = new AmazonCloudWatchLogsClient();
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.ColoredConsole()
            .WriteTo.AmazonCloudWatch(new CloudWatchSinkOptions()
            {
                LogGroupName = $"/aws/logs/{logGroup}",
                TextFormatter = new Serilog.Formatting.Json.JsonFormatter()
            }, cloudWatchClient)
            .CreateLogger();
        
        serviceCollection
            .AddSingleton<ILogger>(logger)
            .AddSingleton<Microsoft.Extensions.Logging.ILogger>(p =>
            {
                var loggerFactory = new LoggerFactory()
                    .AddSerilog(p.GetRequiredService<ILogger>());
                return loggerFactory.CreateLogger("Logger");
            });
        logging.AddSerilog(logger);
    }
    
    public static void LoadLogging(this WebApplicationBuilder builder)
    {
        LoadLogging(builder.Services, builder.Configuration, builder.Logging);
    }
}