# --- Dockerfile for .NET 9 ---

# Stage 1: Use the .NET 9 SDK to build the project
# This line was changed from 8.0 to 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy ALL files from your GitHub repository into the '/source' directory
COPY . .

# Restore dependencies for the entire solution
RUN dotnet restore "Aerocat.sln"

# Publish the server project, creating a release-ready build
RUN dotnet publish "Aerocat.Server/Aerocat.Server.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final, lightweight image with the .NET 9 runtime
# This line was also changed from 8.0 to 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Use the PORT environment variable provided by Render
ENV ASPNETCORE_URLS="http://+:${PORT:-10000}"

# Define the entry point for the application
ENTRYPOINT ["dotnet", "Aerocat.Server.dll"]