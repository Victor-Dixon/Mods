# Cities Regional Server - Docker Image
# 
# Build:   docker build -t cities-regional-server .
# Run:     docker run -p 5000:5000 cities-regional-server

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["RegionalServer.csproj", "./"]
RUN dotnet restore

# Copy source and build
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RegionalServer.dll"]

