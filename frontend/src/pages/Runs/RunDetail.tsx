import { Badge } from "@/components/ui/Badge";
import { PageSpinner } from "@/components/ui/Spinner";
import { DecisionsList } from "@/components/DecisionsList";
import { scrapRuns as runsApi } from "@/lib/api";
import { formatDate, formatDuration } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { ActivitySquare, ArrowLeft } from "lucide-react";
import { Link, useParams } from "react-router-dom";

export function RunDetailPage() {
  const { id = "" } = useParams<{ id: string }>();

  const { data: run, isPending, error } = useQuery({
    queryKey: ["scraprun", id],
    queryFn: () => runsApi.get(id),
    enabled: !!id,
  });

  if (isPending) return <PageSpinner />;
  if (error || !run) {
    return (
      <div className="flex items-center justify-center h-64 text-sm text-red-400">
        {(error as Error)?.message ?? "Run not found"}
      </div>
    );
  }

  const counts = {
    notify: run.decisions.filter((d) => d.verdict === "NotifyWorthy").length,
    silent: run.decisions.filter((d) => d.verdict === "SavedSilent").length,
    skipped: run.decisions.filter((d) => d.verdict === "Skipped").length,
  };

  return (
    <div className="flex flex-col h-full">
      <header className="flex items-center gap-3 px-8 h-14 border-b border-zinc-800/80 flex-shrink-0">
        <Link to="/runs" className="text-zinc-500 hover:text-zinc-300">
          <ArrowLeft className="h-4 w-4" />
        </Link>
        <ActivitySquare className="h-4 w-4 text-zinc-500" />
        <h1 className="text-sm font-semibold text-zinc-100">{run.scrapJobName}</h1>
        <span className="text-xs text-zinc-600 font-mono">{run.runId}</span>
        <Badge variant={run.status === "success" ? "success" : "danger"}>{run.status}</Badge>
      </header>

      <div className="flex-1 overflow-auto p-8 space-y-4">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <Stat label="Started" value={formatDate(run.startedAt)} />
          <Stat label="Duration" value={formatDuration(run.startedAt, run.finishedAt)} />
          <Stat label="Scraped" value={String(run.totalScraped)} />
          <Stat label="New" value={`+${run.newAdsFound}`} accent="emerald" />
        </div>

        <div className="flex items-center gap-4 text-xs text-zinc-500">
          <span>Notify: {counts.notify}</span>
          <span>Silent: {counts.silent}</span>
          <span>Skipped: {counts.skipped}</span>
        </div>

        {run.error && (
          <div className="text-xs text-red-400 font-mono bg-red-500/5 border border-red-500/10 rounded-lg p-2.5">
            {run.error}
          </div>
        )}

        <DecisionsList decisions={run.decisions} />
      </div>
    </div>
  );
}

function Stat({
  label,
  value,
  accent,
}: {
  label: string;
  value: string;
  accent?: "emerald" | "indigo";
}) {
  return (
    <div className="rounded-lg border border-zinc-800 bg-zinc-900/60 p-3">
      <p className="text-[11px] uppercase tracking-wider text-zinc-500">{label}</p>
      <p
        className={`text-lg font-semibold tabular-nums mt-0.5 ${
          accent === "emerald"
            ? "text-emerald-400"
            : accent === "indigo"
            ? "text-indigo-400"
            : "text-zinc-100"
        }`}
      >
        {value}
      </p>
    </div>
  );
}
