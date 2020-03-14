FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["ZIKM.Server/ZIKM.Server.csproj", "ZIKM.Server/"]
RUN dotnet restore "ZIKM.Server/ZIKM.Server.csproj"
COPY . .
WORKDIR /src/ZIKM.Server
RUN dotnet build "ZIKM.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZIKM.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZIKM.Server.dll"]
