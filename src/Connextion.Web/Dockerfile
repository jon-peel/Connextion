FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Connextion.Web/Connextion.Web.csproj", "src/Connextion.Web/"]
COPY ["src/Connextion/Connextion.fsproj", "src/Connextion/"]
RUN dotnet restore "src/Connextion.Web/Connextion.Web.csproj"
COPY . .
WORKDIR "/src/src/Connextion.Web"
RUN dotnet build "Connextion.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Connextion.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Connextion.Web.dll"]
