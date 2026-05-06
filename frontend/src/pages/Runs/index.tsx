import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { PageSpinner } from "@/components/ui/Spinner";
import { scrapRuns as runsApi } from "@/lib/api";
import { formatDate, formatDuration } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { ActivitySquare } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";

export function RunsPage() {
  const [page, setPage] = useState(1);

  const { data, isPending } = useQuery({
    queryKey: ["scrapruns", page],
    queryFn: () => runsApi.list(page),
  });

  return (
    <div className="flex flex-col h-full">
      <header className="flex items-center gap-2.5 px-8 h-14 border-b border-zinc-800/80 flex-shrink-0">
        <ActivitySquare className="h-4 w-4 text-zinc-500" />
        <h1 className="text-sm font-semibold text-zinc-100">Runs</h1>
        {data && (
          <span className="text-xs text-zinc-600 font-mono">{data.total}</span>
        )}
      </header>

      <div className="flex-1 overflow-auto">
        {isPending ? (
          <PageSpinner />
        ) : !data?.data.length ? (
          <div className="flex flex-col items-center justify-center h-64 gap-3">
            <ActivitySquare className="h-8 w-8 text-zinc-700" />
            <p className="text-sm text-zinc-500">No runs yet</p>
          </div>
        ) : (
          <>
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="border-b border-zinc-800/80">
                  {["Run ID", "Job", "Started", "Duration", "Status", "Scraped", "New", "Notify"].map(
                    (h) => (
                      <th
                        key={h}
                        className="px-4 py-3 text-left text-xs font-medium text-zinc-500 uppercase tracking-wider whitespace-nowrap first:pl-8 last:pr-8"
                      >
                        {h}
                      </th>
                    )
                  )}
                </tr>
              </thead>
              <tbody>
                {data.data.map((run) => (
                  <tr
                    key={run.id}
                    className="group border-b border-zinc-800/40 hover:bg-zinc-900/60 transition-colors cursor-pointer"
                  >
                    <td className="pl-8 pr-4 py-3.5 font-mono text-xs text-zinc-500">
                      <Link to={`/runs/${run.id}`} className="hover:text-zinc-300">
                        {run.runId?.slice(0, 18)}…
                      </Link>
                    </td>
                    <td className="px-4 py-3.5 text-zinc-300 whitespace-nowrap">
                      <Link to={`/runs/${run.id}`} className="hover:text-zinc-100">
                        {run.scrapJobName}
                      </Link>
                    </td>
                    <td className="px-4 py-3.5 text-zinc-400 whitespace-nowrap text-xs">
                      {formatDate(run.startedAt)}
                    </td>
                    <td className="px-4 py-3.5 text-zinc-500 whitespace-nowrap text-xs tabular-nums">
                      {formatDuration(run.startedAt, run.finishedAt)}
                    </td>
                    <td className="px-4 py-3.5">
                      <Badge variant={run.status === "success" ? "success" : "danger"}>
                        {run.status}
                      </Badge>
                    </td>
                    <td className="px-4 py-3.5 text-zinc-400 text-center tabular-nums">
                      {run.totalScraped}
                    </td>
                    <td className="px-4 py-3.5 text-center tabular-nums">
                      <span
                        className={
                          run.newAdsFound > 0
                            ? "text-emerald-400 font-semibold"
                            : "text-zinc-600"
                        }
                      >
                        +{run.newAdsFound}
                      </span>
                    </td>
                    <td className="pl-4 pr-8 py-3.5 text-center tabular-nums">
                      <span
                        className={
                          run.notifyWorthyCount > 0
                            ? "text-indigo-400 font-semibold"
                            : "text-zinc-600"
                        }
                      >
                        {run.notifyWorthyCount}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {data.totalPages > 1 && (
              <div className="flex items-center justify-center gap-3 py-6">
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
