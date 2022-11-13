## JsonServerKit
The project is a C# .Net 6.0 software kit used to build (and run) an application server based on TCP/IP communication. 
The server supports SSL security. Data is transfered in JSON format encrypted with SSL. JsonServerKit currently consists of two components.

| Component | Description |
| ------------- | ------------- |
| JsonServerKit.AppServer | Contains the application server |
| JsonServerKit.Logging | Contains a wrapper around the Serilog logger component |

The application server component uses a set of patterns/interface that provides abstraction to enable best possible intergration to projects.
The JsonServerKit uses the dependency injection (DI) software design pattern together with the DI classes provided by .Net to enforce loosely coupling.
The JsonServerKit can be uesd to implement business logic based on an individual domain model.
The applicaton server supports domain objects to be processed using a IOperation based pattern and/or a CRUD based IOperation pattern.
The domain data is transfered using Json serialization/deserialization.     

### Application scenario
The following set of components shows a possible project setup. 

| Component | Description |
| ------------- | ------------- |
| JsonServerKit.AppServer | Contains the application server |
| JsonServerKit.Logging | Contains a wrapper around the Serilog logger component |
| Your.Domain | Contains the domain object model of your business |
| Your.BusinessLogic | Contains the busines logic that you want the application server to process |
| Your.ServerConsoleRuntime | Contains the runtime setup for the application server to run your business case |

### Dependency Graph using JsonServerKit as an application server
![image](https://user-images.githubusercontent.com/118096766/201517279-a9dd813c-803c-4bd1-a1c5-1c831e15a5af.png)

### Dependency Graph using JsonServerKit as an application server with clients
![image](https://user-images.githubusercontent.com/118096766/201517285-6f903ac9-481c-481c-a2e2-95e05a9226d4.png)

### Dependencies of JsonServerKit
The JsonServerKit components have the following dependencies.

| Component | Dependency |
| ------------- | ------------- |
| JsonServerKit.AppServer | Microsoft.Extensions.Hosting |
|  | Newtonsoft.Json |
|  | Serilog |
|  | System.Threading.Tasks.Dataflow |
| JsonServerKit.Logging | Microsoft.Extensions.Hosting |
|  | Serilog |
|  | Serilog.Enrichers.Environment |
|  | Serilog.Enrichers.Process |
|  | Serilog.Enrichers.Thread |
|  | Serilog.Settings.Configuration |
|  | Serilog.Sinks.RollingFile |

### Interfaces in application context
The following dependency diagram shows how the interfaces provided by JsonServerKit might be used in a project setup.
![image](https://user-images.githubusercontent.com/118096766/201525855-e819c239-024e-4b14-ba6c-c8b10c35e1f1.png)

### JsonServerKit 
- **IS NOT** designed to be a REST API.
- **DOES NOT** provide certificate/s for the destination hosting environment.
- **DOES NOT** provide hosting facilities (Docker) for the destination environment.
- **DOES NOT** provide code genaration tool/s to generate ICrudOperation based implementations.
