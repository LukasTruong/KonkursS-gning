"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";

export default function SearchBar() {
  const router = useRouter();
  const params = useSearchParams();
  const [query, setQuery] = useState(params.get("q") ?? "");
  const [type, setType] = useState<"person" | "company">(
    (params.get("type") as "person" | "company") ?? "person"
  );

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!query.trim()) return;
    router.push(`/search?q=${encodeURIComponent(query)}&type=${type}`);
  }

  return (
    <form onSubmit={handleSubmit} className="w-full max-w-2xl mx-auto">
      <div className="flex gap-4 mb-3">
        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="radio"
            name="type"
            value="person"
            checked={type === "person"}
            onChange={() => setType("person")}
            className="accent-blue-600"
          />
          Person
        </label>
        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="radio"
            name="type"
            value="company"
            checked={type === "company"}
            onChange={() => setType("company")}
            className="accent-blue-600"
          />
          Virksomhed
        </label>
      </div>
      <div className="flex gap-2">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder={type === "person" ? "Søg på navn..." : "Søg på virksomhedsnavn eller CVR..."}
          className="flex-1 rounded-lg border border-gray-300 px-4 py-3 text-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button
          type="submit"
          className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg text-lg font-medium transition-colors"
        >
          Søg
        </button>
      </div>
    </form>
  );
}
