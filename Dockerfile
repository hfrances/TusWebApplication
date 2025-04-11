ARG IMGFRAMEWORK=6.0
ARG FRAMEWORK=net6.0

FROM mcr.microsoft.com/dotnet/aspnet:$IMGFRAMEWORK-alpine AS runtime
ARG FRAMEWORK
ENV ASPNETCORE_URLS=http://+:5000;http://+:80

WORKDIR /app
COPY ./$FRAMEWORK .
ENTRYPOINT ["dotnet", "TusWebApplication.dll"]
