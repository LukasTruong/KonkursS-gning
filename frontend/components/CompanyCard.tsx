import Link from "next/link";
import type { CompanySearchResult } from "@/lib/api";

const statusLabels: Record<string, { label: string; classes: string }> = {
  Bankrupt: { label: "Konkurs", classes: "bg-red-100 text-red-700" },
  Active: { label: "Aktiv", classes: "bg-green-100 text-green-700" },
  Dissolved: { label: "Opløst", classes: "bg-gray-100 text-gray-600" },
  Unknown: { label: "Ukendt", classes: "bg-yellow-100 text-yellow-700" },
};

export default function CompanyCard({ company }: { company: CompanySearchResult }) {
  const badge = statusLabels[company.status] ?? statusLabels.Unknown;
  return (
    <Link
      href={`/company/${encodeURIComponent(company.cvrNumber)}`}
      className="block bg-white rounded-lg border border-gray-200 p-4 hover:border-blue-400 hover:shadow-sm transition-all"
    >
      <div className="flex items-center justify-between">
        <span className="font-medium text-gray-900">{company.name}</span>
        <span className={`text-sm font-semibold px-3 py-1 rounded-full ${badge.classes}`}>
          {badge.label}
        </span>
      </div>
      <p className="text-sm text-gray-500 mt-1">CVR: {company.cvrNumber}</p>
    </Link>
  );
}
