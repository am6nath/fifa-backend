using System.Text;
using fifa_backend.Data;
using fifa_backend.Models;
using fifa_backend.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using fifa_backend.Repositories;
using fifa_backend.Services.Audit;
using fifa_backend.Services.Team;
using fifa_backend.Services.VotingSession;
using fifa_backend.Services.Vote;
using fifa_backend.Services.Users;
using fifa_backend.Services.Statistics;
using fifa_backend.Services.Email;

namespace fifa_backend.Extensions;

/// <summary>
/// Service registrations extensions to keep Program.cs clean.
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
        {
            throw new InvalidOperationException("JWT Settings or Secret Key is not configured correctly.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FIFA Fan Vote API",
                Version = "v1",
                Description = "Backend APIs for FIFA Fan Voting Platform."
            });

            // Enable Bearer Auth in Swagger UI
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                      ?? new[] { "http://localhost:4200" };

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins(origins)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core application services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        // Data Access Layer
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // HttpContextAccessor
        services.AddHttpContextAccessor();

        // Application Services
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IVotingSessionService, VotingSessionService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddHttpClient<IEmailService, EmailService>();

        // Bind Brevo Settings
        services.Configure<BrevoSettings>(configuration.GetSection("Brevo"));

        // Intercept validation failures and return standard ApiResponse
        services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                // Retrieve correlation ID from HttpContext items
                string? correlationId = null;
                if (context.HttpContext.Items.TryGetValue(Middleware.CorrelationIdMiddleware.CorrelationIdHeaderKey, out var cId))
                {
                    correlationId = cId?.ToString();
                }

                var response = new DTOs.Common.ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Validation failed.",
                    Errors = errors,
                    TraceId = correlationId
                };

                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
            };
        });

        return services;
    }
}
