import {type FormEvent, useState} from "react";
import {Link} from "react-router-dom";
import {useGetMeQuery, useUpdateProfileMutation} from "../services/authApi";
import type {PartnerUser} from "../interfaces/auth";
import {parseApiFailure} from "../utils/apiErrors";

const PartnerProfileForm = ({partner}: { partner: PartnerUser }) => {
    const [updateProfile, {isLoading: isSaving}] = useUpdateProfileMutation();
    const [firstName, setFirstName] = useState(partner.firstName);
    const [lastName, setLastName] = useState(partner.lastName);
    const [phone, setPhone] = useState(partner.phone);
    const [message, setMessage] = useState("");
    const [failed, setFailed] = useState(false);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setFailed(false);
        setMessage("");
        setFieldErrors({});
        try {
            const result = await updateProfile({firstName, lastName, phone}).unwrap();
            setMessage(result.message);
        } catch (error) {
            const feedback = parseApiFailure(error);
            setFailed(true);
            setMessage(feedback.message);
            setFieldErrors(feedback.fields);
        }
    };

    const fieldClass = "mt-1 w-full rounded border border-slate-300 px-3 py-2 outline-none focus:border-emerald-600";

    return <section className="rounded-xl border border-slate-200 bg-white p-7 shadow-sm">
        <div className="border-b border-slate-200 pb-5"><h2 className="text-lg font-bold text-slate-900">Contact
            information</h2><p className="mt-1 text-sm text-slate-500">Main partner account details</p></div>
        <form className="mt-6 space-y-4" onSubmit={submit}><label className="block text-sm font-medium text-slate-700">First
            name<input aria-invalid={Boolean(fieldErrors.firstName)}
                       className={fieldErrors.firstName ? `${fieldClass} border-red-500` : fieldClass} value={firstName}
                       onChange={(event) => setFirstName(event.target.value)} required/>{fieldErrors.firstName?.[0] &&
                <span className="mt-1 block text-xs font-normal text-red-700">{fieldErrors.firstName[0]}</span>}</label><label
            className="block text-sm font-medium text-slate-700">Last name<input
            aria-invalid={Boolean(fieldErrors.lastName)}
            className={fieldErrors.lastName ? `${fieldClass} border-red-500` : fieldClass} value={lastName}
            onChange={(event) => setLastName(event.target.value)} required/>{fieldErrors.lastName?.[0] &&
            <span className="mt-1 block text-xs font-normal text-red-700">{fieldErrors.lastName[0]}</span>}
        </label><label className="block text-sm font-medium text-slate-700">Phone number<input
            aria-invalid={Boolean(fieldErrors.phone)}
            className={fieldErrors.phone ? `${fieldClass} border-red-500` : fieldClass} type="tel" value={phone}
            onChange={(event) => setPhone(event.target.value)} required/>{fieldErrors.phone?.[0] &&
            <span className="mt-1 block text-xs font-normal text-red-700">{fieldErrors.phone[0]}</span>}</label><label
            className="block text-sm font-medium text-slate-700">Email<input
            className={`${fieldClass} cursor-not-allowed bg-slate-100 text-slate-500`} type="email"
            value={partner.email} readOnly/></label><p className="text-xs text-slate-500">Email changes require
            verification and are not available from this page.</p>
            <button className="rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60"
                    disabled={isSaving}>{isSaving ? "Saving…" : "Save changes"}</button>
            {message &&
                <p className={`rounded p-3 text-sm ${failed ? "bg-red-50 text-red-700" : "bg-emerald-50 text-emerald-800"}`}>{message}</p>}
        </form>
    </section>;
};

const PartnerProfilePage = () => {
    const {data: partner, isLoading, isError} = useGetMeQuery();

    if (isLoading) return <p className="p-6">Loading profile…</p>;
    if (isError || !partner) return <main className="p-6"><p>You need to log in to view your profile.</p><Link
        className="text-emerald-700 underline" to="/login">Log in</Link></main>;

    return <main className="mx-auto max-w-4xl p-6"><Link className="text-sm text-emerald-700 hover:underline"
                                                         to="/dashboard">← Back to dashboard</Link><p
        className="mt-10 text-xs font-bold uppercase tracking-[0.2em] text-slate-500">Partner profile</p><h1
        className="mt-2 text-3xl font-bold text-slate-900">Personal details</h1><p className="mt-2 text-slate-600">View
        and update the contact details saved in your partner profile.</p>
        <div className="mt-8 grid gap-6 md:grid-cols-[280px_1fr]">
            <aside className="rounded-xl bg-slate-950 p-7 text-white">
                <div
                    className="flex h-16 w-16 items-center justify-center rounded-2xl bg-amber-300 text-2xl font-bold text-slate-950">{partner.firstName.slice(0, 1)}{partner.lastName.slice(0, 1)}</div>
                <h2 className="mt-8 text-xl font-bold">{partner.firstName} {partner.lastName}</h2><p
                className="mt-5 break-all text-sm text-slate-300">{partner.phone}</p><p
                className="mt-3 break-all text-sm text-slate-300">{partner.email}</p><p
                className="mt-8 text-sm font-medium text-emerald-300">● Profile active</p></aside>
            <PartnerProfileForm key={partner.id} partner={partner}/></div>
    </main>;
};

export default PartnerProfilePage;
