FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY shared/Toka.Shared/Toka.Shared.csproj shared/Toka.Shared/
COPY services/UserService/UserService.Domain/UserService.Domain.csproj services/UserService/UserService.Domain/
COPY services/UserService/UserService.Application/UserService.Application.csproj services/UserService/UserService.Application/
COPY services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj services/UserService/UserService.Infrastructure/
COPY services/UserService/UserService.Api/UserService.Api.csproj services/UserService/UserService.Api/

RUN dotnet restore services/UserService/UserService.Api/UserService.Api.csproj
COPY . .
RUN dotnet publish services/UserService/UserService.Api/UserService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
