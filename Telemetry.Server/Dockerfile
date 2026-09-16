# 1. Aşama: Derleme (Build) Aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sadece proje dosyasını kopyalayıp bağımlılıkları yükle (Cache optimizasyonu için)
COPY ["Telemetry.Server/Telemetry.Server.csproj", "Telemetry.Server/"]
RUN dotnet restore "Telemetry.Server/Telemetry.Server.csproj"

# Tüm kodları kopyala ve projeyi derle
COPY . .
WORKDIR "/src/Telemetry.Server"
RUN dotnet build "Telemetry.Server.csproj" -c Release -o /app/build

# 2. Aşama: Yayınlama (Publish) Aşaması
RUN dotnet publish "Telemetry.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Aşama: Çalıştırma (Runtime) Aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Sadece derlenmiş temiz dosyaları bir önceki aşamadan al
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Telemetry.Server.dll"]