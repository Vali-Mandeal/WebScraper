import { Button } from "@/components/ui/Button";
import { Dialog } from "@/components/ui/Dialog";
import { type ScrapJob, type Website } from "@/lib/api";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { JobFields, jobFieldsDefaults, type JobFieldsValues } from "./JobFields";

interface JobFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (values: Omit<ScrapJob, "id" | "createdOn" | "websiteName" | "notificationReceivers"> & { notificationReceivers: [] }) => Promise<void>;
  initial?: ScrapJob;
  websites: Website[];
  loading?: boolean;
}

export function JobForm({
  open,
  onClose,
  onSubmit,
  initial,
  websites,
  loading,
}: JobFormProps) {
  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<JobFieldsValues>({ defaultValues: jobFieldsDefaults });

  useEffect(() => {
    reset(
      initial
        ? {
            name: initial.name,
            websiteMetadataId: initial.websiteMetadataId,
            searchValue: initial.searchValue,
            isActive: initial.isActive,
            maxPrice: initial.maxPrice ?? 0,
            maxPages: initial.maxPages ?? 1,
            mustContainList: initial.mustContainList ?? [],
            mustNotContainList: initial.mustNotContainList ?? [],
            mustOrContainList: initial.mustOrContainList ?? [],
            telegramChatId: initial.telegramChatId ?? "",
          }
        : jobFieldsDefaults
    );
  }, [initial, open, reset]);

  const submit = (values: JobFieldsValues) =>
    onSubmit({ ...values, notificationReceivers: [] });

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={initial ? "Edit Scrap Job" : "New Scrap Job"}
      size="xl"
    >
      <form onSubmit={handleSubmit(submit)} className="flex flex-col gap-6">
        <JobFields register={register} control={control} errors={errors} websites={websites} />

        <div className="flex justify-end gap-2 pt-1">
          <Button type="button" variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" variant="primary" loading={loading}>
            {initial ? "Save Changes" : "Create Job"}
          </Button>
        </div>
      </form>
    </Dialog>
  );
}
