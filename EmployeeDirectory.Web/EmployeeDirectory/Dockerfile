FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y krb5-user libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["EmployeeDirectory.Web/EmployeeDirectory/EmployeeDirectory.csproj", "EmployeeDirectory.Web/EmployeeDirectory/"]
COPY ["EmployeeDirectory.Web/EmployeeDirectory.Models/EmployeeDirectory.Models.csproj", "EmployeeDirectory.Web/EmployeeDirectory.Models/"]

RUN dotnet restore "EmployeeDirectory.Web/EmployeeDirectory/EmployeeDirectory.csproj"

COPY . .

WORKDIR "/src/EmployeeDirectory.Web/EmployeeDirectory"
RUN dotnet build "EmployeeDirectory.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeDirectory.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeDirectory.dll"]