# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["GrpcStreamingDemo/GrpcStreamingDemo.csproj", "GrpcStreamingDemo/"]

# Restore dependencies
RUN dotnet restore "GrpcStreamingDemo/GrpcStreamingDemo.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/GrpcStreamingDemo"
RUN dotnet build "GrpcStreamingDemo.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GrpcStreamingDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 7007

ENTRYPOINT ["dotnet", "GrpcStreamingDemo.dll"]