import { Button } from "@/components/ui/Button";
import { Dialog } from "@/components/ui/Dialog";
import { Input } from "@/components/ui/Input";
import { Switch } from "@/components/ui/Switch";
import { type Website } from "@/lib/api";
import { useEffect, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";

type FormValues = {
  name: string;
  url: string;
  shouldAcceptTermsAndConditions: boolean;
  shouldScrollToBottom: boolean;
  shouldSearch: boolean;
  selectors: {
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
};

const defaults: FormValues = {
  name: "",
  url: "",
  shouldAcceptTermsAndConditions: true,
  shouldScrollToBottom: true,
  shouldSearch: true,
  selectors: {
    termsAndConditionsButtonSelector: "",
    searchSelector: "",
    scrollToButtonCommand: "window.scrollBy(0, window.innerHeight)",
    cardsSelector: "",
    cardTitleSelector: "",
    cardPriceSelector: "",
    locationAndDateSelector: "",
    adUrlWrapperSelector: "",
    adUrlSelector: "href",
    thumbnailUrlWrapperSelector: "",
    thumbnailUrlSelector: "src",
    backupThumbnailUrlWrapperSelector: "",
    backupThumbnailUrlSelector: "data-src",
    nextPageButtonSelector: "",
  },
};

interface WebsiteFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (values: FormValues) => Promise<void>;
  initial?: Website;
  loading?: boolean;
}

function Section({
  title,
  collapsible,
  children,
}: {
  title: string;
  collapsible?: boolean;
  children: React.ReactNode;
}) {
  const [open, setOpen] = useState(!collapsible);
  return (
    <section className="flex flex-col gap-3">
      <button
        type="button"
        onClick={() => collapsible && setOpen((o) => !o)}
        className={cn(
          "flex items-center justify-between",
          collapsible && "cursor-pointer"
        )}
      >
        <h3 className="text-xs font-semibold text-zinc-500 uppercase tracking-widest">
          {title}
        </h3>
        {collapsible && (
          <ChevronDown
            className={cn(
              "h-3.5 w-3.5 text-zinc-500 transition-transform",
              open && "rotate-180"
            )}
          />
        )}
      </button>
      {open && <div className="flex flex-col gap-3">{children}</div>}
    </section>
  );
}

export function WebsiteForm({
  open,
  onClose,
  onSubmit,
  initial,
  loading,
}: WebsiteFormProps) {
  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<FormValues>({ defaultValues: defaults });

  useEffect(() => {
    reset(
      initial
        ? {
            name: initial.name,
            url: initial.url,
            shouldAcceptTermsAndConditions: initial.shouldAcceptTermsAndConditions,
            shouldScrollToBottom: initial.shouldScrollToBottom,
            shouldSearch: initial.shouldSearch,
            selectors: { ...defaults.selectors, ...initial.selectors },
          }
        : defaults
    );
  }, [initial, open, reset]);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={initial ? "Edit Website" : "New Website"}
      description="Configure the CSS selectors and scrape options for this website"
      size="xl"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-6">
        {/* ── Basic ── */}
        <Section title="Basic">
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Name"
              placeholder="Olx"
              error={errors.name?.message}
              {...register("name", { required: "Required" })}
            />
            <Input
              label="Base URL"
              placeholder="https://www.olx.ro"
              {...register("url")}
            />
          </div>
        </Section>

        <div className="border-t border-zinc-800" />

        {/* ── Behaviour ── */}
        <Section title="Behaviour">
          <div className="flex items-center justify-between p-3 rounded-lg bg-zinc-800/50 border border-zinc-800">
            <p className="text-sm font-medium text-zinc-200">Accept cookies / ToC</p>
            <Controller
              name="shouldAcceptTermsAndConditions"
              control={control}
              render={({ field }) => (
                <Switch checked={field.value} onChange={field.onChange} />
              )}
            />
          </div>
          <div className="flex items-center justify-between p-3 rounded-lg bg-zinc-800/50 border border-zinc-800">
            <p className="text-sm font-medium text-zinc-200">Type into search box</p>
            <Controller
              name="shouldSearch"
              control={control}
              render={({ field }) => (
                <Switch checked={field.value} onChange={field.onChange} />
              )}
            />
          </div>
          <div className="flex items-center justify-between p-3 rounded-lg bg-zinc-800/50 border border-zinc-800">
            <p className="text-sm font-medium text-zinc-200">Scroll to bottom</p>
            <Controller
              name="shouldScrollToBottom"
              control={control}
              render={({ field }) => (
                <Switch checked={field.value} onChange={field.onChange} />
              )}
            />
          </div>
          <Input
            label="Scroll Command"
            placeholder="window.scrollBy(0, window.innerHeight)"
            hint="JS executed in the page on each scroll step"
            {...register("selectors.scrollToButtonCommand")}
          />
        </Section>

        <div className="border-t border-zinc-800" />

        {/* ── Selectors ── */}
        <Section title="Selectors" collapsible>
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Cards Selector"
              placeholder="div[data-testid='l-card']"
              error={errors.selectors?.cardsSelector?.message}
              {...register("selectors.cardsSelector", { required: "Required" })}
            />
            <Input
              label="Card Title Selector"
              placeholder="h4"
              {...register("selectors.cardTitleSelector")}
            />
            <Input
              label="Card Price Selector"
              placeholder="p[data-testid='ad-price']"
              {...register("selectors.cardPriceSelector")}
            />
            <Input
              label="Location / Date Selector"
              placeholder="p[data-testid='location-date']"
              {...register("selectors.locationAndDateSelector")}
            />
            <Input
              label="Ad URL Wrapper Selector"
              placeholder="div[data-testid='ad-card-title'] > a"
              {...register("selectors.adUrlWrapperSelector")}
            />
            <Input
              label="Ad URL Attribute"
              placeholder="href"
              {...register("selectors.adUrlSelector")}
            />
            <Input
              label="Thumbnail Wrapper Selector"
              placeholder="img[src]"
              {...register("selectors.thumbnailUrlWrapperSelector")}
            />
            <Input
              label="Thumbnail Attribute"
              placeholder="src"
              {...register("selectors.thumbnailUrlSelector")}
            />
            <Input
              label="Backup Thumbnail Wrapper Selector"
              placeholder="img[data-src]"
              {...register("selectors.backupThumbnailUrlWrapperSelector")}
            />
            <Input
              label="Backup Thumbnail Attribute"
              placeholder="data-src"
              {...register("selectors.backupThumbnailUrlSelector")}
            />
            <Input
              label="ToC / Cookie Button Selector"
              placeholder="button#onetrust-accept-btn-handler"
              {...register("selectors.termsAndConditionsButtonSelector")}
            />
            <Input
              label="Search Input Selector"
              placeholder="#search"
              {...register("selectors.searchSelector")}
            />
            <Input
              label="Next Page Button Selector"
              placeholder="[data-testid='pagination-forward']"
              hint="Leave blank if pagination isn't used"
              {...register("selectors.nextPageButtonSelector")}
            />
          </div>
        </Section>

        <div className="flex justify-end gap-2 pt-1">
          <Button type="button" variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" variant="primary" loading={loading}>
            {initial ? "Save Changes" : "Create Website"}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
