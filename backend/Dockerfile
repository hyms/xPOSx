# Development stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev
WORKDIR /src
COPY ["XPos.Api/XPos.Api.csproj", "XPos.Api/"]
COPY ["XPos.Domain/XPos.Domain.csproj", "XPos.Domain/"]
COPY ["XPos.Data/XPos.Data.csproj", "XPos.Data/"]
RUN dotnet restore "XPos.Api/XPos.Api.csproj"
COPY . .
WORKDIR "/src/XPos.Api"
CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["XPos.Api/XPos.Api.csproj", "XPos.Api/"]
COPY ["XPos.Domain/XPos.Domain.csproj", "XPos.Domain/"]
COPY ["XPos.Data/XPos.Data.csproj", "XPos.Data/"]
RUN dotnet restore "XPos.Api/XPos.Api.csproj"
COPY . .
WORKDIR "/src/XPos.Api"
RUN dotnet build "XPos.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "XPos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "XPos.Api.dll"]
