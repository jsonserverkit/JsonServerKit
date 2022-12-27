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

// Get a Startup object instance.
var startup = new Startup();

// Provide appsettings.json configuration to DI.
startup.ConfigureAppConfiguration(new List<Action<HostBuilderContext, IConfigurationBuilder>>
{
    (_, config) =>
    {
        config
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("serversettings.json");
    }
});

// Configure required/additional services for DI.
startup.ConfigureServices(new List<Action<IServiceCollection>>
{
    // Configure/provide a logger implementation.
    // This Version of "JsonServerKit.AppServer.Startup" requires a Wrapper around the Serilog logger.
    // This dependency might become subject to change.
    services => { services.AddSingleton<ILog, Log>();}
});

// Configure the domain handlers.
startup.ConfigureDomainObjectHandler(typeof(Product), new ProductOperation());
startup.ConfigureDomainObjectHandler(typeof(Account), new AccountOperation());
startup.ConfigureDomainObjectCrudHandler(new[]{ typeof(Create<Account>) , typeof(Read<Account>) , typeof(Update<Account>) , typeof(Delete<Account>) }, new AccountCrudOperations());

// Call to run.
await startup.Run(args);