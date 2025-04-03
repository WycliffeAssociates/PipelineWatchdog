FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PipelineWatchdog/PipelineWatchdog.csproj", "PipelineWatchdog/"]
COPY ["Logic/Logic.csproj", "Logic/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Implementation/Implementation.csproj", "Implementation/"]
RUN dotnet restore "PipelineWatchdog/PipelineWatchdog.csproj"
COPY . .
WORKDIR "/src/PipelineWatchdog"
RUN dotnet build "PipelineWatchdog.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PipelineWatchdog.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PipelineWatchdog.dll"]
