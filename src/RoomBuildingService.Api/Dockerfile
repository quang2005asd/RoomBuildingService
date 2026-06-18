FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY RoomBuildingService.sln .
COPY src/RoomBuildingService.Core/RoomBuildingService.Core.csproj src/RoomBuildingService.Core/
COPY src/RoomBuildingService.Infrastructure/RoomBuildingService.Infrastructure.csproj src/RoomBuildingService.Infrastructure/
COPY src/RoomBuildingService.Api/RoomBuildingService.Api.csproj src/RoomBuildingService.Api/
RUN dotnet restore src/RoomBuildingService.Api/RoomBuildingService.Api.csproj

COPY . .
WORKDIR /src/src/RoomBuildingService.Api
RUN dotnet publish RoomBuildingService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RoomBuildingService.Api.dll"]