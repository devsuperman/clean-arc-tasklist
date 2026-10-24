# Build context: repository root. docker build -f src/TaskList.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Restore first, so the layer is cached while only source code changes.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/TaskList.Domain/TaskList.Domain.csproj src/TaskList.Domain/
COPY src/TaskList.Application/TaskList.Application.csproj src/TaskList.Application/
COPY src/TaskList.Infrastructure/TaskList.Infrastructure.csproj src/TaskList.Infrastructure/
COPY src/TaskList.Api/TaskList.Api.csproj src/TaskList.Api/
RUN dotnet restore src/TaskList.Api/TaskList.Api.csproj

COPY src/ src/
RUN dotnet publish src/TaskList.Api/TaskList.Api.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# Run as the non-root user provided by the .NET images.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskList.Api.dll"]
