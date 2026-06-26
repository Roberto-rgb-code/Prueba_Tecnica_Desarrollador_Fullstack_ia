FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY shared/Toka.Shared/Toka.Shared.csproj shared/Toka.Shared/
COPY services/RoleService/RoleService.Domain/RoleService.Domain.csproj services/RoleService/RoleService.Domain/
COPY services/RoleService/RoleService.Application/RoleService.Application.csproj services/RoleService/RoleService.Application/
COPY services/RoleService/RoleService.Infrastructure/RoleService.Infrastructure.csproj services/RoleService/RoleService.Infrastructure/
COPY services/RoleService/RoleService.Api/RoleService.Api.csproj services/RoleService/RoleService.Api/

RUN dotnet restore services/RoleService/RoleService.Api/RoleService.Api.csproj
COPY . .
RUN dotnet publish services/RoleService/RoleService.Api/RoleService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RoleService.Api.dll"]
