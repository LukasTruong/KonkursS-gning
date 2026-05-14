import { Suspense } from "react";
import Link from "next/link";
import SearchBar from "@/components/SearchBar";
import PersonCard from "@/components/PersonCard";
import CompanyCard from "@/components/CompanyCard";
import { searchAll } from "@/lib/api";

interface Props {
  searchParams: { q?: string; type?: string };
}

export default async function SearchPage({ searchParams }: Props) {
  const q = searchParams.q ?? "";
  const type = searchParams.type;

  if (!q) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-12">
        <Link href="/" className="text-blue-600 hover:underline mb-6 inline-block">&larr; Tilbage</Link>
        <Suspense><SearchBar /></Suspense>
      </div>
    );
  }

  const results = await searchAll(q, type).catch(() => ({ persons: [], companies: [] }));

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <Link href="/" className="text-blue-600 hover:underline mb-6 inline-block">&larr; Ny søgning</Link>
      <Suspense>
        <div className="mb-8">
          <SearchBar />
        </div>
      </Suspense>

      {results.persons.length === 0 && results.companies.length === 0 && (
        <p className="text-gray-500 text-center py-12">Ingen resultater for &quot;{q}&quot;.</p>
      )}

      {results.persons.length > 0 && (
        <section className="mb-10">
          <h2 className="text-xl font-semibold mb-4 text-gray-800">
            Personer ({results.persons.length})
          </h2>
          <div className="flex flex-col gap-3">
            {results.persons.map((p) => <PersonCard key={p.personCvrId} person={p} />)}
          </div>
        </section>
      )}

      {results.companies.length > 0 && (
        <section>
          <h2 className="text-xl font-semibold mb-4 text-gray-800">
            Virksomheder ({results.companies.length})
          </h2>
          <div className="flex flex-col gap-3">
            {results.companies.map((c) => <CompanyCard key={c.cvrNumber} company={c} />)}
          </div>
        </section>
      )}
    </div>
  );
}
