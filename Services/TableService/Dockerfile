FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore
COPY ["Services/TableService/RestoPulse.TableService.csproj", "Services/TableService/"]
COPY ["ServiceDefaults/RestoPulse.ServiceDefaults.csproj", "ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "Services/TableService/RestoPulse.TableService.csproj"

# Copy remaining source code
COPY . .

# Build and publish
WORKDIR "/src/Services/TableService"
RUN dotnet publish "RestoPulse.TableService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RestoPulse.TableService.dll"]
