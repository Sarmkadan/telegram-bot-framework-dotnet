# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["src/TelegramBotFramework/TelegramBotFramework.csproj", "src/TelegramBotFramework/"]

RUN dotnet restore "src/TelegramBotFramework/TelegramBotFramework.csproj"

COPY . .

RUN dotnet build "src/TelegramBotFramework/TelegramBotFramework.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "src/TelegramBotFramework/TelegramBotFramework.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/bot/health || exit 1

ENTRYPOINT ["dotnet", "TelegramBotFramework.dll"]
