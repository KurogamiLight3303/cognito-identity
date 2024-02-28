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

// ReSharper disable AccessToModifiedClosure

var builder = WebApplication.CreateBuilder(args);

WebApplication? app = null;

// Add services to the container.

builder.Services.AddControllers(options =>
    {
        options.Filters.Add(new HttpResponseExceptionFilter());
        IHttpRequestStreamReaderFactory? readerFactory;
        ILogger? logger;
        if (app == null 
            || (readerFactory = app.Services.GetService<IHttpRequestStreamReaderFactory>()) == null 
            || (logger = app.Services.GetService<ILogger>()) == null) 
            throw new($"Unable to Bind {nameof(CustomModelBinderProvider)}");
        options.ModelBinderProviders.Insert(0, 
            new CustomModelBinderProvider(options.InputFormatters, readerFactory, logger));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = DomainCoreConfigurationExtensions.InvalidModelHandling;
    });

builder.Configuration.LoadSecrets();

builder.Services
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.Authority = builder.Configuration.GetConnectionString("Cognito");
        x.RequireHttpsMetadata = false;
        x.TokenValidationParameters = new()
        {
            ValidateAudience = false,
            RoleClaimType = "cognito:groups"
        };
    });

builder.Services
    .ConfigureEndpoints()
    .ConfigureAutoMapper()
    .ConfigureMediator()
    .ConfigurePersistence(builder.Configuration.GetConnectionString("DomainConnectionString"))
    .ConfigureRepositories(builder.Configuration)
    .ConfigureServices(builder.Configuration)
    ;
builder
    .LoadLogging();
    

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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
    c.IncludeXmlComments(@"CognitoPOC.IdentityApi.xml");
});

app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsLocal())
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

var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();

if(locOptions != null)
    app.UseRequestLocalization(locOptions.Value);

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseForwardedHeaders(new()
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto
});

app.Run();