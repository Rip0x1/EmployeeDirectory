FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
RUN apt-get update && apt-get install -y krb5-user libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EmployeeDirectory.csproj", "./"]
RUN dotnet restore "EmployeeDirectory.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "EmployeeDirectory.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeDirectory.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeDirectory.dll"]



