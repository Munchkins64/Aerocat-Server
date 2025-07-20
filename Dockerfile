# --- The New, Simpler Dockerfile ---

# Stage 1: Use the .NET SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy ALL files from your GitHub repository into the '/source' directory in the container
COPY . .

# Restore dependencies for the entire solution. This finds your .sln file.
RUN dotnet restore "Aerocat.sln"

# Publish the server project, telling it where to find the .csproj file
RUN dotnet publish "Aerocat.Server/Aerocat.Server.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final, lightweight image with just the runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Use the PORT environment variable provided by Render. This is crucial.
ENV ASPNETCORE_URLS="http://+:${PORT:-10000}"

# Define the entry point for the application
ENTRYPOINT ["dotnet", "Aerocat.Server.dll"]