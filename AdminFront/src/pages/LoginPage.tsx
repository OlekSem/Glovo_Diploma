import {type FormEvent, useState} from "react";
import {useNavigate} from "react-router-dom";
import {adminApi} from "../services/adminApi";

export default function LoginPage({onLogin}: { onLogin: () => Promise<void> }) {
    const navigate = useNavigate();
    const [identifier, setIdentifier] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [saving, setSaving] = useState(false);
    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setSaving(true);
        setError("");
        try {
            await adminApi.login(identifier, password);
            await onLogin();
            navigate("/dashboard", {replace: true});
        } catch (cause) {
            setError(cause instanceof Error ? cause.message : "Unable to sign in.");
        } finally {
            setSaving(false);
        }
    };
    return <main className="grid min-h-screen place-items-center bg-slate-100 p-6">
        <section className="w-full max-w-md rounded-3xl bg-white p-8 shadow-xl shadow-slate-200/60">
            <div className="mb-8 flex items-center gap-3"><span
                className="grid h-11 w-11 place-items-center rounded-2xl bg-glovo-yellow text-lg font-black">g</span>
                <div><p className="font-bold">Glovo</p><p className="text-sm text-slate-500">Administration portal</p>
                </div>
            </div>
            <p className="text-xs font-bold uppercase tracking-[.22em] text-slate-500">Secure access</p><h1
            className="mt-2 text-3xl font-extrabold">Admin sign in</h1><p className="mt-2 text-slate-600">Use your
            administrator username or email and password.</p>
            <form onSubmit={submit} className="mt-7 space-y-5"><label className="block text-sm font-semibold">Username
                or email<input autoComplete="username" value={identifier}
                               onChange={event => setIdentifier(event.target.value)}
                               className="mt-2 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-slate-950"
                               required/></label>
                <label className="block text-sm font-semibold">Password<input type="password"
                                                                              autoComplete="current-password"
                                                                              value={password}
                                                                              onChange={event => setPassword(event.target.value)}
                                                                              className="mt-2 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-slate-950"
                                                                              required/></label>
                {error && <p role="alert" className="rounded-xl bg-rose-50 p-3 text-sm text-rose-800">{error}</p>}
                <button disabled={saving}
                        className="w-full rounded-xl bg-glovo-yellow px-4 py-3 font-bold text-slate-950 transition hover:bg-yellow-400 disabled:opacity-60">{saving ? "Signing in…" : "Sign in"}</button>
            </form>
        </section>
    </main>;
}
