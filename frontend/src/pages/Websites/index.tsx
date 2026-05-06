import { Button } from "@/components/ui/Button";
import { PageSpinner } from "@/components/ui/Spinner";
import { websites as websitesApi, type Website } from "@/lib/api";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Globe, Pencil, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { WebsiteForm } from "./WebsiteForm";

export function WebsitesPage() {
  const qc = useQueryClient();
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<Website | undefined>();

  const { data: list, isPending } = useQuery({
    queryKey: ["websites"],
    queryFn: websitesApi.list,
  });

  const saveMut = useMutation({
    mutationFn: async (values: Website | (Omit<Website, "id"> & { id?: string })) => {
      if ("id" in values && values.id) {
        return websitesApi.update(values.id, values as Website);
      }
      return websitesApi.create(values as Omit<Website, "id">);
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["websites"] });
      setFormOpen(false);
      setEditing(undefined);
    },
  });

  const deleteMut = useMutation({
    mutationFn: websitesApi.delete,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["websites"] }),
  });

  return (
    <div className="flex flex-col h-full">
      <header className="flex items-center justify-between px-8 h-14 border-b border-zinc-800/80 flex-shrink-0">
        <div className="flex items-center gap-2.5">
          <Globe className="h-4 w-4 text-zinc-500" />
          <h1 className="text-sm font-semibold text-zinc-100">Websites</h1>
          {list && (
            <span className="text-xs text-zinc-600 font-mono">{list.length}</span>
          )}
        </div>
        <Button
          variant="primary"
          size="sm"
          onClick={() => {
            setEditing(undefined);
            setFormOpen(true);
          }}
        >
          <Plus className="h-3.5 w-3.5" />
          New Website
        </Button>
      </header>

      <div className="flex-1 overflow-auto">
        {isPending ? (
          <PageSpinner />
        ) : !list?.length ? (
          <div className="flex flex-col items-center justify-center h-64 gap-4">
            <div className="h-12 w-12 rounded-xl bg-zinc-800/80 flex items-center justify-center">
              <Globe className="h-5 w-5 text-zinc-500" />
            </div>
            <p className="text-sm text-zinc-400">No websites configured yet</p>
            <Button
              variant="primary"
              size="sm"
              onClick={() => setFormOpen(true)}
            >
              <Plus className="h-3.5 w-3.5" />
              New Website
            </Button>
          </div>
        ) : (
          <div className="p-8 grid grid-cols-1 gap-3 max-w-3xl">
            {list.map((site) => (
              <div
                key={site.id}
                className="group flex items-center gap-4 p-4 rounded-xl border border-zinc-800 bg-zinc-900/60 hover:border-zinc-700 transition-colors"
              >
                {/* Icon */}
                <div className="h-10 w-10 rounded-lg bg-zinc-800 flex items-center justify-center flex-shrink-0">
                  <Globe className="h-4.5 w-4.5 text-zinc-400" />
                </div>

                {/* Info */}
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-zinc-100">{site.name}</p>
                  <p className="text-xs text-zinc-500 truncate mt-0.5">{site.url}</p>
                  <div className="flex items-center gap-3 mt-1.5 text-xs text-zinc-600">
                    {site.shouldSearch && <span>search</span>}
                    {site.shouldScrollToBottom && <span>· scroll</span>}
                    {site.shouldAcceptTermsAndConditions && <span>· cookie</span>}
                  </div>
                </div>

                {/* Actions */}
                <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => {
                      setEditing(site);
                      setFormOpen(true);
                    }}
                  >
                    <Pencil className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => {
                      if (confirm(`Delete "${site.name}"?`)) {
                        deleteMut.mutate(site.id);
                      }
                    }}
                    className="text-red-500/60 hover:text-red-400 hover:bg-red-500/10"
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <WebsiteForm
        open={formOpen}
        onClose={() => {
          setFormOpen(false);
          setEditing(undefined);
        }}
        onSubmit={async (values) => {
          const payload = editing
            ? ({ ...editing, ...values } as Website)
            : (values as Omit<Website, "id">);
          await saveMut.mutateAsync(payload);
        }}
        initial={editing}
        loading={saveMut.isPending}
      />
    </div>
  );
}
