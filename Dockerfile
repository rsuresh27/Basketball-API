#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM selenium/standalone-chrome
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Basketball API.csproj", "."]
RUN dotnet restore "./Basketball API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Basketball API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Basketball API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Basketball API.dll"]