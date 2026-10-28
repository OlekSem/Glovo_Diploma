import {type FormEvent, useState} from "react";
import {Link} from "react-router-dom";
import {parseApiFailure} from "../utils/apiErrors";
import {useCreateCompanyMutation} from "../services/companyApi";

const CreateCompanyPage = () => {
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [createCompany, {isLoading}] = useCreateCompanyMutation();
    const [message, setMessage] = useState("");
    const [failed, setFailed] = useState(false);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setMessage("");
        setFailed(false);
        setFieldErrors({});
        try {
            const company = await createCompany({name, description}).unwrap();
            setMessage(`${company.name} was created successfully.`);
            setName("");
            setDescription("");
        } catch (error) {
            const feedback = parseApiFailure(error);
            setFailed(true);
            setMessage(feedback.message);
            setFieldErrors(feedback.fields);
        }
    };

    const fieldClass = "mt-1 w-full rounded border px-3 py-2 outline-none focus:border-emerald-600";

    return <main className="mx-auto max-w-xl p-6"><Link className="text-sm text-emerald-700 hover:underline"
                                                        to="/dashboard">← Back to dashboard</Link><p
        className="mt-10 text-xs font-bold uppercase tracking-[0.2em] text-slate-500">Company</p><h1
        className="mt-2 text-3xl font-bold text-slate-900">Create company</h1><p className="mt-2 text-slate-600">Create
        the company that you will manage as a partner.</p>
        <section className="mt-8 rounded-xl border border-slate-200 bg-white p-7 shadow-sm">
            <form className="space-y-5" onSubmit={submit}><label className="block text-sm font-medium text-slate-700">Company
                name<input aria-invalid={Boolean(fieldErrors.name)}
                           className={fieldErrors.name ? `${fieldClass} border-red-500` : `${fieldClass} border-slate-300`}
                           value={name} onChange={(event) => setName(event.target.value)}
                           required/>{fieldErrors.name?.[0] &&
                    <span className="mt-1 block text-xs font-normal text-red-700">{fieldErrors.name[0]}</span>}
            </label><label className="block text-sm font-medium text-slate-700">Description<textarea
                aria-invalid={Boolean(fieldErrors.description)}
                className={fieldErrors.description ? `${fieldClass} border-red-500` : `${fieldClass} border-slate-300`}
                value={description} onChange={(event) => setDescription(event.target.value)} rows={5}
                required/>{fieldErrors.description?.[0] &&
                <span className="mt-1 block text-xs font-normal text-red-700">{fieldErrors.description[0]}</span>}
            </label>
                <button className="rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60"
                        disabled={isLoading}>{isLoading ? "Creating…" : "Create company"}</button>
                {message &&
                    <p className={`rounded p-3 text-sm ${failed ? "bg-red-50 text-red-700" : "bg-emerald-50 text-emerald-800"}`}>{message}</p>}
            </form>
        </section>
    </main>;
};

export default CreateCompanyPage;
