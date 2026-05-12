# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ResumeAI.AI.API.csproj", "./"]
RUN dotnet restore "ResumeAI.AI.API.csproj"

COPY . .
RUN dotnet publish "ResumeAI.AI.API.csproj" \
    -c Release \
    -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
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

ENTRYPOINT ["dotnet", "ResumeAI.AI.API.dll"]
