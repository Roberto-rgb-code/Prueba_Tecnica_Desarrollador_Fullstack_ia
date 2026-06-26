FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY shared/Toka.Shared/Toka.Shared.csproj shared/Toka.Shared/
COPY services/AuditService/AuditService.Domain/AuditService.Domain.csproj services/AuditService/AuditService.Domain/
COPY services/AuditService/AuditService.Application/AuditService.Application.csproj services/AuditService/AuditService.Application/
COPY services/AuditService/AuditService.Infrastructure/AuditService.Infrastructure.csproj services/AuditService/AuditService.Infrastructure/
COPY services/AuditService/AuditService.Api/AuditService.Api.csproj services/AuditService/AuditService.Api/

RUN dotnet restore services/AuditService/AuditService.Api/AuditService.Api.csproj
COPY . .
RUN dotnet publish services/AuditService/AuditService.Api/AuditService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuditService.Api.dll"]
