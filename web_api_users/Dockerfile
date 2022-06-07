#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
#EXPOSE 80
#EXPOSE 443

#ENV ASPNETCORE_HTTP_PORT https://+:5001
ENV ASPNETCORE_URLS http://*:5000

#ENV ASPNETCORE_URLS http://*:5000
#
ENV ASPNETCORE_ENVIRONMENT=Development #FOR SWAGGER
#
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["webApiDocker/webApiDocker.csproj", "webApiDocker/"]
RUN dotnet restore "webApiDocker/webApiDocker.csproj"
COPY . .
WORKDIR "/src/webApiDocker"
RUN dotnet build "webApiDocker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "webApiDocker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "webApiDocker.dll"]


#FROM microsoft/dotnet:2.1-sdk AS build
#ENV ASPNETCORE_URLS http://*:44319
#
#ENV ASPNETCORE_ENVIRONMENT=Development #Add this line.
#
#EXPOSE 44319
#WORKDIR /src
#COPY ["LocationServicesAPI/LocationServicesAPI.csproj", "LocationServicesAPI/"]
#RUN dotnet restore "LocationServicesAPI/LocationServicesAPI.csproj"
#
#COPY . .
#WORKDIR /src/LocationServicesAPI/
#
#
#RUN dotnet build LocationServicesAPI.csproj -c Release -o /app
#
#ENTRYPOINT ["dotnet", "LocationServicesAPI.dll"]
#