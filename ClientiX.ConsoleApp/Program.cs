using ClientiX.Application;
using ClientiX.ConsoleApp.Services;
using ClientiX.Infrastructure.Data;
using ClientiX.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("🚀 Starting ClientiX SaaS Bot");

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // 📁 Configuration
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

    // 🏗️ Services
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructure(builder.Configuration);

    // 🤖 Bots & Tasks
    builder.Services.AddHostedService<MainBotHostedService>();
    builder.Services.AddHostedService<ClientBotsHostedService>();
    builder.Services.AddHostedService<SubscriptionCheckerService>();
    builder.Services.AddHostedService<BookingNotifierService>();

    // 📝 Serilog
    builder.Logging.ClearProviders();
    builder.Services.AddSerilog();

    var host = builder.Build();

    // 🗄️ Auto Migration
    using var scope = host.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    Log.Information("✅ PostgreSQL ready - {ConnectionString}",
        context.Database.GetConnectionString()?.Split(';')[0]);

    Log.Information("🎉 ClientiX SaaS Bot ready!");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Host faulted");
}
finally
{
    Log.CloseAndFlush();
}
