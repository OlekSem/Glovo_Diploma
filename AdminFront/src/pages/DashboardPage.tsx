import {useEffect, useMemo, useState} from "react";
import {useNavigate} from "react-router-dom";
import type {AdminUser, CompanyRequest, CompanyStatus} from "../interfaces/admin";
import {adminApi} from "../services/adminApi";
import RequestModal from "./RequestModal";

const filters: Array<CompanyStatus | "ALL"> = ["ALL", "Pending", "Approved", "Canceled"];
const labels: Record<CompanyStatus | "ALL", string> = {
    ALL: "All requests",
    Pending: "Pending",
    Approved: "Approved",
    Canceled: "Canceled"
};
const badge: Record<CompanyStatus, string> = {
    Pending: "bg-amber-100 text-amber-800",
    Approved: "bg-emerald-100 text-emerald-800",
    Canceled: "bg-rose-100 text-rose-800"
};

export default function DashboardPage({admin, onLogout}: { admin: AdminUser; onLogout: () => void }) {
    const navigate = useNavigate();
    const [filter, setFilter] = useState<CompanyStatus | "ALL">("Pending");
    const [requests, setRequests] = useState<CompanyRequest[]>([]);
    const [selected, setSelected] = useState<CompanyRequest | null>(null);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);
    const load = async (selectedFilter = filter) => {
        setLoading(true);
        setError("");
        try {
            setRequests(await adminApi.companies(selectedFilter));
        } catch (cause) {
            if ((cause as { status?: number }).status === 401 || (cause as {
                status?: number
            }).status === 403) navigate("/login", {replace: true}); else setError(cause instanceof Error ? cause.message : "Unable to load requests.");
        } finally {
            setLoading(false);
        }
    };
    useEffect(() => {
        void load();
    }, [filter]);
    const counts = useMemo(() => ({pending: requests.filter(x => x.status === "Pending").length}), [requests]);
    const signOut = async () => {
        await adminApi.logout().catch(() => undefined);
        onLogout();
        navigate("/login", {replace: true});
    };
    return <main className="min-h-screen bg-slate-100">
        <header className="border-b border-slate-200 bg-white">
            <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-6 py-4">
                <div className="flex items-center gap-3"><span
                    className="grid h-10 w-10 place-items-center rounded-xl bg-glovo-yellow font-black">g</span>
                    <div><p className="font-bold">Glovo Admin</p><p className="text-xs text-slate-500">Company
                        moderation</p></div>
                </div>
                <div className="flex items-center gap-3"><span
                    className="hidden text-sm text-slate-600 sm:inline">{admin.firstName || admin.username}</span>
                    <button onClick={signOut}
                            className="rounded-lg border border-slate-300 px-3 py-2 text-sm font-semibold">Sign out
                    </button>
                </div>
            </div>
        </header>
        <div className="mx-auto max-w-7xl p-6"><p
            className="text-xs font-bold uppercase tracking-[.22em] text-slate-500">Partner applications</p>
            <div className="mt-2 flex flex-wrap items-end justify-between gap-4">
                <div><h1 className="text-3xl font-extrabold">Company request queue</h1><p
                    className="mt-2 text-slate-600">Review submitted company details and decide when they become
                    active.</p></div>
                <button onClick={() => void load()}
                        className="rounded-xl border border-slate-300 bg-white px-4 py-2 font-semibold">↻ Refresh
                </button>
            </div>
            <div className="mt-7 grid gap-4 sm:grid-cols-3">
                <section className="rounded-2xl bg-slate-950 p-5 text-white"><p
                    className="text-sm text-slate-300">Pending in current view</p><p
                    className="mt-2 text-3xl font-bold">{counts.pending}</p></section>
                <section className="rounded-2xl bg-white p-5 shadow-sm"><p
                    className="text-sm text-slate-500">Workflow</p><p className="mt-2 font-semibold">Pending → approved
                    or canceled</p></section>
                <section className="rounded-2xl bg-white p-5 shadow-sm"><p
                    className="text-sm text-slate-500">Visibility</p><p className="mt-2 font-semibold">Only approved
                    companies are active</p></section>
            </div>
            <section className="mt-7 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
                <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 p-5">
                    <div><h2 className="font-bold">Requests</h2><p
                        className="text-sm text-slate-500">{requests.length} result{requests.length === 1 ? "" : "s"}</p>
                    </div>
                    <div className="flex flex-wrap gap-2">{filters.map(item => <button key={item}
                                                                                       onClick={() => setFilter(item)}
                                                                                       className={`rounded-full px-3 py-1.5 text-sm font-semibold ${filter === item ? "bg-slate-950 text-white" : "bg-slate-100 text-slate-700"}`}>{labels[item]}</button>)}</div>
                </div>
                {loading ? <p className="p-8 text-slate-600">Loading requests…</p> : error ?
                    <p className="p-8 text-rose-700">{error}</p> : requests.length === 0 ?
                        <p className="p-8 text-slate-600">No requests match this filter.</p> :
                        <div className="overflow-x-auto">
                            <table className="w-full text-left">
                                <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
                                <tr>
                                    <th className="px-6 py-4">Company</th>
                                    <th className="px-6 py-4">Partner</th>
                                    <th className="px-6 py-4">Submitted</th>
                                    <th className="px-6 py-4">Status</th>
                                    <th className="px-6 py-4"></th>
                                </tr>
                                </thead>
                                <tbody className="divide-y divide-slate-200">{requests.map(request => <tr
                                    key={request.id}>
                                    <td className="px-6 py-4"><p className="font-semibold">{request.name}</p><p
                                        className="max-w-xs truncate text-sm text-slate-500">{request.description}</p>
                                    </td>
                                    <td className="px-6 py-4 text-sm"><p>{request.ownerName}</p><p
                                        className="text-slate-500">{request.ownerEmail}</p></td>
                                    <td className="px-6 py-4 text-sm text-slate-600">{new Date(request.dateCreated).toLocaleDateString()}</td>
                                    <td className="px-6 py-4"><span
                                        className={`rounded-full px-3 py-1 text-sm font-semibold ${badge[request.status]}`}>{request.status}</span>
                                    </td>
                                    <td className="px-6 py-4 text-right">
                                        <button onClick={() => setSelected(request)}
                                                className="rounded-lg border border-slate-300 px-3 py-2 text-sm font-semibold">View
                                            details
                                        </button>
                                    </td>
                                </tr>)}</tbody>
                            </table>
                        </div>}</section>
        </div>
        {selected && <RequestModal request={selected} onClose={() => setSelected(null)} onModerated={(updated) => {
            setRequests(current => current.map(item => item.id === updated.id ? updated : item));
            setSelected(updated);
        }}/>}</main>;
}
