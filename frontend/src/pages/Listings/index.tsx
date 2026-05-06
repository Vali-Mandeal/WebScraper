import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Select } from "@/components/ui/Select";
import { PageSpinner } from "@/components/ui/Spinner";
import { listings as listingsApi, scrapJobs as jobsApi } from "@/lib/api";
import { formatDate } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { ExternalLink, Image, Megaphone } from "lucide-react";
import { useState } from "react";

export function ListingsPage() {
  const [scrapJobId, setScrapJobId] = useState("");
  const [page, setPage] = useState(1);

  const { data: jobsList = [] } = useQuery({
    queryKey: ["scrapjobs"],
    queryFn: () => jobsApi.list(),
  });

  const { data, isPending } = useQuery({
    queryKey: ["listings", scrapJobId, page],
    queryFn: () => listingsApi.list({ scrapJobId: scrapJobId || undefined, page }),
  });

  const jobOptions = [
    { value: "", label: "All Jobs" },
    ...jobsList.map((j) => ({ value: j.id, label: j.name })),
  ];

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <header className="flex items-center justify-between px-8 h-14 border-b border-zinc-800/80 flex-shrink-0">
        <div className="flex items-center gap-2.5">
          <Megaphone className="h-4 w-4 text-zinc-500" />
          <h1 className="text-sm font-semibold text-zinc-100">Listings</h1>
          {data && (
            <span className="text-xs text-zinc-600 font-mono">{data.total}</span>
          )}
        </div>
        <div className="w-48">
          <Select
            options={jobOptions}
            value={scrapJobId}
            onChange={(e) => {
              setScrapJobId(e.target.value);
              setPage(1);
            }}
          />
        </div>
      </header>

      {/* Content */}
      <div className="flex-1 overflow-auto p-8">
        {isPending ? (
          <PageSpinner />
        ) : !data?.data.length ? (
          <div className="flex flex-col items-center justify-center h-48 gap-3">
            <Megaphone className="h-8 w-8 text-zinc-700" />
            <p className="text-sm text-zinc-500">No listings found</p>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4">
              {data.data.map((ad) => {
                const href = ad.url
                  ? /^https?:\/\//i.test(ad.url)
                    ? ad.url
                    : `https://www.olx.ro${ad.url.startsWith("/") ? "" : "/"}${ad.url}`
                  : "#";
                return (
                  <a
                    key={ad.id}
                    href={href}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="group flex flex-col rounded-xl border border-zinc-800 bg-zinc-900/60 hover:border-zinc-700 hover:bg-zinc-900 transition-all overflow-hidden"
                  >
                    {/* Thumbnail */}
                    <div className="aspect-[4/3] bg-zinc-800 overflow-hidden relative">
                      {ad.thumbnailUrl ? (
                        <img
                          src={ad.thumbnailUrl}
                          alt={ad.title}
                          loading="lazy"
                          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                          onError={(e) => {
                            (e.target as HTMLImageElement).style.display = "none";
                          }}
                        />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center">
                          <Image className="h-8 w-8 text-zinc-700" />
                        </div>
                      )}
                      {/* Notify badge */}
                      {ad.shouldSendNotification && (
                        <div className="absolute top-2 right-2">
                          <Badge variant={ad.notificationSent ? "success" : "warning"}>
                            {ad.notificationSent ? "Sent" : "Pending"}
                          </Badge>
                        </div>
                      )}
                    </div>

                    {/* Details */}
                    <div className="p-3 flex flex-col gap-1.5">
                      <p className="text-xs font-medium text-zinc-100 line-clamp-2 leading-snug">
                        {ad.title}
                      </p>
                      {ad.price && (
                        <p className="text-sm font-semibold text-indigo-400">
                          {ad.price}
                        </p>
                      )}
                      {ad.locationAndDate && (
                        <p className="text-[11px] text-zinc-500 truncate">
                          {ad.locationAndDate}
                        </p>
                      )}
                      <div className="flex items-center justify-between mt-1">
                        <p className="text-[10px] text-zinc-600">
                          {formatDate(ad.seenAt)}
                        </p>
                        <ExternalLink className="h-3 w-3 text-zinc-600 group-hover:text-zinc-400 transition-colors" />
                      </div>
                    </div>
                  </a>
                );
              })}
            </div>

            {/* Pagination */}
            {data.totalPages > 1 && (
              <div className="flex items-center justify-center gap-3 mt-8">
                <Button
                  variant="secondary"
                  size="sm"
                  disabled={page === 1}
                  onClick={() => setPage((p) => p - 1)}
                >
                  Previous
                </Button>
                <span className="text-sm text-zinc-500 tabular-nums">
                  {page} / {data.totalPages}
                </span>
                <Button
                  variant="secondary"
                  size="sm"
                  disabled={page === data.totalPages}
                  onClick={() => setPage((p) => p + 1)}
                >
                  Next
                </Button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
