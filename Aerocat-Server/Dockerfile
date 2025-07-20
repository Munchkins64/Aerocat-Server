# Specify the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy the rest of the application's code
COPY . .
# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Specify the .NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# The port Render will expose
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# The entry point for the application
ENTRYPOINT ["dotnet", "Aerocat.Server.dll"]