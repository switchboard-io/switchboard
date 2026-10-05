# Switchboard all-in-one server image.
# Build context is the repo root: docker build -f packages/dotnet/Switchboard.Server/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY assets ./assets
COPY packages/dotnet/Switchboard.Evaluation ./packages/dotnet/Switchboard.Evaluation
COPY packages/dotnet/Switchboard.Server ./packages/dotnet/Switchboard.Server
RUN dotnet publish packages/dotnet/Switchboard.Server -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Switchboard.Server.dll"]
