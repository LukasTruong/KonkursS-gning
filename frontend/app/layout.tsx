import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "KonkursCheck — CVR Konkursoversigt",
  description: "Slå op om ejere og direktører bag en virksomhed tidligere har haft selskaber, der er gået konkurs.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="da">
      <body className="min-h-screen flex flex-col bg-gray-50 text-gray-900">
        <main className="flex-1">{children}</main>
        <footer className="bg-white border-t border-gray-200 py-4 px-6 text-center text-sm text-gray-500">
          Oplysningerne stammer fra det offentlige CVR-register (Erhvervsstyrelsen) og er udelukkende faktuelle.{" "}
          <a href="/privacy" className="underline hover:text-gray-700">Privatlivspolitik</a>
        </footer>
      </body>
    </html>
  );
}
