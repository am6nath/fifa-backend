using fifa_backend.Extensions;
using fifa_backend.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register application configurations and dependencies via extensions
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithAuth();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Core Pipeline Middleware Registration (Execution Order is Critical)
app.UseMiddleware<CorrelationIdMiddleware>(); // Must be first to generate X-Correlation-Id for all downstream processing
app.UseMiddleware<ExceptionMiddleware>();     // Must be early to catch all downstream exceptions and format them
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FIFA Fan Vote API v1");
    });
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Run database migrations and seed system default administrator and teams on startup
await app.Services.SeedDatabaseAsync();

app.Run();