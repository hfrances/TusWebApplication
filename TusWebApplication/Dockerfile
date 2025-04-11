#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG IMGFRAMEWORK=3.1
ARG FRAMEWORK=netcoreapp3.1

FROM mcr.microsoft.com/dotnet/aspnet:$IMGFRAMEWORK AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:$IMGFRAMEWORK AS build
ARG BUILD_CONFIGURATION=Release
ARG FRAMEWORK
ARG RELATIVEPATH
WORKDIR /src
COPY ["TusWebApplication/TusWebApplication.csproj", "TusWebApplication/"]
COPY ["nuget.config", "nuget.config"]
RUN find . -iname "*.csproj" -exec echo {} \;
COPY . .
RUN find . -iname "*.csproj" -exec sed -i "s|<TargetFrameworks>.*</TargetFrameworks>|<TargetFrameworks>$FRAMEWORK</TargetFrameworks>|" {} \;
WORKDIR "/src/TusWebApplication"
RUN echo "Build framework: $FRAMEWORK"
RUN dotnet build "TusWebApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build --framework $FRAMEWORK

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
WORKDIR "/src"
RUN echo "Publish framework: $FRAMEWORK"
RUN dotnet publish "TusWebApplication/TusWebApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --framework $FRAMEWORK

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TusWebApplication.dll"]