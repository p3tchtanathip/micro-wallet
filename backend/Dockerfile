# Stage 1: Base SDK (used for Dev and Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /src

# Stage 2: Development (Hot-reload)
FROM base AS development
ENV ASPNETCORE_ENVIRONMENT=Development
# The source will be mounted via volumes in docker-compose
CMD ["dotnet", "watch", "run", "--project", "src/API/API.csproj", "--no-launch-profile"]

# Stage 3: Build & Publish
FROM base AS build
COPY . .
RUN dotnet restore
RUN dotnet publish "src/API/API.csproj" -c Release -o /app/publish

# Stage 4: Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production
WORKDIR /app
COPY --from=build /app/publish .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]