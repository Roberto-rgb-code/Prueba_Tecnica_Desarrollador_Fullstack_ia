FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY shared/Toka.Shared/Toka.Shared.csproj shared/Toka.Shared/
COPY services/AuthService/AuthService.Domain/AuthService.Domain.csproj services/AuthService/AuthService.Domain/
COPY services/AuthService/AuthService.Application/AuthService.Application.csproj services/AuthService/AuthService.Application/
COPY services/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj services/AuthService/AuthService.Infrastructure/
COPY services/AuthService/AuthService.Api/AuthService.Api.csproj services/AuthService/AuthService.Api/

RUN dotnet restore services/AuthService/AuthService.Api/AuthService.Api.csproj
COPY . .
RUN dotnet publish services/AuthService/AuthService.Api/AuthService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
