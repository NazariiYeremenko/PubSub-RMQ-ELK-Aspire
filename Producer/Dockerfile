#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0.3 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.202 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Producer/Producer.csproj", "Producer/"]
RUN dotnet restore "./Producer/./Producer.csproj"
COPY . .
WORKDIR "/src/Producer"
RUN dotnet build "./Producer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Producer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY appsettings.json /app
ENTRYPOINT ["dotnet", "Producer.dll"]