# ── Stage 1: build + install Playwright browsers ─────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY WebScrapper.ScraperApi.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out

# Install Playwright Chromium and all OS dependencies during the build stage.
# Browser binaries are copied to the runtime image - no SDK needed at runtime.
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
RUN /root/.nuget/packages/microsoft.playwright/1.59.0/.playwright/node/linux-x64/node \
    /root/.nuget/packages/microsoft.playwright/1.59.0/.playwright/package/cli.js \
    install --with-deps chromium

# ── Stage 2: lean runtime image ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Chromium runtime system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libgbm1 \
    libx11-xcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libasound2t64 \
    libpangocairo-1.0-0 \
    libgtk-3-0t64 \
    && rm -rf /var/lib/apt/lists/*

ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /ms-playwright /ms-playwright
COPY --from=build /out .

RUN chmod -R 755 /ms-playwright

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebScrapper.ScraperApi.dll"]
