FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dependencyApi.csproj", "dependencyApi/"]
RUN dotnet restore "dependencyApi/dependencyApi.csproj"
COPY . dependencyApi/
WORKDIR "/src/dependencyApi"
RUN dotnet build "dependencyApi.csproj" -c $BUILD_CONFIGURATION --use-current-runtime -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "dependencyApi.csproj" -c $BUILD_CONFIGURATION --use-current-runtime -o /app/publish /p:UseAppHost=false
RUN chmod +x /app/publish/instrument.sh 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["./instrument.sh", "dotnet", "dependencyApi.dll"]
