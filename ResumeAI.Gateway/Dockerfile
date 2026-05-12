# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ResumeAI.Gateway.csproj", "./"]
RUN dotnet restore "ResumeAI.Gateway.csproj"

COPY . .
RUN dotnet publish "ResumeAI.Gateway.csproj" \
    -c Release \
    -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

COPY --from=build /app/publish .

# Logs directory
RUN mkdir -p /app/logs && chown -R appuser:appgroup /app/logs

USER appuser

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "ResumeAI.Gateway.dll"]
