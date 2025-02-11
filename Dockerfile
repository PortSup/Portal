# Use the official .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET SDK image for build environment
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VendorPortalAPI/VendorPortalAPI.csproj", "VendorPortalAPI/"]
RUN dotnet restore "VendorPortalAPI/VendorPortalAPI.csproj"
COPY . .
WORKDIR "/src/VendorPortalAPI"
RUN dotnet build "VendorPortalAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VendorPortalAPI.csproj" -c Release -o /app/publish

# Final stage, copy the build to the base image and run
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VendorPortalAPI.dll"]
