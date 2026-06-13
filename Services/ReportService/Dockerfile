FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Shared/Contracts/RestoPulse.Contracts.csproj", "Shared/Contracts/"]
COPY ["Shared/SharedKernel/RestoPulse.SharedKernel.csproj", "Shared/SharedKernel/"]
COPY ["ServiceDefaults/RestoPulse.ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["Services/ReportService/RestoPulse.ReportService.csproj", "Services/ReportService/"]

RUN dotnet restore "Services/ReportService/RestoPulse.ReportService.csproj"

COPY . .
WORKDIR "/src/Services/ReportService"
RUN dotnet publish "RestoPulse.ReportService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestoPulse.ReportService.dll"]
