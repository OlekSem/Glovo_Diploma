import { Link } from "react-router-dom";
import { useGetMyCompanyRequestsQuery } from "../services/companyApi";
import type { Company } from "../interfaces/company";

const statusStyle: Record<Company["status"], string> = {
    Pending: "bg-amber-100 text-amber-800",
    Approved: "bg-emerald-100 text-emerald-800",
    Canceled: "bg-rose-100 text-rose-800",
};
const statusText: Record<Company["status"], string> = {
    Pending: "Under review",
    Approved: "Approved",
    Canceled: "Canceled",
};

export default function CompanyRequestsPage() {
    const { data: companies = [], isLoading, isError, refetch } = useGetMyCompanyRequestsQuery();
    return <main className="mx-auto max-w-5xl p-6">
        <div className="flex flex-wrap items-start justify-between gap-4">
            <div><Link className="text-sm text-emerald-700 hover:underline" to="/dashboard">← Back to dashboard</Link>
                <p className="mt-8 text-xs font-bold uppercase tracking-[0.2em] text-slate-500">Partner applications</p>
                <h1 className="mt-2 text-3xl font-bold text-slate-900">My companies</h1>
                <p className="mt-2 text-slate-600">Track the current review status of every company submission.</p></div>
            <button onClick={() => refetch()} className="rounded-full border border-slate-300 px-4 py-2 text-sm font-medium">Refresh</button>
        </div>
        <section className="mt-8 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
            <div className="border-b border-slate-200 px-6 py-5"><h2 className="font-semibold">Company applications</h2><p className="text-sm text-slate-500">{companies.length} submission{companies.length === 1 ? "" : "s"}</p></div>
            {isLoading ? <p className="p-6 text-slate-600">Loading applications…</p> : isError ? <p className="p-6 text-rose-700">Unable to load your applications.</p> : companies.length === 0 ? <p className="p-6 text-slate-600">You have not submitted a company yet.</p> :
                <div className="divide-y divide-slate-200">{companies.map((company) => <article key={company.id} className="grid gap-3 px-6 py-5 md:grid-cols-[1fr_auto] md:items-center">
                    <div><h3 className="font-semibold text-slate-950">{company.name}</h3><p className="mt-1 text-sm text-slate-600">Submitted {new Date(company.dateCreated).toLocaleDateString()}</p>
                        {company.moderationComment && <p className="mt-2 rounded-lg bg-slate-50 p-3 text-sm text-slate-700"><span className="font-medium">Moderator note: </span>{company.moderationComment}</p>}</div>
                    <span className={`w-fit rounded-full px-3 py-1 text-sm font-semibold ${statusStyle[company.status]}`}>{statusText[company.status]}</span>
                </article>)}</div>}
        </section>
    </main>;
}
