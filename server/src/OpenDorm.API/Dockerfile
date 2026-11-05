FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OpenDorm.API/OpenDorm.API.csproj", "src/OpenDorm.API/"]
COPY ["src/OpenDorm.Application/OpenDorm.Application.csproj", "src/OpenDorm.Application/"]
COPY ["src/OpenDorm.Domain/OpenDorm.Domain.csproj", "src/OpenDorm.Domain/"]
COPY ["src/OpenDorm.Infrastructure/OpenDorm.Infrastructure.csproj", "src/OpenDorm.Infrastructure/"]
RUN dotnet restore "src/OpenDorm.API/OpenDorm.API.csproj"
COPY . .
WORKDIR "/src/src/OpenDorm.API"
RUN dotnet build "./OpenDorm.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OpenDorm.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenDorm.API.dll"]
