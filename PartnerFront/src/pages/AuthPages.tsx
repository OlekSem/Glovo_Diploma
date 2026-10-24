import { type FormEvent, type ReactNode, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useAppDispatch } from "../store";
import { loginSuccess } from "../services/authSlice";
import {
    useConfirmEmailMutation,
    useChangePasswordMutation,
    useForgotPasswordMutation,
    useLoginMutation,
    useRegisterMutation,
    useResendConfirmationEmailMutation,
    useResetPasswordMutation,
} from "../services/authApi";

type AuthFormProps = { title: string; children: ReactNode };

const AuthForm = ({ title, children }: AuthFormProps) => (
    <main className="mx-auto flex min-h-screen max-w-md items-center p-5">
        <section className="w-full rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
            <Link className="text-sm text-emerald-700 hover:underline" to="/">← Partner portal</Link>
            <h1 className="mt-4 text-2xl font-bold text-slate-900">{title}</h1>
            <div className="mt-5">{children}</div>
        </section>
    </main>
);

const Field = ({ label, type = "text", value, onChange, error }: { label: string; type?: string; value: string; onChange: (value: string) => void; error?: string }) => (
    <label className="mb-4 block text-sm font-medium text-slate-700">
        {label}
        <input aria-invalid={Boolean(error)} className={`mt-1 w-full rounded border px-3 py-2 outline-none focus:border-emerald-600 ${error ? "border-red-500" : "border-slate-300"}`} type={type} value={value} onChange={(event) => onChange(event.target.value)} required />
        {error && <span className="mt-1 block text-xs font-normal text-red-700">{error}</span>}
    </label>
);

const ApiMessage = ({ message, error }: { message: string; error: boolean }) => message ? <p className={`mt-4 rounded p-3 text-sm ${error ? "bg-red-50 text-red-700" : "bg-emerald-50 text-emerald-800"}`}>{message}</p> : null;

type FormFeedback = { message: string; fields: Record<string, string[]> };
const emptyFeedback: FormFeedback = { message: "", fields: {} };

const toFieldName = (name: string) => {
    const leaf = name.split(".").at(-1) ?? name;
    return leaf ? leaf.charAt(0).toLowerCase() + leaf.slice(1) : name;
};

const messagesFrom = (value: unknown): string[] => {
    if (typeof value === "string") return [value];
    if (Array.isArray(value)) return value.flatMap(messagesFrom);
    if (typeof value === "object" && value !== null) {
        const item = value as Record<string, unknown>;
        if (typeof item.description === "string") return [item.description];
        if (typeof item.message === "string") return [item.message];
        if (typeof item.title === "string") return [item.title];
    }
    return [];
};

const fieldForIdentityError = (value: unknown) => {
    if (typeof value !== "object" || value === null) return undefined;
    const code = (value as { code?: unknown }).code;
    return typeof code === "string" && code.startsWith("Password") ? "password" : undefined;
};

const parseApiFailure = (error: unknown): FormFeedback => {
    const fallback = "The request could not be completed.";
    const response = typeof error === "object" && error !== null && "data" in error
        ? (error as { data?: unknown }).data
        : undefined;
    if (typeof response === "string") return { message: response, fields: {} };

    const fields: Record<string, string[]> = {};
    const addField = (field: string, messages: string[]) => {
        if (messages.length > 0) fields[toFieldName(field)] = messages;
    };

    if (Array.isArray(response)) {
        response.forEach((item) => {
            const messages = messagesFrom(item);
            const field = fieldForIdentityError(item);
            if (field) addField(field, messages);
        });
        const messages = response.flatMap(messagesFrom);
        return { message: messages.join(" ") || fallback, fields };
    }

    if (typeof response === "object" && response !== null) {
        const body = response as Record<string, unknown>;
        if (typeof body.errors === "object" && body.errors !== null && !Array.isArray(body.errors)) {
            Object.entries(body.errors as Record<string, unknown>).forEach(([field, value]) => addField(field, messagesFrom(value)));
        } else if (Array.isArray(body.errors)) {
            body.errors.forEach((item) => {
                const messages = messagesFrom(item);
                const field = fieldForIdentityError(item);
                if (field) addField(field, messages);
            });
        }
        const message = typeof body.message === "string" ? body.message
            : typeof body.title === "string" ? body.title
                : typeof body.description === "string" ? body.description
                    : Object.keys(fields).length > 0 ? "Please correct the highlighted fields." : fallback;
        return { message, fields };
    }

    if (typeof error === "object" && error !== null && "error" in error) {
        const message = (error as { error?: unknown }).error;
        if (typeof message === "string") return { message, fields: {} };
    }
    return { message: fallback, fields: {} };
};

