FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY shared/Toka.Shared/Toka.Shared.csproj shared/Toka.Shared/
COPY services/AiAgentService/AiAgentService.Domain/AiAgentService.Domain.csproj services/AiAgentService/AiAgentService.Domain/
COPY services/AiAgentService/AiAgentService.Application/AiAgentService.Application.csproj services/AiAgentService/AiAgentService.Application/
COPY services/AiAgentService/AiAgentService.Infrastructure/AiAgentService.Infrastructure.csproj services/AiAgentService/AiAgentService.Infrastructure/
COPY services/AiAgentService/AiAgentService.Api/AiAgentService.Api.csproj services/AiAgentService/AiAgentService.Api/

RUN dotnet restore services/AiAgentService/AiAgentService.Api/AiAgentService.Api.csproj
COPY . .
RUN dotnet publish services/AiAgentService/AiAgentService.Api/AiAgentService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AiAgentService.Api.dll"]
