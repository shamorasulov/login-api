# STAGE 1: Build
# Use the .NET 10 SDK image to compile the code
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["login-api.csproj", "./"]
RUN dotnet restore "login-api.csproj"

# Copy the rest of the code and build it
COPY . .
RUN dotnet build "login-api.csproj" -c Release -o /app/build

# STAGE 2: Publish
FROM build AS publish
RUN dotnet publish "login-api.csproj" -c Release -o /app/publish

# STAGE 3: Final Runtime
# Use a smaller image that only has the runtime (not the full SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "login-api.dll"]