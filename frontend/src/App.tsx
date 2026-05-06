import { Layout } from "@/components/layout/Layout";
import { ListingsPage } from "@/pages/Listings";
import { JobsPage } from "@/pages/Jobs";
import { RunsPage } from "@/pages/Runs";
import { RunDetailPage } from "@/pages/Runs/RunDetail";
import { TestPage } from "@/pages/Test";
import { WebsitesPage } from "@/pages/Websites";
import { Navigate, Route, Routes } from "react-router-dom";

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Navigate to="/jobs" replace />} />
        <Route path="/jobs" element={<JobsPage />} />
        <Route path="/websites" element={<WebsitesPage />} />
        <Route path="/listings" element={<ListingsPage />} />
        <Route path="/runs" element={<RunsPage />} />
        <Route path="/runs/:id" element={<RunDetailPage />} />
        <Route path="/test" element={<TestPage />} />
      </Route>
    </Routes>
  );
}
