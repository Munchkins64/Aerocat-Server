# --- Dockerfile that ONLY builds the server ---

# Stage 1: Use the .NET 9 SDK to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# --- Step 1: Copy ONLY the project files for the server and its dependencies ---
# This avoids copying the client's .csproj file
COPY Aerocat.Server/Aerocat.Server.csproj ./Aerocat.Server/
COPY Aerocat.Shared/Aerocat.Shared.csproj ./Aerocat.Shared/

# --- Step 2: Restore dependencies for ONLY the server project ---
# Because the server project references the shared project, this command
# is smart enough to restore both of them.
RUN dotnet restore "Aerocat.Server/Aerocat.Server.csproj"

# --- Step 3: Copy the rest of the source code for ONLY the needed projects ---
# We explicitly ignore the client project's source code.
COPY Aerocat.Server/. ./Aerocat.Server/
COPY Aerocat.Shared/. ./Aerocat.Shared/

# --- Step 4: Publish ONLY the server project ---
WORKDIR "/source/Aerocat.Server"
RUN dotnet publish "Aerocat.Server.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Create the final, lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Use the PORT environment variable provided by Render
ENV ASPNETCORE_URLS="http://+:${PORT:-10000}"

# Define the entry point for the application
ENTRYPOINT ["dotnet", "Aerocat.Server.dll"]