# Build stage - .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Copiar proyectos y restaurar (optimización de caché)
COPY ["XPos.Api/XPos.Api.csproj", "XPos.Api/"]
COPY ["XPos.Domain/XPos.Domain.csproj", "XPos.Domain/"]
COPY ["XPos.Data/XPos.Data.csproj", "XPos.Data/"]
RUN dotnet restore "XPos.Api/XPos.Api.csproj"

# 2. Copiar el resto del código y publicar
COPY . .
WORKDIR "/src/XPos.Api"
RUN dotnet publish "XPos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - .NET 10 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instalar utilitarios necesarios para salud y diagnóstico
RUN apt-get update && apt-get install -y postgresql-client curl procps && rm -rf /var/lib/apt/lists/*

# Configuración de seguridad: Usuario no-root
RUN groupadd -r appgroup && useradd -r -g appgroup appuser
COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

# Cambiar a usuario no-root
USER appuser

# Configuración de entorno
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Ejecución
ENTRYPOINT ["dotnet", "XPos.Api.dll"]
