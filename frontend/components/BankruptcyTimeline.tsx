import type { BankruptcyDto } from "@/lib/api";
import Link from "next/link";

export default function BankruptcyTimeline({ bankruptcies }: { bankruptcies: BankruptcyDto[] }) {
  if (bankruptcies.length === 0) {
    return <p className="text-gray-500 italic">Ingen registrerede konkurser.</p>;
  }

  return (
    <ol className="relative border-l-2 border-red-200 ml-4">
      {bankruptcies.map((b) => (
        <li key={b.cvrNumber} className="mb-6 ml-6">
          <span className="absolute -left-[9px] flex items-center justify-center w-4 h-4 bg-red-500 rounded-full ring-4 ring-white" />
          <time className="block text-sm text-gray-500 mb-1">
            {b.bankruptcyDate
              ? new Date(b.bankruptcyDate).toLocaleDateString("da-DK", { year: "numeric", month: "long" })
              : "Dato ukendt"}
          </time>
          <Link
            href={`/company/${encodeURIComponent(b.cvrNumber)}`}
            className="font-semibold text-gray-900 hover:text-blue-600"
          >
            {b.companyName}
          </Link>
          <p className="text-sm text-gray-500">CVR: {b.cvrNumber}</p>
        </li>
      ))}
    </ol>
  );
}