export const LoginPage = () => {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [feedback, setFeedback] = useState<FormFeedback>(emptyFeedback);
    const [failed, setFailed] = useState(false);
    const [login, { isLoading }] = useLoginMutation();
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const submit = async (event: FormEvent) => {
        event.preventDefault();
        setFeedback(emptyFeedback); setFailed(false);
        try {
            await login({ email, password }).unwrap();
            dispatch(loginSuccess());
            navigate("/dashboard");
        } catch (error) { setFailed(true); setFeedback(parseApiFailure(error)); }
    };
    return <AuthForm title="Log in"><form onSubmit={submit}><Field label="Email" type="email" value={email} onChange={setEmail} error={feedback.fields.email?.[0]} /><Field label="Password" type="password" value={password} onChange={setPassword} error={feedback.fields.password?.[0]} /><button className="w-full rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60" disabled={isLoading}>{isLoading ? "Logging in…" : "Log in"}</button></form><ApiMessage message={feedback.message} error={failed} /><div className="mt-4 flex flex-col gap-2 text-sm text-emerald-700"><Link to="/forgot-password">Forgot password?</Link><Link to="/resend-confirmation">Resend email confirmation</Link><Link to="/register">Need an account? Register</Link></div></AuthForm>;
};

export const RegisterPage = () => {
    const [firstName, setFirstName] = useState(""); const [lastName, setLastName] = useState(""); const [email, setEmail] = useState(""); const [password, setPassword] = useState("");
    const [feedback, setFeedback] = useState<FormFeedback>(emptyFeedback); const [failed, setFailed] = useState(false);
    const [register, { isLoading }] = useRegisterMutation();
    const submit = async (event: FormEvent) => { event.preventDefault(); setFailed(false); setFeedback(emptyFeedback); try { const result = await register({ firstName, lastName, email, password }).unwrap(); setFeedback({ message: result.message, fields: {} }); } catch (error) { setFailed(true); setFeedback(parseApiFailure(error)); } };
    return <AuthForm title="Create partner account"><form onSubmit={submit}><Field label="First name" value={firstName} onChange={setFirstName} error={feedback.fields.firstName?.[0]} /><Field label="Last name" value={lastName} onChange={setLastName} error={feedback.fields.lastName?.[0]} /><Field label="Email" type="email" value={email} onChange={setEmail} error={feedback.fields.email?.[0]} /><Field label="Password (at least 6 characters)" type="password" value={password} onChange={setPassword} error={feedback.fields.password?.[0]} /><button className="w-full rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60" disabled={isLoading}>{isLoading ? "Creating…" : "Create account"}</button></form><ApiMessage message={feedback.message} error={failed} /><p className="mt-4 text-sm text-slate-600">Already registered? <Link className="text-emerald-700" to="/login">Log in</Link></p></AuthForm>;
};

export const ForgotPasswordPage = () => {
    const [email, setEmail] = useState(""); const [feedback, setFeedback] = useState<FormFeedback>(emptyFeedback); const [failed, setFailed] = useState(false);
    const [forgotPassword, { isLoading }] = useForgotPasswordMutation();
    const submit = async (event: FormEvent) => { event.preventDefault(); setFailed(false); setFeedback(emptyFeedback); try { setFeedback({ message: (await forgotPassword({ email }).unwrap()).message, fields: {} }); } catch (error) { setFailed(true); setFeedback(parseApiFailure(error)); } };
    return <AuthForm title="Forgot password"><p className="mb-4 text-sm text-slate-600">Enter the email for your partner account.</p><form onSubmit={submit}><Field label="Email" type="email" value={email} onChange={setEmail} error={feedback.fields.email?.[0]} /><button className="w-full rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60" disabled={isLoading}>{isLoading ? "Sending…" : "Send reset link"}</button></form><ApiMessage message={feedback.message} error={failed} /><Link className="mt-4 block text-sm text-emerald-700" to="/reset-password">Already have a reset link?</Link></AuthForm>;
};

export const ResetPasswordPage = () => {
    const [params] = useSearchParams(); const [email, setEmail] = useState(params.get("email") ?? ""); const [token, setToken] = useState(params.get("token") ?? ""); const [newPassword, setNewPassword] = useState(""); const [feedback, setFeedback] = useState<FormFeedback>(emptyFeedback); const [failed, setFailed] = useState(false);
    const [resetPassword, { isLoading }] = useResetPasswordMutation();
    const submit = async (event: FormEvent) => { event.preventDefault(); setFailed(false); setFeedback(emptyFeedback); try { setFeedback({ message: (await resetPassword({ email, token, newPassword }).unwrap()).message, fields: {} }); } catch (error) { setFailed(true); setFeedback(parseApiFailure(error)); } };
    return <AuthForm title="Reset password"><form onSubmit={submit}><Field label="Email" type="email" value={email} onChange={setEmail} error={feedback.fields.email?.[0]} /><Field label="Reset token" value={token} onChange={setToken} error={feedback.fields.token?.[0]} /><Field label="New password" type="password" value={newPassword} onChange={setNewPassword} error={feedback.fields.newPassword?.[0] ?? feedback.fields.password?.[0]} /><button className="w-full rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60" disabled={isLoading}>{isLoading ? "Resetting…" : "Reset password"}</button></form><ApiMessage message={feedback.message} error={failed} /><Link className="mt-4 block text-sm text-emerald-700" to="/login">Back to login</Link></AuthForm>;
};

