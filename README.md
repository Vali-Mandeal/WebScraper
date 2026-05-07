# WebScraper

A self-hosted scraper that watches classified-ads sites and pings me the moment a listing matches what I'm looking for. I built it because manually refreshing marketplace pages all day is a terrible way to find good deals, and it's how I've been picking up high-end server equipment well below market price.

You point it at a website once (by writing down a handful of CSS selectors), then create as many search jobs as you want against it. Every hour, it opens a real browser, walks through pages, filters out ads that don't match your criteria, and sends the survivors to Telegram or email. New ads only - it remembers what it's already seen.

## What it can do

- **Scrape any listings site** without writing code per site. Each website is just a record in MongoDB describing its layout (cookie banner button, search box, card container, title, price, location, pagination, etc.). Add a new site by adding a new document.
- **Run multiple search jobs per site.** Each job has its own search term, must-contain keywords (AND), must-not-contain keywords (NONE), max price, and how many pages to walk.
- **Filter on real text matching.** Numeric keywords use digit boundaries so a job watching for "16GB" doesn't also fire on "1GB" or "160GB". Word keywords use word boundaries.
- **Deduplicate automatically.** Each ad's ID is derived from its URL, so a listing only ever notifies you once - even if it's still on the front page next time the scraper runs.
- **Notify on two channels at once.** Telegram (per-job chat ID, or a default) and SMTP (per-job recipient list with retry/backoff). Both fire in parallel for the same ad.
- **Save below-budget ads silently.** If an ad clears the keyword filters but blows past your max price, it still gets stored - it just doesn't notify. Useful for tracking the market without the spam.
- **Test runs with live feedback.** Before activating a job, you can fire a test run from the UI and watch each card stream in over SignalR with the verdict (kept / skipped / why) attached. Lets you validate selectors and filters in real time.
- **Run history.** Every cron run is logged with counts (total scraped, new found, notify-worthy) and the per-ad decision log, so you can audit what happened and when.

## How it works

![Architecture](docs/architecture.png)

The admin UI talks to the API over HTTP for CRUD on jobs/websites/listings/runs, and over SignalR (`/hubs/scrap-events`) for live test-run streaming.

Presenting one test UI-based feature for testing scrapers:
![Test-run](docs/test-run-demo.png)

## Tech stack

**Backend** (`WebScrapper.ScraperApi/`)
- .NET 10 minimal APIs
- Playwright .NET (Chromium, headless) for scraping
- Quartz.NET for the hourly cron
- MongoDB.Driver for persistence
- SignalR for live test-run events
- MailKit + Polly for SMTP with retry/backoff
- Multi-stage Dockerfile - Playwright browsers are installed in the build stage and copied into a lean ASP.NET runtime image so the final image doesn't carry the SDK.

**Frontend** (`frontend/`)
- React 18 + TypeScript + Vite
- TailwindCSS
- TanStack Query for server state
- React Router
- `@microsoft/signalr` client for live updates
- Built into a static bundle and served by nginx in the runtime image. The API base URL is baked in at build time via `VITE_API_URL`.

**Storage**
- MongoDB (any 6+ instance - I run a small Atlas cluster, but a self-hosted Mongo container works fine).

## Running it

You need: Docker, MongoDB (URL with credentials), optionally a Telegram bot token + chat ID, and optionally an SMTP account.

### 1. Clone

```bash
git clone https://github.com/Vali-Mandeal/WebScraper.git
cd WebScraper
```

### 2. Configure the API

The API reads its config via standard ASP.NET configuration. Easiest path is environment variables (double underscores map to nested keys):

```bash
DbSettings__MongoUrl="mongodb+srv://user:pass@cluster.mongodb.net"
DbSettings__DatabaseName="WebScrapperV2"

# Optional - leave empty to disable
TelegramSettings__BotToken="123456:ABC..."
TelegramSettings__DefaultChatId="123456789"

SmtpSettings__SenderEmail="bot@example.com"
SmtpSettings__SenderPassword="..."
SmtpSettings__SmtpHost="smtp.example.com"
SmtpSettings__SmtpPort="587"
SmtpSettings__SecureSocketOptions="StartTls"
```

Or fill in `WebScrapper.ScraperApi/appsettings.json` directly (don't commit it).

### 3. Build and run the API

```bash
cd WebScrapper.ScraperApi
docker build -t webscraper-api .
docker run -d --name webscraper-api \
  -p 8080:8080 \
  --env-file ../api.env \
  webscraper-api
```

Swagger lands at `http://localhost:8080/swagger`.

### 4. Build and run the UI

The API URL is baked into the JS bundle at build time:

```bash
cd frontend
docker build --build-arg VITE_API_URL=http://localhost:8080 -t webscraper-ui .
docker run -d --name webscraper-ui -p 3000:80 webscraper-ui
```

UI is at `http://localhost:3000`.

### 5. Add your first website + job

Open the UI:

1. **Websites** → add the target site. Fill in name, URL, and the selectors for cards/title/price/location/etc. The scraper supports cookie-banner clicks, optional search-box typing, optional infinite scroll, and pagination clicks.
2. **Jobs** → create a job pointed at that website. Set the search term, must-contain / must-not-contain keywords, max price, max pages, notification channels, and `IsActive=true`.
3. **Test** → run the job once interactively and watch the per-ad decisions stream in. Tweak selectors or filters until you're happy.
4. Wait for the next hourly tick, or trigger a run via the API.

## API surface (high-level)

- `GET/POST/PUT/PATCH/DELETE /scrapjobs` - manage search jobs
- `GET/POST/PUT/DELETE /websites` - manage website metadata + selectors
- `GET /listings` - paged ads (filter by job, by `shouldSendNotification`)
- `GET /scrapruns`, `GET /scrapruns/{id}` - run history with decision logs
- `POST /scrapruns/test` - run a job once and stream events over SignalR
- `GET /health`
- `/hubs/scrap-events` - SignalR hub for test-run streaming

Full schema is in Swagger.

## Project layout

```
WebScraper/
├── WebScrapper.ScraperApi/      # .NET backend
│   ├── Endpoints/               # minimal API route maps
│   ├── Services/                # scrap, ads, runs, notifications, broadcaster
│   ├── Repositories/            # Mongo + Telegram + SMTP
│   ├── BackgroundJobs/          # Quartz HourlyScrape
│   ├── Hubs/                    # SignalR ScrapEventsHub
│   ├── Entities/                # Mongo documents
│   └── Dockerfile
└── frontend/                    # React + Vite + Tailwind admin UI
    ├── src/pages/               # Jobs, Websites, Listings, Runs, Test
    ├── src/components/          # layout, ui, DecisionsList
    └── Dockerfile
```

## Notes and caveats

- **Sites change layouts.** The whole point of the per-website metadata model is that when a site reshuffles its DOM you fix it in the UI, not in code. But you do have to fix it.
- **Rate limiting.** The hourly schedule and `MaxDegreeOfParallelism = 3` in `HourlyScrape.cs` are tuned for polite scraping of a handful of sites. Crank these up at your own risk.
- **Be respectful.** Check the target site's terms of service. This tool is for personal use - scraping at scale, ignoring robots.txt, or reselling the data is on you.
- **Time zone.** The cron is hardcoded to `Europe/Bucharest` in `Program.cs`. Change it to your zone if you care about the 9-22 active window.

## License

MIT - see [LICENSE](LICENSE).
