import { Button } from "@/components/ui/Button";
import { PageSpinner } from "@/components/ui/Spinner";
import { Switch } from "@/components/ui/Switch";
import { scrapJobs as jobsApi, websites as websitesApi, type ScrapJob } from "@/lib/api";
import { formatDate } from "@/lib/utils";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { LayoutGrid, Pencil, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { JobForm } from "./JobForm";

export function JobsPage() {
  const qc = useQueryClient();
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<ScrapJob | undefined>();
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const { data: jobsList, isPending } = useQuery({
    queryKey: ["scrapjobs"],
    queryFn: () => jobsApi.list(),
  });

  const { data: websitesList = [] } = useQuery({
    queryKey: ["websites"],
    queryFn: websitesApi.list,
  });

  const saveMut = useMutation({
    mutationFn: async (values: ScrapJob | (Omit<ScrapJob, "id" | "createdOn" | "websiteName"> & { id?: string })) => {
      if ("id" in values && values.id) {
        return jobsApi.update(values.id, values as ScrapJob);
      }
      return jobsApi.create(values as Omit<ScrapJob, "id" | "createdOn" | "websiteName">);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["scrapjobs"] });
      setFormOpen(false);
      setEditing(undefined);
    },
  });

  const toggleMut = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      jobsApi.toggle(id, isActive),
    onMutate: async ({ id, isActive }) => {
      await qc.cancelQueries({ queryKey: ["scrapjobs"] });
      const prev = qc.getQueryData<ScrapJob[]>(["scrapjobs"]);
      qc.setQueryData<ScrapJob[]>(["scrapjobs"], (old) =>
        old?.map((j) => (j.id === id ? { ...j, isActive } : j))
      );
      return { prev };
    },
    onError: (_err, _vars, ctx) => {
      qc.setQueryData(["scrapjobs"], ctx?.prev);
    },
    onSettled: () => qc.invalidateQueries({ queryKey: ["scrapjobs"] }),
  });

  const deleteMut = useMutation({
    mutationFn: jobsApi.delete,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["scrapjobs"] }),
  });

  const openCreate = () => {
    setEditing(undefined);
    setFormOpen(true);
  };

  const openEdit = (job: ScrapJob) => {
    setEditing(job);
    setFormOpen(true);
  };

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <header className="flex items-center justify-between px-8 h-14 border-b border-zinc-800/80 flex-shrink-0">
        <div className="flex items-center gap-2.5">
          <LayoutGrid className="h-4 w-4 text-zinc-500" />
          <h1 className="text-sm font-semibold text-zinc-100">Scrap Jobs</h1>
          {jobsList && (
            <span className="text-xs text-zinc-600 font-mono">
              {jobsList.length}
            </span>
          )}
        </div>
        <Button variant="primary" size="sm" onClick={openCreate}>
          <Plus className="h-3.5 w-3.5" />
          New Job
        </Button>
      </header>

      {/* Content */}
      <div className="flex-1 overflow-auto">
        {isPending ? (
          <PageSpinner />
        ) : !jobsList?.length ? (
          <EmptyState onNew={openCreate} />
        ) : (
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-zinc-800/80">
                {["Name", "Website", "Search", "Max Price", "Active", "Created", ""].map(
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
              {jobsList.map((job) => (
                <tr
                  key={job.id}
                  className="group border-b border-zinc-800/40 hover:bg-zinc-900/60 transition-colors"
                >
                  <td className="pl-8 pr-4 py-3.5 font-medium text-zinc-100 whitespace-nowrap">
                    {job.name}
                  </td>
                  <td className="px-4 py-3.5 text-zinc-400 whitespace-nowrap">
                    {job.websiteName ?? "—"}
                  </td>
                  <td className="px-4 py-3.5">
                    <span className="font-mono text-xs text-zinc-400 bg-zinc-800/60 px-2 py-0.5 rounded">
                      {job.searchValue}
                    </span>
                  </td>
                  <td className="px-4 py-3.5 text-zinc-400 whitespace-nowrap tabular-nums">
                    {job.maxPrice ? `${job.maxPrice} lei` : <span className="text-zinc-600">—</span>}
                  </td>
                  <td className="px-4 py-3.5">
                    <Switch
                      size="sm"
                      checked={job.isActive}
                      onChange={() => toggleMut.mutate({ id: job.id, isActive: !job.isActive })}
                    />
                  </td>
                  <td className="px-4 py-3.5 text-zinc-500 whitespace-nowrap text-xs">
                    {formatDate(job.createdOn)}
                  </td>
                  <td className="pl-4 pr-8 py-3.5">
                    <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => openEdit(job)}
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        loading={deletingId === job.id && deleteMut.isPending}
                        onClick={() => {
                          if (confirm(`Delete "${job.name}"?`)) {
                            setDeletingId(job.id);
                            deleteMut.mutate(job.id, {
                              onSettled: () => setDeletingId(null),
                            });
                          }
                        }}
                        className="text-red-500/60 hover:text-red-400 hover:bg-red-500/10"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <JobForm
        open={formOpen}
        onClose={() => {
          setFormOpen(false);
          setEditing(undefined);
        }}
        onSubmit={async (values) => {
          const payload = editing
            ? ({ ...editing, ...values } as ScrapJob)
            : (values as Omit<ScrapJob, "id" | "createdOn" | "websiteName">);
          await saveMut.mutateAsync(payload);
        }}
        initial={editing}
        websites={websitesList}
        loading={saveMut.isPending}
      />
    </div>
  );
}

function EmptyState({ onNew }: { onNew: () => void }) {
  return (
    <div className="flex flex-col items-center justify-center h-64 gap-4 text-center">
      <div className="h-12 w-12 rounded-xl bg-zinc-800/80 flex items-center justify-center">
        <LayoutGrid className="h-5 w-5 text-zinc-500" />
      </div>
      <div>
        <p className="text-sm font-medium text-zinc-300">No scrap jobs yet</p>
        <p className="text-xs text-zinc-500 mt-1">
          Create a job to start monitoring listings
        </p>
      </div>
      <Button variant="primary" size="sm" onClick={onNew}>
        <Plus className="h-3.5 w-3.5" />
        New Job
      </Button>
    </div>
  );
}
