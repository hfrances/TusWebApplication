#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["TusWebApplication/TusWebApplication.csproj", "TusWebApplication/"]
RUN dotnet restore "TusWebApplication/TusWebApplication.csproj"
COPY . .
WORKDIR "/src/TusWebApplication"
RUN dotnet build "TusWebApplication.csproj" -c Release -o /app/build --framework netcoreapp3.1

FROM build AS publish
RUN dotnet publish "TusWebApplication.csproj" -c Release -o /app/publish /p:UseAppHost=false --framework netcoreapp3.1

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TusWebApplication.dll"]