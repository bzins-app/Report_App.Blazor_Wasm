# Report application on blazor webassembly hosted 
A demo version is available at: [https://reportappdemo.dev.bzins.app/](https://reportappdemo.dev.bzins.app/) 

This application is designed to generate reports and manage database operations efficiently. It is built with ASP.NET Core (NET 8) and offers three main features:

1. **Report Generation**: Users can create reports in various formats such as XLS, XLS templates, XML, CSV, and more. These reports can either be sent via email to a designated recipient list or dropped to FTP/SFTP locations. Email alerts can also be scheduled using a visual cron editor to ensure timely delivery.

2. **Database Querying and Bulk Insert**: The application allows users to query databases and perform bulk data inserts into other databases, either into existing tables or by creating new ones with a primary key. This functionality is fully schedulable via cron jobs, enabling automated and recurring database operations.

3. **Query Store and Data Visualization**: Users can store and run queries manually to extract data, visualize results in pivot tables, or generate charts. This feature provides a powerful interface for data analysis directly within the application.

Moreover, an API is available to trigger jobs externally, allowing seamless integration with other systems.

### Additional Features and Technologies:
- The application supports multiple database types, including Oracle, SQL Server, PostgreSQL, MariaDB, and MySQL, with the ability to add others easily.
- It utilizes official Microsoft libraries such as Entity Framework Core and Identity Manager.
- The layout is built using the MudBlazor UI framework (https://mudblazor.com/), while data rendering and grids are powered by a fork of QuickGrid (https://aspnet.github.io/quickgridsamples/).
- Background tasks are managed using Hangfire (https://www.hangfire.io/), enabling robust job scheduling and execution.


Below are some UI screenshots:

---

This combines all key points, outlining the application's structure, features, and technologies used. Let me know if you'd like further adjustments!


Below some UI screenshots:

![image](https://user-images.githubusercontent.com/46160493/213934774-20c90656-1c1d-4afc-86a0-976ddaa37dc7.png)
![image](https://user-images.githubusercontent.com/46160493/213935278-7ae761d8-4ca8-4163-84d7-9754b9d323f6.png)
![image](https://user-images.githubusercontent.com/46160493/213935358-487dfb89-c353-4989-9044-995a6483b856.png)
![image](https://user-images.githubusercontent.com/46160493/213935415-875a4ed1-f357-48db-a4cf-b2386ce40e56.png)

## How to start:

With docker-compose: base user "admin@report.app" and base password "$tR0ngcR3d?". In the future those parameters will be customisable.

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
     - reportapp_db:/var/opt/mssql
     
  report_app_blazorserv:
    image: benoitzins/reportapp_wasm:latest
    ports:
      - '7080:80'
      - '7443:443'
    build:
      context: .
      dockerfile: Report_App_BlazorServ/Dockerfile
    container_name: ReportApp_Blazor_server
    restart: always
    environment:
      - 'ConnectionStrings__DefaultConnection=Server=db;Database=report_app;User Id=sa;Password=UltraSecurePAs$worD!;MultipleActiveResultSets=true'
      - 'BaseUserDefaultOptions:Email=admin@report.app'
      - 'BaseUserDefaultOptions:Password=$tR0ngcR3d?'
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

