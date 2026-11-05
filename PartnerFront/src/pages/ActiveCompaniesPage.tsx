import { Link } from "react-router-dom";
import { useGetMyCompaniesQuery } from "../services/companyApi";

export default function ActiveCompaniesPage() {
    const { data: companies = [], isLoading, isError } = useGetMyCompaniesQuery();
    return <main className="mx-auto max-w-4xl p-6"><Link className="text-sm text-emerald-700 hover:underline" to="/dashboard">← Back to dashboard</Link>
        <p className="mt-8 text-xs font-bold uppercase tracking-[0.2em] text-slate-500">Active companies</p><h1 className="mt-2 text-3xl font-bold">My active companies</h1>
        <p className="mt-2 text-slate-600">Only approved companies appear here.</p>
        {isLoading ? <p className="mt-8">Loading companies…</p> : isError ? <p className="mt-8 text-rose-700">Unable to load active companies.</p> : companies.length === 0 ? <div className="mt-8 rounded-xl border border-dashed border-slate-300 p-6 text-slate-600">No active companies yet. <Link className="text-emerald-700 underline" to="/companies/new">Submit one for review.</Link></div> :
            <div className="mt-8 grid gap-4 sm:grid-cols-2">{companies.map(company => <Link key={company.id} to={`/companies/${company.id}`} className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition hover:border-emerald-400 hover:shadow"><h2 className="font-semibold">{company.name}</h2><p className="mt-2 text-sm text-slate-600">{company.description}</p><span className="mt-4 inline-block text-sm font-medium text-emerald-700">Manage branches →</span></Link>)}</div>}
    </main>;
}
