# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY PaySmartly.Persistence.LoadBalancer/*.csproj ./PaySmartly.Persistence.LoadBalancer/
RUN dotnet restore

# copy everything else and build app
COPY PaySmartly.Persistence.LoadBalancer/. ./PaySmartly.Persistence.LoadBalancer/
WORKDIR /source/PaySmartly.Persistence.LoadBalancer
RUN dotnet publish -c release -o /PaySmartly.Persistence.LoadBalancer --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

LABEL author="Stefan Bozov"

WORKDIR /PaySmartly.Persistence.LoadBalancer
COPY --from=build /PaySmartly.Persistence.LoadBalancer ./
ENTRYPOINT ["dotnet", "PaySmartly.Persistence.LoadBalancer.dll"]