using Azure.Messaging.ServiceBus;
using AzureVerificationProvider.Data.Contexts;
using AzureVerificationProvider.Functions;
using AzureVerificationProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<DataContext>(x => x.UseSqlServer(Environment.GetEnvironmentVariable("VerificationRequestDB")));
        services.AddSingleton(x => new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnection")));
        services.AddScoped<VerificationCodeService>();
        services.AddScoped<VerificationCleanerService>();
        services.AddScoped<ValidateVerificationCodeService>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{


    try
    {
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var migrations = context.Database.GetPendingMigrations();
        if (migrations != null && migrations.Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex) 
    { 
        Debug.WriteLine($"ERROR : AzureVerificationProvider.Program :: {ex.Message}");
    }
}

host.Run();