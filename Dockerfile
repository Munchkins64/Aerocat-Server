# --- The Final, Most Robust Dockerfile ---

# Stage 1: Use the .NET 9 SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy ALL files from your GitHub repository into the '/source' directory.
# This is the simplest way and avoids all path guessing errors.
COPY . .

# *** THE KEY FIX IS HERE ***
# Instead of restoring the whole solution, we restore ONLY the server project.
# This automatically restores its dependencies (like Aerocat.Shared)
# but completely IGNORES the problematic Windows client project.
RUN dotnet restore "Aerocat.Server/Aerocat.Server.csproj"

# Now, publish the server project. This command was already correct.
RUN dotnet publish "Aerocat.Server/Aerocat.Server.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final, lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Use the PORT environment variable provided by Render
ENV ASPNETCORE_URLS="http://+:${PORT:-10000}"

# Define the entry point for the application
ENTRYPOINT ["dotnet", "Aerocat.Server.dll"]