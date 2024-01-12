FROM mcr.microsoft.com/dotnet/sdk:5.0 AS sdk
WORKDIR /app
EXPOSE 80

WORKDIR /src
COPY shard.sln ./
COPY Shard.Shared.Core/*.csproj ./Shard.Shared.Core/
COPY Shard.UNGNUNES/*.csproj ./Shard.UNGNUNES/
COPY Shard.Shared.Web.IntegrationTests/*.csproj ./Shard.Shared.Web.IntegrationTests/
COPY Shard.UNGNUNES.Tests/*.csproj ./Shard.UNGNUNES.Tests/

RUN dotnet restore
COPY . .

WORKDIR /src/Shard.Shared.Core
RUN dotnet build -c Release -o /app/out

WORKDIR /src/Shard.UNGNUNES
RUN dotnet build -c Release -o /app/out

WORKDIR /src/Shard.UNGNUNES.Tests
RUN dotnet build -c Release -o /app/out

WORKDIR /src/Shard.Shared.Web.IntegrationTests
RUN dotnet build -c Release -o /app/out

RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=sdk /app/out .
ENTRYPOINT ["dotnet", "Shard.UNGNUNES.dll"]