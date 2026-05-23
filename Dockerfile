# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY salasries7/*.csproj salasries7/
RUN dotnet restore salasries7/salasries7.csproj

# Copy everything else and build
COPY salasries7/ salasries7/
RUN dotnet publish salasries7/salasries7.csproj -c Release -o /app --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app .

# Expose the port (Render uses PORT env var)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "salasries7.dll"]
