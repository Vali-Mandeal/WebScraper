import { Button } from "@/components/ui/Button";
import { Select } from "@/components/ui/Select";
import { DecisionsList } from "@/components/DecisionsList";
import { JobFields, jobFieldsDefaults, type JobFieldsValues } from "@/pages/Jobs/JobFields";
import {
  apiBase,
  scrapJobs as jobsApi,
  scrapRuns as runsApi,
  websites as websitesApi,
  type AdDecisionLog,
  type AdVerdict,
  type ScrapJob,
} from "@/lib/api";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { FlaskConical, Play, Plus, RotateCcw, Save, Square } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";

type RunState = "idle" | "running" | "finished" | "failed";

type AdDecidedPayload = {
  ad: {
    id: number;
    title?: string;
    price?: string;
    locationAndDate?: string;
    url?: string;
    thumbnailUrl?: string;
  };
  verdict: AdVerdict;
  reasonCode: string;
  reasonArgs: Record<string, string>;
};

export function TestPage() {
  const qc = useQueryClient();
  const [scrapJobId, setScrapJobId] = useState("");
  const [decisions, setDecisions] = useState<AdDecisionLog[]>([]);
  const [scraped, setScraped] = useState<number | null>(null);
  const [state, setState] = useState<RunState>("idle");
  const [error, setError] = useState<string | null>(null);
  const connectionRef = useRef<HubConnection | null>(null);

  const { register, handleSubmit, control, reset, getValues, formState: { errors } } =
    useForm<JobFieldsValues>({ defaultValues: jobFieldsDefaults });

  const { data: jobsList = [] } = useQuery({
    queryKey: ["scrapjobs"],
    queryFn: () => jobsApi.list(),
  });

  const { data: websitesList = [] } = useQuery({
    queryKey: ["websites"],
    queryFn: websitesApi.list,
  });

  // Populate form when a job is selected
  useEffect(() => {
    if (!scrapJobId) {
      reset(jobFieldsDefaults);
      return;
    }
    const job = jobsList.find((j) => j.id === scrapJobId);
    if (!job) return;
    reset({
      name: job.name,
      websiteMetadataId: job.websiteMetadataId,
      searchValue: job.searchValue,
      isActive: job.isActive,
      maxPrice: job.maxPrice ?? 0,
      maxPages: job.maxPages ?? 1,
      mustContainList: job.mustContainList ?? [],
      mustNotContainList: job.mustNotContainList ?? [],
      mustOrContainList: job.mustOrContainList ?? [],
      telegramChatId: job.telegramChatId ?? "",
    });
  }, [scrapJobId, jobsList, reset]);

  useEffect(() => {
    return () => {
      connectionRef.current?.stop();
    };
  }, []);

  const saveMut = useMutation({
    mutationFn: async (values: JobFieldsValues) => {
      if (!scrapJobId) throw new Error("Pick a job first");
      return jobsApi.patch(scrapJobId, values);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["scrapjobs"] });
    },
  });

  const createMut = useMutation({
    mutationFn: (values: JobFieldsValues) =>
      jobsApi.create({ ...values, notificationReceivers: [] }),
    onSuccess: (created) => {
      qc.invalidateQueries({ queryKey: ["scrapjobs"] });
      setScrapJobId(created.id);
    },
  });

  const start = async (values: JobFieldsValues) => {
    setDecisions([]);
    setScraped(null);
    setError(null);
    setState("running");

    const streamId = crypto.randomUUID();

    const connection = new HubConnectionBuilder()
      .withUrl(`${apiBase}/hubs/scrap-events`)
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("AdsScraped", (payload: { total: number }) => setScraped(payload.total));

    connection.on("AdDecided", (payload: AdDecidedPayload) => {
      setDecisions((prev) => [
        ...prev,
        {
          adId: payload.ad.id,
          title: payload.ad.title,
          price: payload.ad.price,
          url: payload.ad.url,
          thumbnailUrl: payload.ad.thumbnailUrl,
          verdict: payload.verdict,
          reasonCode: payload.reasonCode,
          reasonArgs: payload.reasonArgs,
          human: humanize(payload.reasonCode, payload.reasonArgs),
        },
      ]);
    });

    connection.on("RunFinished", () => setState("finished"));
    connection.on("RunFailed", (payload: { error: string }) => {
      setError(payload.error);
      setState("failed");
    });

    await connection.start();
    await connection.invoke("JoinRun", streamId);
    connectionRef.current = connection;

    const probeJob: Partial<ScrapJob> = {
      ...(scrapJobId ? { id: scrapJobId } : {}),
      name: values.name,
      websiteMetadataId: values.websiteMetadataId,
      searchValue: values.searchValue,
      isActive: values.isActive,
      maxPrice: values.maxPrice,
      maxPages: values.maxPages,
      mustContainList: values.mustContainList,
      mustNotContainList: values.mustNotContainList,
      mustOrContainList: values.mustOrContainList,
      telegramChatId: values.telegramChatId,
      notificationReceivers: [],
    };

    try {
      await runsApi.test(probeJob, streamId);
    } catch (e) {
      setError((e as Error).message);
      setState("failed");
    }
  };

  const stop = () => {
    connectionRef.current?.stop();
    connectionRef.current = null;
    setState("idle");
  };

  const counts = {
    notify: decisions.filter((d) => d.verdict === "NotifyWorthy").length,
    silent: decisions.filter((d) => d.verdict === "SavedSilent").length,
    skipped: decisions.filter((d) => d.verdict === "Skipped").length,
  };

  return (
    <div className="flex flex-col h-full">
      <header className="flex items-center justify-between px-8 h-14 border-b border-zinc-800/80 flex-shrink-0">
        <div className="flex items-center gap-2.5">
          <FlaskConical className="h-4 w-4 text-zinc-500" />
          <h1 className="text-sm font-semibold text-zinc-100">Test Scrape</h1>
          <span className="text-xs text-zinc-600">
            edit · run · iterate · save when happy
          </span>
        </div>
      </header>

      <div className="flex-1 overflow-auto p-8 space-y-6 max-w-4xl">
        {/* Job picker + actions */}
        <div className="flex items-end gap-3">
          <div className="w-64">
            <Select
              label="Load From Job"
              options={[
                { value: "", label: "Blank…" },
                ...jobsList.map((j) => ({ value: j.id, label: j.name })),
              ]}
              value={scrapJobId}
              onChange={(e) => setScrapJobId(e.target.value)}
            />
          </div>
          {scrapJobId && (
            <Button
              variant="ghost"
              onClick={() => {
                const job = jobsList.find((j) => j.id === scrapJobId);
                if (!job) return;
                reset({
                  name: job.name,
                  websiteMetadataId: job.websiteMetadataId,
                  searchValue: job.searchValue,
                  isActive: job.isActive,
                  maxPrice: job.maxPrice ?? 0,
                  mustContainList: job.mustContainList ?? [],
                  mustNotContainList: job.mustNotContainList ?? [],
                  mustOrContainList: job.mustOrContainList ?? [],
                  telegramChatId: job.telegramChatId ?? "",
                });
              }}
            >
              <RotateCcw className="h-3.5 w-3.5" />
              Reset
            </Button>
          )}
        </div>

        {/* The form fields — same as Edit Job */}
        <form
          onSubmit={handleSubmit(start)}
          className="rounded-xl border border-zinc-800 bg-zinc-900/40 p-5 space-y-6"
        >
          <JobFields
            register={register}
            control={control}
            errors={errors}
            websites={websitesList}
          />

          <div className="flex items-center justify-end gap-2 pt-2 border-t border-zinc-800">
            {scrapJobId ? (
              <Button
                type="button"
                variant="secondary"
                onClick={() => saveMut.mutate(getValues())}
                loading={saveMut.isPending}
              >
                <Save className="h-3.5 w-3.5" />
                Save Changes to Job
              </Button>
            ) : (
              <Button
                type="button"
                variant="secondary"
                onClick={() => createMut.mutate(getValues())}
                loading={createMut.isPending}
              >
                <Plus className="h-3.5 w-3.5" />
                Create Job
              </Button>
            )}
            {state === "running" ? (
              <Button type="button" variant="secondary" onClick={stop}>
                <Square className="h-3.5 w-3.5" />
                Stop
              </Button>
            ) : (
              <Button type="submit" variant="primary">
                <Play className="h-3.5 w-3.5" />
                Start Test
              </Button>
            )}
          </div>
        </form>

        {/* Counters */}
        {(state !== "idle" || decisions.length > 0) && (
          <div className="flex items-center gap-4 text-xs text-zinc-500 px-1">
            <span>
              State:{" "}
              <span
                className={
                  state === "running"
                    ? "text-amber-400"
                    : state === "finished"
                    ? "text-emerald-400"
                    : state === "failed"
                    ? "text-red-400"
                    : "text-zinc-300"
                }
              >
                {state}
              </span>
            </span>
            {scraped !== null && <span>Scraped: {scraped}</span>}
            <span>Notify: {counts.notify}</span>
            <span>Silent: {counts.silent}</span>
            <span>Skipped: {counts.skipped}</span>
          </div>
        )}

        {error && (
          <div className="text-xs text-red-400 font-mono bg-red-500/5 border border-red-500/10 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <DecisionsList decisions={decisions} />
      </div>
    </div>
  );
}

function humanize(code: string, args: Record<string, string>): string {
  switch (code) {
    case "Duplicate":
      return "Already saved in a previous run";
    case "MissingRequiredKeyword":
      return `Missing required keyword '${args.keyword ?? ""}'`;
    case "ExcludedKeyword":
      return `Contains excluded keyword '${args.keyword ?? ""}'`;
    case "PriceTooHigh":
      return `Price ${args.price} exceeds max ${args.maxPrice}`;
    case "NotifyWorthy":
      return "Matches all filters and price within budget";
    default:
      return code;
  }
}
