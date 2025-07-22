FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY TunnelGPT/TunnelGPT.csproj TunnelGPT/
RUN dotnet restore TunnelGPT/TunnelGPT.csproj
COPY TunnelGPT/ TunnelGPT/
RUN dotnet publish ./TunnelGPT -c Release --self-contained false -p:ContinuousIntegrationBuild=true -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /opt
COPY --from=build /app/publish tunnelgpt
ENTRYPOINT ["dotnet", "tunnelgpt/TunnelGPT.dll"]
