import Link from "next/link";
import type { PersonSearchResult } from "@/lib/api";

export default function PersonCard({ person }: { person: PersonSearchResult }) {
  return (
    <Link
      href={`/person/${encodeURIComponent(person.personCvrId)}`}
      className="block bg-white rounded-lg border border-gray-200 p-4 hover:border-blue-400 hover:shadow-sm transition-all"
    >
      <div className="flex items-center justify-between">
        <span className="font-medium text-gray-900">{person.fullName}</span>
        {person.totalBankruptcies > 0 && (
          <span className="bg-red-100 text-red-700 text-sm font-semibold px-3 py-1 rounded-full">
            {person.totalBankruptcies} konkurs{person.totalBankruptcies !== 1 ? "er" : ""}
          </span>
        )}
      </div>
      <p className="text-sm text-gray-500 mt-1">ID: {person.personCvrId}</p>
    </Link>
  );
}
