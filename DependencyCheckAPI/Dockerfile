FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=DEVELOPMENT



FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y unzip wget

# Install dependency-check
RUN mkdir /dependency-check \
    && wget https://github.com/jeremylong/DependencyCheck/releases/download/v8.3.1/dependency-check-8.3.1-release.zip -O /dependency-check/dependency-check.zip \
    && unzip /dependency-check/dependency-check.zip -d /dependency-check


COPY ["DependencyCheckAPI/DependencyCheckAPI.csproj", "DependencyCheckAPI/"]
RUN dotnet restore "DependencyCheckAPI/DependencyCheckAPI.csproj"
COPY . .

WORKDIR "/src/DependencyCheckAPI"
RUN dotnet build "DependencyCheckAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DependencyCheckAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN apt-get update && apt-get install -y openjdk-11-jdk
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /dependency-check/ /app/dependency-check/


ENTRYPOINT ["dotnet", "DependencyCheckAPI.dll"]