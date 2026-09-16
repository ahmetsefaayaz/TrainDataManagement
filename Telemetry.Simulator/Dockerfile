# 1. Aşama: Derleme (Build) Aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Telemetry.Simulator/Telemetry.Simulator.csproj", "Telemetry.Simulator/"]
RUN dotnet restore "Telemetry.Simulator/Telemetry.Simulator.csproj"

COPY . .
WORKDIR "/src/Telemetry.Simulator"
RUN dotnet build "Telemetry.Simulator.csproj" -c Release -o /app/build
RUN dotnet publish "Telemetry.Simulator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Aşama: Çalıştırma (Runtime) Aşaması
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Telemetry.Simulator.dll"]