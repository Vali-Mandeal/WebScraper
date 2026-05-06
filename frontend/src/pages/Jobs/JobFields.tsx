import { Input } from "@/components/ui/Input";
import { Select } from "@/components/ui/Select";
import { Switch } from "@/components/ui/Switch";
import { TagInput } from "@/components/ui/TagInput";
import { type Website } from "@/lib/api";
import { Controller, type Control, type FieldErrors, type UseFormRegister } from "react-hook-form";

export type JobFieldsValues = {
  name: string;
  websiteMetadataId: string;
  searchValue: string;
  isActive: boolean;
  maxPrice: number;
  maxPages: number;
  mustContainList: string[];
  mustNotContainList: string[];
  mustOrContainList: string[];
  telegramChatId: string;
};

export const jobFieldsDefaults: JobFieldsValues = {
  name: "",
  websiteMetadataId: "",
  searchValue: "",
  isActive: true,
  maxPrice: 0,
  maxPages: 1,
  mustContainList: [],
  mustNotContainList: [],
  mustOrContainList: [],
  telegramChatId: "-5233621099",
};

interface JobFieldsProps {
  register: UseFormRegister<JobFieldsValues>;
  control: Control<JobFieldsValues>;
  errors: FieldErrors<JobFieldsValues>;
  websites: Website[];
  showActiveToggle?: boolean;
}

export function JobFields({ register, control, errors, websites, showActiveToggle = true }: JobFieldsProps) {
  const websiteOptions = websites.map((w) => ({ value: w.id, label: w.name }));

  return (
    <div className="flex flex-col gap-6">
      {/* ── Basic ── */}
      <section className="flex flex-col gap-4">
        <h3 className="text-xs font-semibold text-zinc-500 uppercase tracking-widest">Basic</h3>
        <div className="grid grid-cols-2 gap-4">
          <Input
            label="Name"
            placeholder="Unifi Camera"
            error={errors.name?.message}
            {...register("name", { required: "Required" })}
          />
          <Controller
            name="websiteMetadataId"
            control={control}
            rules={{ required: "Required" }}
            render={({ field }) => (
              <Select
                label="Website"
                options={websiteOptions}
                placeholder="Select website..."
                error={errors.websiteMetadataId?.message}
                {...field}
              />
            )}
          />
        </div>
        <Input
          label="Search Query"
          placeholder="unifi camera"
          error={errors.searchValue?.message}
          {...register("searchValue", { required: "Required" })}
        />
        {showActiveToggle && (
          <div className="flex items-center justify-between p-3 rounded-lg bg-zinc-800/50 border border-zinc-800">
            <div>
              <p className="text-sm font-medium text-zinc-200">Active</p>
              <p className="text-xs text-zinc-500">Job will run on each scheduled scrape</p>
            </div>
            <Controller
              name="isActive"
              control={control}
              render={({ field }) => <Switch checked={field.value} onChange={field.onChange} />}
            />
          </div>
        )}
      </section>

      <div className="border-t border-zinc-800" />

      {/* ── Limits ── */}
      <section className="flex flex-col gap-4">
        <h3 className="text-xs font-semibold text-zinc-500 uppercase tracking-widest">Limits</h3>
        <div className="grid grid-cols-2 gap-4">
          <Input
            label="Max Price (lei)"
            type="number"
            min={0}
            placeholder="0 = no limit"
            {...register("maxPrice", { valueAsNumber: true, min: 0 })}
          />
          <Input
            label="Max Pages"
            type="number"
            min={1}
            placeholder="1"
            hint="How many result pages to walk per run"
            {...register("maxPages", { valueAsNumber: true, min: 1 })}
          />
        </div>
      </section>

      <div className="border-t border-zinc-800" />

      {/* ── Filters ── */}
      <section className="flex flex-col gap-4">
        <h3 className="text-xs font-semibold text-zinc-500 uppercase tracking-widest">Filters</h3>
        <Controller
          name="mustContainList"
          control={control}
          render={({ field }) => (
            <TagInput
              label="Must Contain (ALL)"
              hint="All terms must appear in the title"
              color="green"
              value={field.value}
              onChange={field.onChange}
              placeholder="Add term..."
            />
          )}
        />
        <Controller
          name="mustOrContainList"
          control={control}
          render={({ field }) => (
            <TagInput
              label="Must Contain (ANY)"
              hint="At least one term must appear in the title"
              color="amber"
              value={field.value}
              onChange={field.onChange}
              placeholder="Add term..."
            />
          )}
        />
        <Controller
          name="mustNotContainList"
          control={control}
          render={({ field }) => (
            <TagInput
              label="Must NOT Contain"
              hint="None of these terms may appear in the title"
              color="red"
              value={field.value}
              onChange={field.onChange}
              placeholder="Add term..."
            />
          )}
        />
      </section>

      <div className="border-t border-zinc-800" />

      {/* ── Meta ── */}
      <section className="flex flex-col gap-4">
        <h3 className="text-xs font-semibold text-zinc-500 uppercase tracking-widest">Meta</h3>
        <Input label="Telegram Chat ID" placeholder="-5233621099" {...register("telegramChatId")} />
      </section>
    </div>
  );
}
