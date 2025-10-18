# Use the official .NET SDK image to build the app
# SDK = includes compiler + build tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy project files into the container
COPY *.csproj ./

# Restore dependencies (download NuGet packages)
RUN dotnet restore

# Copy the rest of the source code into the container
COPY . ./

# Build the application in Release mode (optimized)
RUN dotnet publish -c Release -o out


# ----------------------------------------
# Runtime stage — only includes what's needed to run the app
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Set working directory
WORKDIR /app

# Copy the compiled output from the build stage
COPY --from=build /app/out ./

# Tell ASP.NET Core which port to listen on inside the container
# (This is the container port, not the host port)
ENV ASPNETCORE_URLS=http://+:80

# Expose the port (for documentation and K8s)
EXPOSE 80

# Set the entry point (run the app)
ENTRYPOINT ["dotnet", "CommandsService.dll"]
