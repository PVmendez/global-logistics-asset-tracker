# Build stage for C library
FROM alpine:latest AS c-builder

WORKDIR /app/native/c

COPY src/native/c/ .

RUN apk add --no-cache gcc musl-dev make && \
    make clean && \
    make

# Build stage for .NET API
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS dotnet-builder

WORKDIR /app

COPY src/backend/ ./

RUN dotnet restore "Lexart.GLAT.Api/Lexart.GLAT.Api.csproj" && \
    dotnet publish "Lexart.GLAT.Api/Lexart.GLAT.Api.csproj" -c Release -o /app/publish \
    -p:PublishTrimmed=false -p:PublishInvariantGlobalization=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine

WORKDIR /app

# Install runtime dependencies, locale support and curl for healthcheck
RUN apk add --no-cache libstdc++ libc6-compat icu-libs curl

# Set globalization mode and locale
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LANG=en_US.UTF-8
ENV LC_ALL=en_US.UTF-8

# Copy the published .NET application
COPY --from=dotnet-builder /app/publish .

# Copy the built C library
COPY --from=c-builder /app/native/c/libhealthcalculator.so ./

# Set runtime library path
ENV LD_LIBRARY_PATH=/app:$LD_LIBRARY_PATH

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "Lexart.GLAT.Api.dll"]
