// All requests go to the .NET ScraperApi.
// VITE_API_URL is the base URL of the API (e.g. http://localhost:8080).
// Falls back to localhost:8080 for dev.

const API_BASE = import.meta.env.VITE_API_URL ?? "http://localhost:8080";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: { "Content-Type": "application/json", ...init?.headers },
    ...init,
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(body || `HTTP ${res.status}`);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

// ── Pagination wrapper ──────────────────────────────────────────────────────

export type PagedResult<T> = {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

// ── ScrapJobs ───────────────────────────────────────────────────────────────

export type ScrapJob = {
  id: string;
  websiteMetadataId: string;
  websiteName?: string;
  name: string;
  searchValue: string;
  mustContainList: string[];
  mustNotContainList: string[];
  mustOrContainList: string[];
  maxPrice: number;
  maxPages: number;
  telegramChatId?: string;
  notificationReceivers: NotificationReceiver[];
  isActive: boolean;
  createdOn: string;
};

export type NotificationReceiver = { name: string; email: string };

export const scrapJobs = {
  list: (active?: boolean) => {
    const q = new URLSearchParams();
    if (active !== undefined) q.set("active", String(active));
    return request<ScrapJob[]>(`/scrapjobs?${q}`);
  },
  get: (id: string) => request<ScrapJob>(`/scrapjobs/${id}`),
  create: (data: Omit<ScrapJob, "id" | "createdOn" | "websiteName">) =>
    request<ScrapJob>("/scrapjobs", { method: "POST", body: JSON.stringify(data) }),
  update: (id: string, data: ScrapJob) =>
    request<void>(`/scrapjobs/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  patch: (id: string, updates: Record<string, unknown>) =>
    request<void>(`/scrapjobs/${id}`, { method: "PATCH", body: JSON.stringify(updates) }),
  toggle: (id: string, isActive: boolean) =>
    request<void>(`/scrapjobs/${id}`, {
      method: "PATCH",
      body: JSON.stringify({ IsActive: isActive }),
    }),
  delete: (id: string) =>
    request<void>(`/scrapjobs/${id}`, { method: "DELETE" }),
};

// ── Websites ─────────────────────────────────────────────────────────────────

export type MetadataSelectors = {
  termsAndConditionsButtonSelector: string;
  searchSelector: string;
  scrollToButtonCommand: string;
  cardsSelector: string;
  cardTitleSelector: string;
  cardPriceSelector: string;
  locationAndDateSelector: string;
  adUrlWrapperSelector: string;
  adUrlSelector: string;
  thumbnailUrlWrapperSelector: string;
  thumbnailUrlSelector: string;
  backupThumbnailUrlWrapperSelector: string;
  backupThumbnailUrlSelector: string;
  nextPageButtonSelector: string;
};

export type Website = {
  id: string;
  name: string;
  url: string;
  shouldAcceptTermsAndConditions: boolean;
  shouldScrollToBottom: boolean;
  shouldSearch: boolean;
  selectors: MetadataSelectors;
};

export const websites = {
  list: () => request<Website[]>("/websites"),
  get: (id: string) => request<Website>(`/websites/${id}`),
  create: (data: Omit<Website, "id">) =>
    request<Website>("/websites", { method: "POST", body: JSON.stringify(data) }),
  update: (id: string, data: Website) =>
    request<void>(`/websites/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  delete: (id: string) =>
    request<void>(`/websites/${id}`, { method: "DELETE" }),
};

// ── Ads ──────────────────────────────────────────────────────────────────────

export type Ad = {
  id: number;
  scrapJobId: string;
  runId?: string;
  title?: string;
  price?: string;
  locationAndDate?: string;
  url?: string;
  thumbnailUrl?: string;
  shouldSendNotification: boolean;
  notificationSent: boolean;
  seenAt: string;
};

export const listings = {
  list: (params: {
    scrapJobId?: string;
    shouldSendNotification?: boolean;
    page?: number;
    pageSize?: number;
  }) => {
    const q = new URLSearchParams();
    if (params.scrapJobId) q.set("scrapJobId", params.scrapJobId);
    if (params.shouldSendNotification !== undefined)
      q.set("shouldSendNotification", String(params.shouldSendNotification));
    if (params.page) q.set("page", String(params.page));
    if (params.pageSize) q.set("pageSize", String(params.pageSize));
    return request<PagedResult<Ad>>(`/listings?${q}`);
  },
  get: (id: number) => request<Ad>(`/listings/${id}`),
};

// ── ScrapRuns + decisions ────────────────────────────────────────────────────

export type AdVerdict = "NotifyWorthy" | "SavedSilent" | "Skipped";

export type AdDecisionLog = {
  adId: number;
  title?: string;
  price?: string;
  url?: string;
  thumbnailUrl?: string;
  verdict: AdVerdict;
  reasonCode: string;
  reasonArgs: Record<string, string>;
  human: string;
};

export type ScrapRun = {
  id: string;
  runId: string;
  scrapJobId: string;
  scrapJobName: string;
  startedAt: string;
  finishedAt: string;
  totalScraped: number;
  newAdsFound: number;
  notifyWorthyCount: number;
  status: "success" | "failure";
  error?: string;
  decisions: AdDecisionLog[];
};

export const scrapRuns = {
  list: (page = 1, pageSize = 25) =>
    request<PagedResult<ScrapRun>>(`/scrapruns?page=${page}&pageSize=${pageSize}`),
  get: (id: string) => request<ScrapRun>(`/scrapruns/${id}`),
  run: (scrapJobId: string) =>
    request<ScrapRun>("/scrapruns", {
      method: "POST",
      body: JSON.stringify({ ScrapJobId: scrapJobId }),
    }),
  test: (scrapJob: Partial<ScrapJob>, streamId: string) =>
    request<{ streamId: string }>("/scrapruns/test", {
      method: "POST",
      body: JSON.stringify({ ScrapJob: scrapJob, StreamId: streamId }),
    }),
};

export const apiBase = API_BASE;
