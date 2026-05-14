import Link from "next/link";
import { getCompanyProfile } from "@/lib/api";
import { notFound } from "next/navigation";

interface Props {
  params: { cvr: string };
}

const roleLabels: Record<string, string> = {
  Director: "Direktør",
  BoardMember: "Bestyrelsesmedlem",
  Owner: "Ejer",
  Other: "Anden rolle",
};

export default async function CompanyPage({ params }: Props) {
  const profile = await getCompanyProfile(params.cvr).catch(() => null);
  if (!profile) notFound();

  const isBankrupt = profile.status === "Bankrupt";

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      <Link href="/" className="text-blue-600 hover:underline mb-6 inline-block">&larr; Søg igen</Link>

      <div className="flex items-start gap-4 mb-6">
        <div className="flex-1">
          <h1 className="text-3xl font-bold text-gray-900 mb-1">{profile.name}</h1>
          <p className="text-gray-500">CVR: {profile.cvrNumber}</p>
        </div>
        {isBankrupt && (
          <span className="bg-red-100 text-red-700 font-semibold px-4 py-2 rounded-full text-sm mt-1">
            KONKURS
          </span>
        )}
      </div>

      <dl className="grid grid-cols-2 gap-4 mb-8 bg-white border border-gray-200 rounded-lg p-4 text-sm">
        <div>
          <dt className="text-gray-500">Status</dt>
          <dd className={`font-medium ${isBankrupt ? "text-red-600" : "text-gray-900"}`}>{profile.status}</dd>
        </div>
        <div>
          <dt className="text-gray-500">Stiftet</dt>
          <dd>{profile.foundedDate ? new Date(profile.foundedDate).toLocaleDateString("da-DK") : "—"}</dd>
        </div>
        {isBankrupt && (
          <div>
            <dt className="text-gray-500">Konkursdato</dt>
            <dd className="text-red-600 font-medium">
              {profile.bankruptcyDate ? new Date(profile.bankruptcyDate).toLocaleDateString("da-DK") : "—"}
            </dd>
          </div>
        )}
        {profile.industryCode && (
          <div>
            <dt className="text-gray-500">Branchekode</dt>
            <dd>{profile.industryCode}</dd>
          </div>
        )}
      </dl>

      <section>
        <h2 className="text-xl font-semibold mb-4 text-gray-800">
          Direktører og ejere ({profile.persons.length})
        </h2>
        {profile.persons.length === 0 ? (
          <p className="text-gray-500 italic">Ingen registrerede personer.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm border-collapse">
              <thead>
                <tr className="bg-gray-100 text-left">
                  <th className="p-3 font-medium">Navn</th>
                  <th className="p-3 font-medium">Rolle</th>
                  <th className="p-3 font-medium">Fra</th>
                  <th className="p-3 font-medium">Til</th>
                </tr>
              </thead>
              <tbody>
                {profile.persons.map((p, i) => (
                  <tr key={i} className="border-t border-gray-200 hover:bg-gray-50">
                    <td className="p-3">
                      <Link href={`/person/${encodeURIComponent(p.personCvrId)}`} className="text-blue-600 hover:underline">
                        {p.fullName}
                      </Link>
                      {p.isCurrent && (
                        <span className="ml-2 bg-green-100 text-green-700 text-xs px-2 py-0.5 rounded-full">Aktiv</span>
                      )}
                    </td>
                    <td className="p-3">{roleLabels[p.role] ?? p.role}</td>
                    <td className="p-3 text-gray-500">{p.startDate ? new Date(p.startDate).toLocaleDateString("da-DK") : "—"}</td>
                    <td className="p-3 text-gray-500">{p.endDate ? new Date(p.endDate).toLocaleDateString("da-DK") : "Nuværende"}</td>
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
