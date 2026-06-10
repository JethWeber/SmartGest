FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SmartGest.Core/SmartGest.Core.csproj  SmartGest.Core/
COPY SmartGest.API/SmartGest.API.csproj    SmartGest.API/

RUN dotnet restore SmartGest.API/SmartGest.API.csproj

COPY SmartGest.Core/  SmartGest.Core/
COPY SmartGest.API/   SmartGest.API/

WORKDIR /src/SmartGest.API
RUN dotnet publish SmartGest.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartGest.API.dll"]
