import Link from "next/link";

export default function PrivacyPage() {
  return (
    <div className="max-w-2xl mx-auto px-4 py-12">
      <Link href="/" className="text-blue-600 hover:underline mb-8 inline-block">&larr; Tilbage til forsiden</Link>

      <h1 className="text-3xl font-bold text-gray-900 mb-6">Privatlivspolitik</h1>

      <section className="mb-8">
        <h2 className="text-xl font-semibold mb-3 text-gray-800">Hvad er KonkursCheck?</h2>
        <p className="text-gray-700 leading-relaxed">
          KonkursCheck er et due diligence-værktøj, der giver private borgere og virksomheder mulighed for at
          slå op, om ejere eller direktører bag en virksomhed tidligere har haft selskaber, der er gået konkurs.
          Tjenesten er baseret udelukkende på offentlige data fra det danske CVR-register (Erhvervsstyrelsen).
        </p>
      </section>

      <section className="mb-8">
        <h2 className="text-xl font-semibold mb-3 text-gray-800">Hvilke data behandles?</h2>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li>Navne på fysiske og juridiske personer registreret i CVR</li>
          <li>CVR-interne person-ID&apos;er (ikke CPR-numre)</li>
          <li>Virksomhedsoplysninger: navn, CVR-nummer, status, stiftelsesdato, branchekode</li>
          <li>Roller: direktør, bestyrelsesmedlem, ejer — med datoer</li>
        </ul>
      </section>

      <section className="mb-8">
        <h2 className="text-xl font-semibold mb-3 text-gray-800">Hvad behandles IKKE?</h2>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li>CPR-numre gemmes eller vises aldrig</li>
          <li>Privatadresser på fysiske personer vises aldrig</li>
          <li>Vi indsamler ikke brugerdata, cookies eller sporingsinformation</li>
        </ul>
      </section>

      <section className="mb-8">
        <h2 className="text-xl font-semibold mb-3 text-gray-800">Retsgrundlag</h2>
        <p className="text-gray-700 leading-relaxed">
          Behandlingen af personoplysninger sker på grundlag af legitim interesse (GDPR artikel 6(1)(f)),
          idet data stammer fra det offentlige CVR-register og udelukkende vedrører erhvervsmæssige roller.
          Formålet er forbrugerbeskyttelse og gennemsigtighed.
        </p>
      </section>

      <section className="mb-8">
        <h2 className="text-xl font-semibold mb-3 text-gray-800">Dine rettigheder og indsigtsformular</h2>
        <p className="text-gray-700 leading-relaxed mb-3">
          Du har ret til indsigt i, berigtigelse af og sletning af dine oplysninger, i det omfang
          dette er foreneligt med de offentliggjorte CVR-data.
        </p>
        <p className="text-gray-700">
          Kontakt dataansvarlig via e-mail:{" "}
          <a href="mailto:KimAivieTommyAalborg@gmail.com" className="text-blue-600 underline">
            KimAivieTommyAalborg@gmail.com
          </a>
        </p>
      </section>

      <section>
        <h2 className="text-xl font-semibold mb-3 text-gray-800">Datakilde</h2>
        <p className="text-gray-700">
          Alle oplysninger stammer fra Erhvervsstyrelsens CVR-register og er offentligt tilgængelige
          via CVR&apos;s ElasticSearch API.
        </p>
      </section>
    </div>
  );
}
