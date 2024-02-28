using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using CognitoPOC.Domain.Core;
using CognitoPOC.Domain.Core.Common;
using CognitoPOC.Domain.Core.Common.CustomBinder;
using CognitoPOC.Infrastructure;
using CognitoPOC.Infrastructure.Extensions;
using CognitoPOC.Infrastructure.Middleware;

namespace CognitoPOC.IdentityApi;

/// <summary>
/// StartUp for AWS Lambda
/// </summary>
public class LambdaStartUp
{
    private ILoggingBuilder LoggingBuilder { get; }
    private const string AppSettingsName = "appsettings.json";
    private ConfigurationManager Configuration { get; }
    private IApplicationBuilder? App { get; set; }

    /// <summary>
    /// Start Up Services
    /// </summary>
    public LambdaStartUp(ILoggingBuilder? loggingBuilder = null)
    {
        LoggingBuilder = loggingBuilder ?? throw new("Invalid Access to logging Builder");
        Configuration = new ConfigurationManager();
        Configuration.AddJsonFile(AppSettingsName, true);
        Configuration.AddJsonFile($"{AppSettingsName}.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}",
            true);
    }
    
    /// <summary>
    /// Configure Services
    /// </summary>
    /// <param name="services">Service Collection</param>
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        Console.WriteLine(@"Start Services Configuration");
        // Add services to the container.
        services
            .AddControllers(options =>
            {
                options.Filters.Add(new HttpResponseExceptionFilter());
                IHttpRequestStreamReaderFactory? readerFactory;
                ILogger? logger;
                if (App == null 
                    || (readerFactory = App.ApplicationServices.GetService<IHttpRequestStreamReaderFactory>()) == null 
                    || (logger = App.ApplicationServices.GetService<ILogger>()) == null) 
                    throw new($"Unable to Bind {nameof(CustomModelBinderProvider)}");
                options.ModelBinderProviders.Insert(0, 
                    new CustomModelBinderProvider(options.InputFormatters, readerFactory, logger));
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = DomainCoreConfigurationExtensions.InvalidModelHandling;
            });
        
        Configuration.LoadSecrets();
        
        services
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.Authority = Configuration.GetConnectionString("Cognito");
                x.RequireHttpsMetadata = false;
                x.TokenValidationParameters = new()
                {
                    ValidateAudience = false,
                    RoleClaimType = "cognito:groups"
                };
            });
        
        services
            .ConfigureEndpoints()
            .ConfigureAutoMapper()
            .ConfigureMediator()
            .ConfigureRepositories(Configuration)
            .ConfigurePersistence(Configuration.GetConnectionString("DomainConnectionString"))
            .ConfigureServices(Configuration)
            ;
        services.LoadLogging(Configuration, LoggingBuilder);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new()
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.OperationFilter<SwaggerDeviceAuthOperationFilter>();
            c.AddSecurityRequirement(new()
            {
                {securityScheme, Array.Empty<string>()}
            });
            c.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Title = "Identity API",
                Contact = new OpenApiContact()
                {
                    Name = "Jose R Barro",
                    Email = "digits.inc@icloud.com"
                }
            });
            c.IncludeXmlComments($@"CognitoPOC.IdentityApi.xml");
        });
        
        Console.WriteLine(@"End Services Configuration");
    }
        
    /// <summary>
    /// Configure Application Start
    /// </summary>
    /// <param name="app">Application</param>
    /// <param name="env">Environment</param>
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        App = app;
        if (env.IsDevelopment() || env.IsLocal())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DefaultModelsExpandDepth(-1);
            });
            app.UseCors(options =>
                options
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(_ => true) // allow any origin
                    .AllowCredentials()
            );
        }
        else
        {
            string? origins;
            if (!string.IsNullOrEmpty(origins = Environment.GetEnvironmentVariable("APP_CORS_ORIGINS")))
            {
                Console.WriteLine(@"Allowed Origins {origins}");
                var urls = origins.Split(";");
                app.UseCors(options =>
                    options
                        .WithOrigins(urls)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                );
            }
        }

        var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();

        if(locOptions != null)
            app.UseRequestLocalization(locOptions.Value);

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseForwardedHeaders(new()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto
        });
    }
}