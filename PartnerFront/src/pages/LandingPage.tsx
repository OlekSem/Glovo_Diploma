import { Link } from "react-router-dom";

const LandingPage = () => {
    return (
        <main className="mx-auto flex min-h-screen max-w-3xl flex-col items-center justify-center gap-5 p-6 text-center">
            <h1 className="text-4xl font-bold text-emerald-700">Partner portal</h1>
            <p className="text-slate-600">Manage your restaurant on our delivery platform.</p>
            <div className="flex gap-3">
                <Link className="rounded bg-emerald-600 px-4 py-2 font-medium text-white" to="/login">Log in</Link>
                <Link className="rounded border border-emerald-600 px-4 py-2 font-medium text-emerald-700" to="/register">Create account</Link>
            </div>
        </main>
    )
}

export default LandingPage;
