FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG VERSION="2.0.0"
ARG REVISION="0"
WORKDIR /src
COPY TunnelGPT/TunnelGPT.csproj TunnelGPT/
RUN dotnet restore TunnelGPT/TunnelGPT.csproj
COPY TunnelGPT/ TunnelGPT/
RUN dotnet publish ./TunnelGPT \
                   -c Release  \
                   -o /app/publish \
                   --self-contained false \
                   -p:ContinuousIntegrationBuild=true \
                   -p:Version=${VERSION} \
                   -p:InformationalVersion=${VERSION}+${REVISION}

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /opt
COPY --from=build /app/publish tunnelgpt
ENTRYPOINT ["dotnet", "tunnelgpt/TunnelGPT.dll"]
