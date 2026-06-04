FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for restore
COPY ["Services/BillingService/RestoPulse.BillingService.csproj", "Services/BillingService/"]
COPY ["ServiceDefaults/RestoPulse.ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["Shared/Contracts/RestoPulse.Contracts.csproj", "Shared/Contracts/"]

# Restore dependencies
RUN dotnet restore "Services/BillingService/RestoPulse.BillingService.csproj"

# Copy remaining source code
COPY . .

# Build and publish
WORKDIR "/src/Services/BillingService"
RUN dotnet publish "RestoPulse.BillingService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "RestoPulse.BillingService.dll"]
