FROM mcr.microsoft.com/dotnet/sdk:5.0 AS sdk
WORKDIR /Shard.UNGNUNES

COPY *.csproj ./
RUN dotnet restore Shard.UNGNUNES

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /Shard.UNGNUNES
EXPOSE 80
COPY --from=sdk /Shard.UNGNUNES/out .
ENTRYPOINT ["dotnet", "Shard.UNGNUNES.dll"]