FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=DEVELOPMENT

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DependencyCheckAPI/DependencyCheckAPI.csproj", "DependencyCheckAPI/"]
RUN dotnet restore "DependencyCheckAPI/DependencyCheckAPI.csproj"
COPY . .
WORKDIR "/src/DependencyCheckAPI"
RUN dotnet build "DependencyCheckAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DependencyCheckAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DependencyCheckAPI.dll"]