export const ConfirmEmailPage = () => {
    const [params] = useSearchParams(); const [userId, setUserId] = useState(params.get("userId") ?? ""); const [token, setToken] = useState(params.get("token") ?? ""); const [feedback, setFeedback] = useState<FormFeedback>(emptyFeedback); const [failed, setFailed] = useState(false);
    const [confirmEmail, { isLoading }] = useConfirmEmailMutation();
    const submit = async (event: FormEvent) => { event.preventDefault(); setFailed(false); setFeedback(emptyFeedback); try { setFeedback({ message: (await confirmEmail({ userId, token }).unwrap()).message, fields: {} }); } catch (error) { setFailed(true); setFeedback(parseApiFailure(error)); } };
    return <AuthForm title="Confirm email"><p className="mb-4 text-sm text-slate-600">The confirmation link should fill these values automatically.</p><form onSubmit={submit}><Field label="User ID" value={userId} onChange={setUserId} error={feedback.fields.userId?.[0]} /><Field label="Confirmation token" value={token} onChange={setToken} error={feedback.fields.token?.[0]} /><button className="w-full rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60" disabled={isLoading}>{isLoading ? "Confirming…" : "Confirm email"}</button></form><ApiMessage message={feedback.message} error={failed} /><Link className="mt-4 block text-sm text-emerald-700" to="/login">Go to login</Link></AuthForm>;
};

export const ResendConfirmationPage = () => {
    const [email, setEmail] = useState(""); const [feedback, setFeedback] = useState<FormFeedback>(emptyFeedback); const [failed, setFailed] = useState(false);
    const [resend, { isLoading }] = useResendConfirmationEmailMutation();
    const submit = async (event: FormEvent) => { event.preventDefault(); setFailed(false); setFeedback(emptyFeedback); try { setFeedback({ message: (await resend({ email }).unwrap()).message, fields: {} }); } catch (error) { setFailed(true); setFeedback(parseApiFailure(error)); } };
    return <AuthForm title="Resend confirmation email"><form onSubmit={submit}><Field label="Email" type="email" value={email} onChange={setEmail} error={feedback.fields.email?.[0]} /><button className="w-full rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60" disabled={isLoading}>{isLoading ? "Sending…" : "Resend confirmation"}</button></form><ApiMessage message={feedback.message} error={failed} /><Link className="mt-4 block text-sm text-emerald-700" to="/login">Back to login</Link></AuthForm>;
};

export const ChangePasswordPage = () => {
    const [currentPassword, setCurrentPassword] = useState(""); const [newPassword, setNewPassword] = useState(""); const [feedback, setFeedback] = useState<FormFeedback>(emptyFeedback); const [failed, setFailed] = useState(false);
    const [changePassword, { isLoading }] = useChangePasswordMutation();
    const navigate = useNavigate();
    const submit = async (event: FormEvent) => { event.preventDefault(); setFailed(false); setFeedback(emptyFeedback); try { setFeedback({ message: (await changePassword({ currentPassword, newPassword }).unwrap()).message, fields: {} }); } catch (error) { setFailed(true); setFeedback(parseApiFailure(error)); } };
    return <AuthForm title="Change password"><p className="mb-4 text-sm text-slate-600">Changing your password signs you out on every device.</p><form onSubmit={submit}><Field label="Current password" type="password" value={currentPassword} onChange={setCurrentPassword} error={feedback.fields.currentPassword?.[0]} /><Field label="New password" type="password" value={newPassword} onChange={setNewPassword} error={feedback.fields.newPassword?.[0] ?? feedback.fields.password?.[0]} /><button className="w-full rounded bg-emerald-600 px-4 py-2 font-medium text-white disabled:opacity-60" disabled={isLoading}>{isLoading ? "Changing…" : "Change password"}</button></form><ApiMessage message={feedback.message} error={failed} /><button className="mt-4 text-sm text-emerald-700" onClick={() => navigate("/dashboard")}>Back to dashboard</button></AuthForm>;
};
