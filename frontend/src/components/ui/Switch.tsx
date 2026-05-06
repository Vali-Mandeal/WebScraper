import { cn } from "@/lib/utils";

interface SwitchProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
  label?: string;
  size?: "sm" | "md";
}

export function Switch({ checked, onChange, disabled, label, size = "md" }: SwitchProps) {
  return (
    <label
      className={cn(
        "inline-flex items-center gap-2 cursor-pointer select-none",
        disabled && "opacity-40 cursor-not-allowed"
      )}
    >
      <button
        type="button"
        role="switch"
        aria-checked={checked}
        disabled={disabled}
        onClick={() => !disabled && onChange(!checked)}
        className={cn(
          "relative rounded-full transition-all duration-200 focus-visible:ring-2 focus-visible:ring-indigo-500 focus-visible:ring-offset-2 focus-visible:ring-offset-zinc-900 focus:outline-none",
          size === "sm" ? "w-8 h-4" : "w-10 h-5",
          checked ? "bg-indigo-500" : "bg-zinc-700"
        )}
      >
        <span
          className={cn(
            "absolute top-0.5 rounded-full bg-white shadow-sm transition-transform duration-200",
            size === "sm"
              ? "h-3 w-3 left-0.5"
              : "h-4 w-4 left-0.5",
            checked
              ? size === "sm"
                ? "translate-x-4"
                : "translate-x-5"
              : "translate-x-0"
          )}
        />
      </button>
      {label && (
        <span className="text-sm text-zinc-300">{label}</span>
      )}
    </label>
  );
}
