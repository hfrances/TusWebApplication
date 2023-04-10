##
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime

ENV ASPNETCORE_URLS http://+:5000
WORKDIR /app
COPY ./net6.0 .
ENTRYPOINT ["dotnet", "TusWebApplication.dll"]
