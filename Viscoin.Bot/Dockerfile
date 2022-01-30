FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Viscoin.Bot/Viscoin.Bot.csproj", "Viscoin.Bot/"]
RUN dotnet restore "Viscoin.Bot/Viscoin.Bot.csproj"
COPY . .
WORKDIR "/src/Viscoin.Bot"
RUN dotnet build "Viscoin.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Viscoin.Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Viscoin.Bot.dll"]
