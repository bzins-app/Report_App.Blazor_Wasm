# Report applciation on blazor webassembly hosted (readme will be updated soon)

The purpose of this application is to generate reports from databases. You can choose the type of database (Oracle, SQLserver, PostgreSQL, Mariadb or MySQL) but others can be easily added.

Once you have entered the connection credentials, you can create report or alerts define which email recipients or a network path via FTP or sFTP
Each job can be "croned" with a dedicated tool.

This application is built with ASP.NET Core (NET 7).

An example is available at: https://reportappdemo.dev.bzins.app/ -> Update on going. 
To log in, use the user "superviseur" and the password "$tR0ng)cR3d?"

Number of official Microsoft libraries are used like EntityFramework core or identity manager.

* The layout uses the Mud Blazor UI framework: https://mudblazor.com/
* Datagrid and data rendering is powered by a fork of QuickGrid: https://aspnet.github.io/quickgridsamples/
* BackgroundWorkers are handled by Hangfire: https://www.hangfire.io/


Below some UI screenshots:

![image](https://user-images.githubusercontent.com/46160493/213934774-20c90656-1c1d-4afc-86a0-976ddaa37dc7.png)
![image](https://user-images.githubusercontent.com/46160493/213935278-7ae761d8-4ca8-4163-84d7-9754b9d323f6.png)
![image](https://user-images.githubusercontent.com/46160493/213935358-487dfb89-c353-4989-9044-995a6483b856.png)
![image](https://user-images.githubusercontent.com/46160493/213935415-875a4ed1-f357-48db-a4cf-b2386ce40e56.png)

## How to start:

With docker-compose: base user "admin@report.app" and base password "$tR0ng)cR3d?". In the future those parameters will be customisable.

```yml
version: '3.4'

services:
  db:
    image: "mcr.microsoft.com/mssql/server:2019-latest"
    user: root
    ports:
      - '7433:1433'    
    restart: always
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Ultra)SecurePAs$worD!
      - MSSQL_DB=report_app
    volumes:
     - reportapp_db:/var/opt/mssql/data
     
  report_app_blazorserv:
    image: registry.bzins.app/reportappblazorserv:latest
    ports:
      - '7080:80'
      - '7443:443'
    build:
      context: .
      dockerfile: Report_App_BlazorServ/Dockerfile
    container_name: ReportApp_Blazor_server
    restart: always
    environment:
      - 'ConnectionStrings__DefaultConnection=Server=db;Database=report_app;User Id=sa;Password=Ultra)SecurePAs$worD!;MultipleActiveResultSets=true'
      - 'BaseUserDefaultOptions:Email=admin@report.app'
      - 'BaseUserDefaultOptions:Password=$tR0ng)cR3d?'
    volumes:
      - reportapp_docsstorage:/app/wwwroot/docsstorage
      - reportapp_upload:/app/wwwroot/upload
    depends_on:
      - db  
volumes:
  reportapp_docsstorage:
  reportapp_upload:
  reportapp_db:
```
