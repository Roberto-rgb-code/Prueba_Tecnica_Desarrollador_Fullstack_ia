FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY shared/Toka.Shared/Toka.Shared.csproj shared/Toka.Shared/
COPY gateway/Gateway.Api/Gateway.Api.csproj gateway/Gateway.Api/
RUN dotnet restore gateway/Gateway.Api/Gateway.Api.csproj
COPY shared/Toka.Shared/ shared/Toka.Shared/
COPY gateway/Gateway.Api/ gateway/Gateway.Api/
RUN dotnet publish gateway/Gateway.Api/Gateway.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
