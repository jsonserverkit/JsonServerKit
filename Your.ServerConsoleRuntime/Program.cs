// .Net
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// JsonServerKit
using JsonServerKit.Logging;
using JsonServerKit.AppServer;
using JsonServerKit.AppServer.Data.Crud;
// Your domain
using Your.BusinessLogic.CrudOperations;
using Your.BusinessLogic.Operations;
using Your.Domain.BusinessObjects;

// Get a runtime object instance.
var runtime = new Runtime();

// Provide appsettings.json configuration to DI.
runtime.ConfigureAppConfiguration(new List<Action<HostBuilderContext, IConfigurationBuilder>>
{
    (_, config) =>
    {
        config
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
    }
});

// Configure required/additional services for DI.
runtime.ConfigureServices(new List<Action<IServiceCollection>>
{
    // Configure/provide a logger implementation.
    // This Version of "JsonServerKit.AppServer.Runtime" requires a Wrapper around the Serilog logger.
    // This dependency might become subject to change.
    services => { services.AddSingleton<ILog, Log>();}
});

// Configure the domain handlers.
runtime.ConfigureDomainObjectHandler(typeof(Account), new AccountOperation());
runtime.ConfigureDomainObjectCrudHandler(new[]{ typeof(Create<Account>) , typeof(Read<Account>) , typeof(Update<Account>) , typeof(Delete<Account>) }, new AccountCrudOperations());

// Call to run.
await runtime.Run(args);