FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8002
ENV ASPNETCORE_URLS=http://+:8002

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["nuget.config", "."]
COPY ["src/DirectoryService.Presentation/DirectoryService.Presentation.csproj", "src/DirectoryService.Presentation/"]
COPY ["src/DirectoryService.Application/DirectoryService.Application.csproj", "src/DirectoryService.Application/"]
COPY ["src/DirectoryService.Domain/DirectoryService.Domain.csproj", "src/DirectoryService.Domain/"]
COPY ["src/DirectoryService.Contracts/DirectoryService.Contracts.csproj", "src/DirectoryService.Contracts/"]
COPY ["src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj", "src/DirectoryService.Infrastructure/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

ARG GITHUB_TOKEN

RUN dotnet nuget update source mazeland-private --username "maze37" --password "${GITHUB_TOKEN}" --store-password-in-clear-text && \
    dotnet restore "src/DirectoryService.Presentation/DirectoryService.Presentation.csproj"

COPY . .
WORKDIR "/src/src/DirectoryService.Presentation"
RUN dotnet build "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]