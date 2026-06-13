FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Shared/Contracts/RestoPulse.Contracts.csproj", "Shared/Contracts/"]
COPY ["Shared/SharedKernel/RestoPulse.SharedKernel.csproj", "Shared/SharedKernel/"]
COPY ["ServiceDefaults/RestoPulse.ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["Services/TableService/RestoPulse.TableService.csproj", "Services/TableService/"]

RUN dotnet restore "Services/TableService/RestoPulse.TableService.csproj"

COPY . .
WORKDIR "/src/Services/TableService"
RUN dotnet publish "RestoPulse.TableService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestoPulse.TableService.dll"]
