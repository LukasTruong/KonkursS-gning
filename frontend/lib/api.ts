const BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export interface PersonSearchResult {
  personCvrId: string;
  fullName: string;
  totalBankruptcies: number;
}

export interface CompanySearchResult {
  cvrNumber: string;
  name: string;
  status: string;
}

export interface SearchResponse {
  persons: PersonSearchResult[];
  companies: CompanySearchResult[];
}

export interface RoleDto {
  cvrNumber: string;
  companyName: string;
  companyStatus: string;
  role: string;
  startDate: string | null;
  endDate: string | null;
}

export interface PersonProfile {
  personCvrId: string;
  fullName: string;
  totalBankruptcies: number;
  roles: RoleDto[];
}

export interface BankruptcyDto {
  cvrNumber: string;
  companyName: string;
  bankruptcyDate: string | null;
}

export interface CompanyPersonDto {
  personCvrId: string;
  fullName: string;
  role: string;
  startDate: string | null;
  endDate: string | null;
  isCurrent: boolean;
}

export interface CompanyProfile {
  cvrNumber: string;
  name: string;
  status: string;
  foundedDate: string | null;
  bankruptcyDate: string | null;
  industryCode: string | null;
  persons: CompanyPersonDto[];
}

async function apiFetch<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE}/api${path}`, { next: { revalidate: 3600 } });
  if (!res.ok) throw new Error(`API ${res.status}: ${path}`);
  return res.json();
}

export const searchAll = (q: string, type?: string) =>
  apiFetch<SearchResponse>(`/search?q=${encodeURIComponent(q)}${type ? `&type=${type}` : ""}`);

export const getPersonProfile = (id: string) =>
  apiFetch<PersonProfile>(`/person/${encodeURIComponent(id)}`);

export const getPersonBankruptcies = (id: string) =>
  apiFetch<BankruptcyDto[]>(`/person/${encodeURIComponent(id)}/bankruptcies`);

export const getCompanyProfile = (cvr: string) =>
  apiFetch<CompanyProfile>(`/company/${encodeURIComponent(cvr)}`);

export const getCompanyPersons = (cvr: string) =>
  apiFetch<CompanyPersonDto[]>(`/company/${encodeURIComponent(cvr)}/persons`);
