#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TusWebApplication.csproj", "."]
RUN dotnet restore "TusWebApplication.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "TusWebApplication.csproj" -c Release -o /app/build --framework net6.0

FROM build AS publish
RUN dotnet publish "TusWebApplication.csproj" -c Release -o /app/publish /p:UseAppHost=false --framework net6.0

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://+:5000
ENTRYPOINT ["dotnet", "TusWebApplication.dll"]