#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
## add library for autofit excel column with Epplus
#RUN apt-get update
#RUN apt-get install -y apt-utils
#RUN apt-get install -y libgdiplus
#RUN apt-get install -y libc6-dev 
#RUN ln -s /usr/lib/libgdiplus.so/usr/lib/gdiplus.dll
# Change timezone to local time
ENV TZ=Europe/Paris
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone


WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Report_App_WASM/Server/Report_App_WASM.Server.csproj", "Report_App_WASM/Server/"]
COPY ["Report_App_WASM/Client/Report_App_WASM.Client.csproj", "Report_App_WASM/Client/"]
COPY ["Report_App_WASM/Shared/Report_App_WASM.Shared.csproj", "Report_App_WASM/Shared/"]
RUN dotnet restore "Report_App_WASM/Server/Report_App_WASM.Server.csproj"
COPY . .
WORKDIR "/src/Report_App_WASM/Server"
RUN dotnet build "Report_App_WASM.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Report_App_WASM.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Report_App_WASM.Server.dll"]

RUN mkdir -p /app/wwwroot/docsstorage
RUN mkdir -p /app/wwwroot/upload