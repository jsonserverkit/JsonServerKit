## JsonServerKit
The project contains a C# .Net 6.0 software kit used to build/run an application server based on Tcp/Ip communication. 
The server supports SSL Security. The JsonServerKit currently consists of two components.

| Component | Descirption |
| ------------- | ------------- |
| JsonServerKit.AppServer | Contains the application server |
| JsonServerKit.Logging | Contains a wrapper around the Serilog logger component |

The application server component uses a set of patterns/interface that provides abstraction to enable best possible intergration to projects.
The JsonServerKit can be uesd to implement business logic based on an individual domain model.

### Application scenario
The following set of components shows a possible project setup. 

| Component | Descirption |
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
