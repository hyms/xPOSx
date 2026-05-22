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

# Final stage - .NET 10 Runtime Alpine (Mucho más ligero)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

# Instalar utilitarios necesarios usando apk en Alpine (limpia caché automáticamente)
RUN apk add --no-cache postgresql-client curl procps

# Configuración de seguridad: Usuario no-root (Alpine nativo)
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Copiar y asignar dueño en un solo paso (Evita duplicar tamaño en capas)
COPY --from=build --chown=appuser:appgroup /app/publish .

# Cambiar a usuario no-root
USER appuser

# Configuración de entorno
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Ejecución
ENTRYPOINT ["dotnet", "XPos.Api.dll"]
