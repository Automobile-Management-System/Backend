# Stage 1: The "build" stage
# Uses the .NET 9.0 SDK image, which contains all the tools needed 
# to restore dependencies, compile, and publish the application.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the project file first.
# This optimizes Docker's layer caching. If the .csproj file hasn't changed,
# Docker will use the cached layer from the 'dotnet restore' step below,
# saving time by not re-downloading dependencies.
COPY ["automobile-backend.csproj", "."]
RUN dotnet restore "./automobile-backend.csproj"

# Copy the rest of the source code into the container.
COPY . .

# Build and publish the application in 'Release' mode.
# -o /app/publish: Specifies the output directory inside this container.
# /p:UseAppHost=false: Creates a framework-dependent app (a .dll) instead of
# a self-contained executable. This is standard for running with the 'dotnet' command.
RUN dotnet publish "automobile-backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---

# Stage 2: The "final" runtime stage
# Uses the lightweight ASP.NET 9.0 runtime image, which is much smaller
# than the SDK image because it doesn't include the compilers.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy the published application (and only the published app)
# from the 'build' stage into this final image.
COPY --from=build /app/publish .

# Set the URL for the Kestrel web server to listen on port 5001
# The "+:" syntax means it will listen on port 5001 for any IP address
# within the container.
ENV ASPNETCORE_URLS=http://+:5001

# Set the environment to Development.
# For a production image, you would typically set this to "Production"
# (e.g., via a docker run command or Kubernetes config).
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose port 5001. This is metadata to inform Docker that the
# container listens on this port. You still need to map it using '-p'
# when running the container (e.g., docker run -p 8080:5001 ...).
EXPOSE 5001

# The entry point for the container. This is the command that
# will be run when the container starts. It executes the application's
# DLL using the 'dotnet' runtime.
ENTRYPOINT ["dotnet", "automobile-backend.dll"]