FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore
COPY ["Services/MenuService/RestoPulse.MenuService.csproj", "Services/MenuService/"]
COPY ["ServiceDefaults/RestoPulse.ServiceDefaults.csproj", "ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "Services/MenuService/RestoPulse.MenuService.csproj"

# Copy remaining source code
COPY . .

# Build and publish
WORKDIR "/src/Services/MenuService"
RUN dotnet publish "RestoPulse.MenuService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RestoPulse.MenuService.dll"]
