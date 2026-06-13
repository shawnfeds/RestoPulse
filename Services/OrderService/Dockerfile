FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Shared/Contracts/RestoPulse.Contracts.csproj", "Shared/Contracts/"]
COPY ["Shared/SharedKernel/RestoPulse.SharedKernel.csproj", "Shared/SharedKernel/"]
COPY ["ServiceDefaults/RestoPulse.ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["Services/OrderService/RestoPulse.OrderService.csproj", "Services/OrderService/"]

RUN dotnet restore "Services/OrderService/RestoPulse.OrderService.csproj"

COPY . .
WORKDIR "/src/Services/OrderService"
RUN dotnet publish "RestoPulse.OrderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestoPulse.OrderService.dll"]
