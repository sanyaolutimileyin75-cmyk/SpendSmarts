# ===== STAGE 1: Build the app =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the csproj from the subfolder
COPY SpendSmart2/*.csproj ./SpendSmart2/
RUN dotnet restore SpendSmart2/SpendSmart2.csproj

# Copy everything else
COPY SpendSmart2/. ./SpendSmart2/
WORKDIR /app/SpendSmart2
RUN dotnet publish -c Release -o /app/out

# ===== STAGE 2: Run the app =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose port 8080 (Render uses this)
EXPOSE 8080

# Set environment variable for port
ENV ASPNETCORE_URLS=http://+:8080

# Start the app
ENTRYPOINT ["dotnet", "SpendSmart2.dll"]