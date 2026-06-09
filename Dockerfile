# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["fifa-backend.csproj", "./"]
RUN dotnet restore "fifa-backend.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "fifa-backend.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "fifa-backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

# Run as non-root user (provided by default dotnet aspnet image: user 'app')
USER app

ENTRYPOINT ["dotnet", "fifa-backend.dll"]
