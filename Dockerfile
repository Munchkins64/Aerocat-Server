# --- Dockerfile with CORRECTED hyphenated paths ---

# Stage 1: Use the .NET 9 SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy ALL files from your GitHub repository into the '/source' directory.
COPY . .

# *** THE KEY FIX IS HERE: Use the correct hyphenated project path ***
RUN dotnet restore "Aerocat-Server/Aerocat-Server.csproj"

# Publish the server project using the correct hyphenated path
RUN dotnet publish "Aerocat-Server/Aerocat-Server.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final, lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Use the PORT environment variable provided by Render
ENV ASPNETCORE_URLS="http://+:${PORT:-10000}"

# Define the entry point for the application, using the correct hyphenated DLL name
ENTRYPOINT ["dotnet", "Aerocat-Server.dll"]