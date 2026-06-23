# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy project files and restore dependencies
COPY RMASystem.slnx ./
COPY RMA.Shared/RMA.Shared.csproj ./RMA.Shared/
COPY RMA.Server/RMA.Server.csproj ./RMA.Server/
RUN dotnet restore RMA.Server/RMA.Server.csproj

# Copy source code and publish
COPY RMA.Shared/ ./RMA.Shared/
COPY RMA.Server/ ./RMA.Server/
RUN dotnet publish RMA.Server/RMA.Server.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Install native dependencies for Tesseract OCR and general compatibility
RUN apk add --no-cache \
    tesseract-ocr \
    libc6-compat \
    icu-libs

# Disable globalization invariant mode for proper Unicode/culture support
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy compiled files from build stage
COPY --from=build-env /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "RMA.Server.dll"]
