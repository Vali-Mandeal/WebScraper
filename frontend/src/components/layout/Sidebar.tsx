import { cn } from "@/lib/utils";
import {
  LayoutGrid,
  Globe,
  Megaphone,
  ActivitySquare,
  Search,
  FlaskConical,
} from "lucide-react";
import { NavLink } from "react-router-dom";

const navItems = [
  { to: "/jobs", icon: LayoutGrid, label: "Scrap Jobs" },
  { to: "/websites", icon: Globe, label: "Websites" },
  { to: "/listings", icon: Megaphone, label: "Listings" },
  { to: "/runs", icon: ActivitySquare, label: "Runs" },
  { to: "/test", icon: FlaskConical, label: "Test Scrape" },
];

export function Sidebar() {
  return (
    <aside className="w-56 flex-shrink-0 h-full flex flex-col border-r border-zinc-800/80 bg-zinc-950">
      {/* Logo */}
      <div className="px-4 h-14 flex items-center gap-2.5 border-b border-zinc-800/80">
        <div className="h-7 w-7 rounded-lg bg-indigo-600 flex items-center justify-center flex-shrink-0">
          <Search className="h-3.5 w-3.5 text-white" />
        </div>
        <div>
          <p className="text-sm font-semibold text-zinc-100 leading-tight">Scrapper</p>
          <p className="text-[11px] text-zinc-500 leading-tight">Admin</p>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 p-2 flex flex-col gap-0.5 overflow-y-auto">
        {navItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              cn(
                "flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm font-medium transition-all duration-100",
                isActive
                  ? "bg-indigo-500/10 text-indigo-400"
                  : "text-zinc-400 hover:text-zinc-200 hover:bg-zinc-800/60"
              )
            }
          >
            {({ isActive }) => (
              <>
                <Icon
                  className={cn(
                    "h-4 w-4 flex-shrink-0 transition-colors",
                    isActive ? "text-indigo-400" : "text-zinc-500"
                  )}
                />
                {label}
              </>
            )}
          </NavLink>
        ))}
      </nav>

      {/* Footer */}
      <div className="p-3 border-t border-zinc-800/80">
        <p className="text-[11px] text-zinc-600 text-center">WebScrapperV2</p>
      </div>
    </aside>
  );
}
