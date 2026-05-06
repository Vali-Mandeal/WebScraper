import { Badge } from "@/components/ui/Badge";
import { type AdDecisionLog, type AdVerdict } from "@/lib/api";
import { ExternalLink, Image } from "lucide-react";

const verdictMeta: Record<AdVerdict, { label: string; variant: "success" | "warning" | "default"; tone: string }> = {
  NotifyWorthy: { label: "Notify", variant: "success", tone: "border-emerald-500/30 bg-emerald-500/5" },
  SavedSilent: { label: "Silent", variant: "warning", tone: "border-amber-500/20 bg-amber-500/5" },
  Skipped: { label: "Skipped", variant: "default", tone: "border-zinc-800 bg-zinc-900/40" },
};

export function DecisionsList({ decisions }: { decisions: AdDecisionLog[] }) {
  if (decisions.length === 0) {
    return (
      <div className="flex items-center justify-center h-32 text-sm text-zinc-500">
        No decisions yet
      </div>
    );
  }
  return (
    <div className="flex flex-col gap-2">
      {decisions.map((d, i) => (
        <DecisionRow key={`${d.adId}-${i}`} decision={d} />
      ))}
    </div>
  );
}

function DecisionRow({ decision }: { decision: AdDecisionLog }) {
  const meta = verdictMeta[decision.verdict];
  const href = decision.url
    ? /^https?:\/\//i.test(decision.url)
      ? decision.url
      : `https://www.olx.ro${decision.url.startsWith("/") ? "" : "/"}${decision.url}`
    : undefined;

  return (
    <div className={`flex items-center gap-3 p-2.5 rounded-lg border ${meta.tone}`}>
      <div className="h-12 w-16 rounded-md bg-zinc-800 overflow-hidden flex-shrink-0 flex items-center justify-center">
        {decision.thumbnailUrl ? (
          <img
            src={decision.thumbnailUrl}
            alt={decision.title}
            loading="lazy"
            className="w-full h-full object-cover"
            onError={(e) => ((e.target as HTMLImageElement).style.display = "none")}
          />
        ) : (
          <Image className="h-4 w-4 text-zinc-600" />
        )}
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <Badge variant={meta.variant}>{meta.label}</Badge>
          {decision.price && (
            <span className="text-xs text-indigo-300 font-semibold">{decision.price}</span>
          )}
        </div>
        <p className="text-sm text-zinc-100 line-clamp-1 mt-0.5">{decision.title ?? "(no title)"}</p>
        <p className="text-[11px] text-zinc-500 line-clamp-1 mt-0.5">{decision.human}</p>
      </div>
      {href && (
        <a
          href={href}
          target="_blank"
          rel="noopener noreferrer"
          className="text-zinc-500 hover:text-zinc-300"
          aria-label="Open ad"
        >
          <ExternalLink className="h-4 w-4" />
        </a>
      )}
    </div>
  );
}
