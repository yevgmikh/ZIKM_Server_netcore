FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app
VOLUME [ "/app/Data", "/app/Logs", "/app/Captchas" ]
ENV Storage=Files Server=localhost UserID=root Password=pass123 DBName=ZikmDB
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["ZIKM.Server/ZIKM.csproj", "ZIKM.Server/"]
RUN dotnet restore "ZIKM.Server/ZIKM.csproj"
COPY . .
WORKDIR /src/ZIKM.Server
RUN dotnet build "ZIKM.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZIKM.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZIKM.dll"]
