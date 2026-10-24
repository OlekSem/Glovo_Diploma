import { Link, useNavigate } from "react-router-dom";
import { useAppDispatch } from "../store";
import { logout as clearSession } from "../services/authSlice";
import { useGetMeQuery, useLogoutMutation } from "../services/authApi";

const PartnerDashboard = () => {
    const { data: partner, isLoading, isError } = useGetMeQuery();
    const [logout] = useLogoutMutation();
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const signOut = async () => { try { await logout().unwrap(); } finally { dispatch(clearSession()); navigate("/login"); } };
    if (isLoading) return <p className="p-6">Loading session…</p>;
    if (isError || !partner) return <main className="p-6"><p>You need to log in to access the partner dashboard.</p><Link className="text-emerald-700 underline" to="/login">Log in</Link></main>;
    return <main className="mx-auto max-w-3xl p-6"><h1
        className="text-3xl font-bold">Welcome, {partner.firstName ?? partner.email}</h1><p
        className="mt-2 text-slate-600">Your authenticated partner area is ready for future restaurant-management
        features.</p>
        <div className="mt-6 flex gap-3"><Link className="rounded border border-emerald-700 px-4 py-2 text-emerald-700"
                                               to="/change-password">Change password</Link>
            <button className="rounded bg-slate-800 px-4 py-2 text-white" onClick={signOut}>Log out</button>
        </div>
    </main>;
};

export default PartnerDashboard;
