import {useState} from "react";
import type {CompanyRequest} from "../interfaces/admin";
import {adminApi} from "../services/adminApi";

export default function RequestModal({request, onClose, onModerated}: {
    request: CompanyRequest;
    onClose: () => void;
    onModerated: (request: CompanyRequest) => void
}) {
    const [comment, setComment] = useState(request.moderationComment ?? "");
    const [error, setError] = useState("");
    const [saving, setSaving] = useState(false);
    const moderate = async (action: "approve" | "cancel") => {
        setSaving(true);
        setError("");
        try {
            onModerated(await adminApi.moderate(request.id, action, comment));
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Unable to save the decision.");
        } finally {
            setSaving(false);
        }
    };
    const pending = request.status === "Pending";
    return <div role="dialog" aria-modal="true"
                className="fixed inset-0 z-10 grid place-items-center bg-slate-950/45 p-4">
        <section className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-3xl bg-white p-6 shadow-2xl">
            <div className="flex items-start justify-between gap-4">
                <div><p className="text-xs font-bold uppercase tracking-[.2em] text-slate-500">Company application</p>
                    <h2 className="mt-1 text-2xl font-extrabold">{request.name}</h2></div>
                <button onClick={onClose} aria-label="Close" className="rounded-full bg-slate-100 px-3 py-1 text-xl">×
                </button>
            </div>
            <div className="mt-6 grid gap-4 sm:grid-cols-2"><Detail label="Company description"
                                                                    value={request.description} wide/><Detail
                label="Partner" value={request.ownerName}/><Detail label="Email" value={request.ownerEmail}/><Detail
                label="Phone" value={request.ownerPhone}/><Detail label="Submitted"
                                                                  value={new Date(request.dateCreated).toLocaleString()}/>
            </div>
            <label className="mt-6 block text-sm font-semibold">Decision comment <span
                className="font-normal text-slate-500">(shown to the partner)</span><textarea disabled={!pending}
                                                                                              value={comment}
                                                                                              onChange={event => setComment(event.target.value)}
                                                                                              maxLength={1000} rows={4}
                                                                                              placeholder="Leave context for the partner…"
                                                                                              className="mt-2 w-full rounded-xl border border-slate-300 p-3 outline-none focus:border-slate-950 disabled:bg-slate-100"/></label>
            {error && <p className="mt-4 rounded-xl bg-rose-50 p-3 text-sm text-rose-800">{error}</p>}{pending ?
            <div className="mt-6 flex flex-wrap justify-end gap-3">
                <button disabled={saving} onClick={() => void moderate("cancel")}
                        className="rounded-xl bg-rose-100 px-4 py-2.5 font-bold text-rose-800 disabled:opacity-60">Cancel
                    application
                </button>
                <button disabled={saving} onClick={() => void moderate("approve")}
                        className="rounded-xl bg-glovo-yellow px-4 py-2.5 font-bold disabled:opacity-60">{saving ? "Saving…" : "Approve company"}</button>
            </div> : <p className="mt-6 rounded-xl bg-slate-100 p-3 text-sm text-slate-600">This application has already
                been {request.status.toLowerCase()}.</p>}</section>
    </div>;
}

function Detail({label, value, wide = false}: { label: string; value: string; wide?: boolean }) {
    return <div className={`rounded-xl bg-slate-50 p-4 ${wide ? "sm:col-span-2" : ""}`}><p
        className="text-xs font-bold uppercase tracking-wide text-slate-500">{label}</p><p
        className="mt-1 whitespace-pre-wrap text-sm text-slate-800">{value}</p></div>;
}
