import { cn } from "@/lib/utils";
import { X } from "lucide-react";
import { useRef, useState, type KeyboardEvent } from "react";

interface TagInputProps {
  label?: string;
  hint?: string;
  value: string[];
  onChange: (tags: string[]) => void;
  placeholder?: string;
  color?: "default" | "green" | "red" | "amber";
}

const tagColors = {
  default: "bg-zinc-700/80 text-zinc-200 border-zinc-600",
  green: "bg-emerald-500/10 text-emerald-300 border-emerald-500/20",
  red: "bg-red-500/10 text-red-300 border-red-500/20",
  amber: "bg-amber-500/10 text-amber-300 border-amber-500/20",
};

export function TagInput({
  label,
  hint,
  value,
  onChange,
  placeholder = "Type and press Enter",
  color = "default",
}: TagInputProps) {
  const [input, setInput] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const add = (raw: string) => {
    const tag = raw.trim().toLowerCase();
    if (tag && !value.includes(tag)) {
      onChange([...value, tag]);
    }
    setInput("");
  };

  const remove = (tag: string) => onChange(value.filter((t) => t !== tag));

  const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" || e.key === ",") {
      e.preventDefault();
      add(input);
    } else if (e.key === "Backspace" && !input && value.length > 0) {
      remove(value[value.length - 1]);
    }
  };

  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label className="text-xs font-medium text-zinc-400 uppercase tracking-wider">
          {label}
        </label>
      )}
      <div
        className="min-h-[2.25rem] w-full rounded-lg border border-zinc-700 bg-zinc-800/80 px-2 py-1.5 flex flex-wrap gap-1.5 cursor-text focus-within:border-indigo-500 focus-within:ring-2 focus-within:ring-indigo-500/20 transition-colors"
        onClick={() => inputRef.current?.focus()}
      >
        {value.map((tag) => (
          <span
            key={tag}
            className={cn(
              "inline-flex items-center gap-1 pl-2 pr-1 py-0.5 rounded-md text-xs font-medium border",
              tagColors[color]
            )}
          >
            {tag}
            <button
              type="button"
              onClick={(e) => {
                e.stopPropagation();
                remove(tag);
              }}
              className="hover:text-white transition-colors rounded-sm"
            >
              <X className="h-2.5 w-2.5" />
            </button>
          </span>
        ))}
        <input
          ref={inputRef}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={onKeyDown}
          onBlur={() => input && add(input)}
          placeholder={value.length === 0 ? placeholder : ""}
          className="flex-1 min-w-[120px] bg-transparent text-sm text-zinc-100 placeholder:text-zinc-600 outline-none"
        />
      </div>
      {hint && <p className="text-xs text-zinc-500">{hint}</p>}
    </div>
  );
}
