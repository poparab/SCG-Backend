# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY SCG.sln .
COPY src/Presentation/SCG.API/SCG.API.csproj src/Presentation/SCG.API/
COPY src/BuildingBlocks/SCG.Application.Abstractions/SCG.Application.Abstractions.csproj src/BuildingBlocks/SCG.Application.Abstractions/
COPY src/BuildingBlocks/SCG.Infrastructure.Common/SCG.Infrastructure.Common.csproj src/BuildingBlocks/SCG.Infrastructure.Common/
COPY src/BuildingBlocks/SCG.SharedKernel/SCG.SharedKernel.csproj src/BuildingBlocks/SCG.SharedKernel/
COPY src/Modules/AgencyManagement/SCG.AgencyManagement.Application/SCG.AgencyManagement.Application.csproj src/Modules/AgencyManagement/SCG.AgencyManagement.Application/
COPY src/Modules/AgencyManagement/SCG.AgencyManagement.Domain/SCG.AgencyManagement.Domain.csproj src/Modules/AgencyManagement/SCG.AgencyManagement.Domain/
COPY src/Modules/AgencyManagement/SCG.AgencyManagement.Infrastructure/SCG.AgencyManagement.Infrastructure.csproj src/Modules/AgencyManagement/SCG.AgencyManagement.Infrastructure/
COPY src/Modules/Identity/SCG.Identity.Application/SCG.Identity.Application.csproj src/Modules/Identity/SCG.Identity.Application/
COPY src/Modules/Identity/SCG.Identity.Domain/SCG.Identity.Domain.csproj src/Modules/Identity/SCG.Identity.Domain/
COPY src/Modules/Identity/SCG.Identity.Infrastructure/SCG.Identity.Infrastructure.csproj src/Modules/Identity/SCG.Identity.Infrastructure/
COPY src/Modules/InquiryManagement/SCG.InquiryManagement.Application/SCG.InquiryManagement.Application.csproj src/Modules/InquiryManagement/SCG.InquiryManagement.Application/
COPY src/Modules/InquiryManagement/SCG.InquiryManagement.Domain/SCG.InquiryManagement.Domain.csproj src/Modules/InquiryManagement/SCG.InquiryManagement.Domain/
COPY src/Modules/InquiryManagement/SCG.InquiryManagement.Infrastructure/SCG.InquiryManagement.Infrastructure.csproj src/Modules/InquiryManagement/SCG.InquiryManagement.Infrastructure/
COPY src/Modules/Notification/SCG.Notification.Application/SCG.Notification.Application.csproj src/Modules/Notification/SCG.Notification.Application/
COPY src/Modules/Notification/SCG.Notification.Domain/SCG.Notification.Domain.csproj src/Modules/Notification/SCG.Notification.Domain/
COPY src/Modules/Notification/SCG.Notification.Infrastructure/SCG.Notification.Infrastructure.csproj src/Modules/Notification/SCG.Notification.Infrastructure/
COPY src/Modules/RulesAndConfiguration/SCG.Rules.Application/SCG.Rules.Application.csproj src/Modules/RulesAndConfiguration/SCG.Rules.Application/
COPY src/Modules/RulesAndConfiguration/SCG.Rules.Domain/SCG.Rules.Domain.csproj src/Modules/RulesAndConfiguration/SCG.Rules.Domain/
COPY src/Modules/RulesAndConfiguration/SCG.Rules.Infrastructure/SCG.Rules.Infrastructure.csproj src/Modules/RulesAndConfiguration/SCG.Rules.Infrastructure/

RUN dotnet restore src/Presentation/SCG.API/SCG.API.csproj

# Copy everything and build
COPY . .
RUN dotnet publish src/Presentation/SCG.API/SCG.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Run as non-root user
RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser:appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "SCG.API.dll"]
