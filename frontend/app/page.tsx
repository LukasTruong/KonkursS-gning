import { Suspense } from "react";
import SearchBar from "@/components/SearchBar";

export default function HomePage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[80vh] px-4">
      <div className="text-center mb-10">
        <h1 className="text-4xl font-bold text-gray-900 mb-3">KonkursCheck</h1>
        <p className="text-lg text-gray-600 max-w-lg">
          Slå op om ejere eller direktører bag en virksomhed tidligere har haft selskaber,
          der er gået konkurs — baseret på det offentlige CVR-register.
        </p>
      </div>
      <Suspense>
        <SearchBar />
      </Suspense>
    </div>
  );
}
