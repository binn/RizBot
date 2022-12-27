FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /bot

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore
COPY . .

FROM build AS publish
RUN dotnet publish "RizBot.csproj" -c Release -o /bot/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /bot
COPY --from=publish /bot/publish .
ENTRYPOINT ["dotnet", "RizBot.dll"]