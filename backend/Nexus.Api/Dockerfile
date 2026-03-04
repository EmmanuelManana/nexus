# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["Nexus.Api/Nexus.Api.csproj", "Nexus.Api/"]
COPY ["Nexus.Application/Nexus.Application.csproj", "Nexus.Application/"]
COPY ["Nexus.Domain/Nexus.Domain.csproj", "Nexus.Domain/"]
COPY ["Nexus.Infrastructure/Nexus.Infrastructure.csproj", "Nexus.Infrastructure/"]

RUN dotnet restore "Nexus.Api/Nexus.Api.csproj"
COPY . .
RUN dotnet build "Nexus.Api/Nexus.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Nexus.Api/Nexus.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .
USER 1000
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget -qO- http://localhost:5000/health || exit 1
ENTRYPOINT ["dotnet", "Nexus.Api.dll"]
