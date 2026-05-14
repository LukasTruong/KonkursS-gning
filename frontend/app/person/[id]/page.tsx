import Link from "next/link";
import BankruptcyTimeline from "@/components/BankruptcyTimeline";
import { getPersonProfile, getPersonBankruptcies } from "@/lib/api";
import { notFound } from "next/navigation";

interface Props {
  params: { id: string };
}

const roleLabels: Record<string, string> = {
  Director: "Direktør",
  BoardMember: "Bestyrelsesmedlem",
  Owner: "Ejer",
  Other: "Anden rolle",
};

const statusClasses: Record<string, string> = {
  Bankrupt: "text-red-600 font-semibold",
  Active: "text-green-600",
  Dissolved: "text-gray-500",
};

export default async function PersonPage({ params }: Props) {
  const [profile, bankruptcies] = await Promise.all([
    getPersonProfile(params.id).catch(() => null),
    getPersonBankruptcies(params.id).catch(() => []),
  ]);

  if (!profile) notFound();

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <Link href="/" className="text-blue-600 hover:underline mb-6 inline-block">&larr; Søg igen</Link>

      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6 text-sm text-blue-800">
        Disse oplysninger stammer fra det offentlige CVR-register og er udelukkende faktuelle.
      </div>

      <h1 className="text-3xl font-bold text-gray-900 mb-1">{profile.fullName}</h1>
      <p className="text-gray-500 mb-6">Person-ID: {profile.personCvrId}</p>

      {profile.totalBankruptcies > 0 && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
          <p className="text-red-700 font-semibold">
            {profile.totalBankruptcies} konkursramte selskab{profile.totalBankruptcies !== 1 ? "er" : ""} registreret
          </p>
        </div>
      )}

      <section className="mb-10">
        <h2 className="text-xl font-semibold mb-4 text-gray-800">Konkurstidslinje</h2>
        <BankruptcyTimeline bankruptcies={bankruptcies} />
      </section>

      <section>
        <h2 className="text-xl font-semibold mb-4 text-gray-800">Alle roller ({profile.roles.length})</h2>
        {profile.roles.length === 0 ? (
          <p className="text-gray-500 italic">Ingen registrerede roller.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-gray-100 text-left">
                  <th className="p-3 font-medium">Virksomhed</th>
                  <th className="p-3 font-medium">Rolle</th>
                  <th className="p-3 font-medium">Status</th>
                  <th className="p-3 font-medium">Fra</th>
                  <th className="p-3 font-medium">Til</th>
                </tr>
              </thead>
              <tbody>
                {profile.roles.map((r, i) => (
                  <tr key={i} className="border-t border-gray-200 hover:bg-gray-50">
                    <td className="p-3">
                      <Link href={`/company/${encodeURIComponent(r.cvrNumber)}`} className="text-blue-600 hover:underline">
                        {r.companyName}
                      </Link>
                    </td>
                    <td className="p-3">{roleLabels[r.role] ?? r.role}</td>
                    <td className={`p-3 ${statusClasses[r.companyStatus] ?? ""}`}>{r.companyStatus}</td>
                    <td className="p-3 text-gray-500">{r.startDate ? new Date(r.startDate).toLocaleDateString("da-DK") : "—"}</td>
                    <td className="p-3 text-gray-500">{r.endDate ? new Date(r.endDate).toLocaleDateString("da-DK") : "Nuværende"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
