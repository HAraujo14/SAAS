using AprovaFlow.Api.Extensions;
using AprovaFlow.Api.Middlewares;
using AprovaFlow.Infrastructure.Data;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Events;

// ─── Serilog: configurar antes de tudo para capturar erros de startup ────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/aprovaflow-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ─── Serviços ─────────────────────────────────────────────────────────────
    builder.Services
        .AddDatabase(builder.Configuration)
        .AddRepositories()
        .AddApplicationServices()
        .AddJwtAuthentication(builder.Configuration)
        .AddValidation()
        .AddSwagger()
        .AddRateLimiting()
        .AddCorsPolicy(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddHttpLogging(logging =>
    {
        logging.LoggingFields = HttpLoggingFields.RequestMethod
            | HttpLoggingFields.RequestPath
            | HttpLoggingFields.ResponseStatusCode
            | HttpLoggingFields.Duration;
    });

    var app = builder.Build();

    // ─── Seed (apenas em desenvolvimento) ────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        await DatabaseSeeder.SeedAsync(db, logger);
    }

    // ─── Pipeline ─────────────────────────────────────────────────────────────
    // A ordem importa: middlewares executam em sequência (FIFO na entrada, LIFO na saída)

    app.UseMiddleware<ErrorHandlingMiddleware>();  // 1. Erros (outer)

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AprovaFlow API v1");
            c.RoutePrefix = string.Empty;  // Swagger na raiz em dev
        });
    }

    app.UseHttpsRedirection();
    app.UseHttpLogging();
    app.UseCors("Frontend");
    app.UseRateLimiter();
    app.UseAuthentication();            // 2. Autenticação (valida JWT)
    app.UseMiddleware<TenantResolutionMiddleware>(); // 3. Tenant (após auth)
    app.UseAuthorization();             // 4. Autorização (verifica roles)

    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
        .AllowAnonymous();

    Log.Information("AprovaFlow API iniciada. Ambiente: {Env}", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha fatal ao iniciar a aplicação.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Necessário para testes de integração (InternalsVisibleTo)
public partial class Program { }
