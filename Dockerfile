# Stage 1: Build the application using the .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy the solution file and project files first to leverage Docker layer caching
COPY *.sln .
COPY Aerocat.Server/*.csproj ./Aerocat.Server/
COPY Aerocat.Shared/*.csproj ./Aerocat.Shared/
# The client project isn't strictly needed for the server build, but this is a robust way to copy
COPY Aerocat.Client/*.csproj ./Aerocat.Client/

# Restore all NuGet packages for the entire solution
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Publish only the server project, creating a release-ready build
WORKDIR "/source/Aerocat.Server"
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Create the final, smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render provides a PORT environment variable. We tell ASP.NET Core to listen on it.
# The default is 10000 if PORT is not set.
ENV ASPNETCORE_URLS="http://+:${PORT:-10000}"

# The command to run the application
ENTRYPOINT ["dotnet", "Aerocat.Server.dll"]