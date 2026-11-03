import {useEffect, useState} from "react";
import {Navigate, Route, Routes} from "react-router-dom";
import type {AdminUser} from "./interfaces/admin";
import LoginPage from "./pages/LoginPage";
import DashboardPage from "./pages/DashboardPage";
import {adminApi} from "./services/adminApi";

export default function App() {
    const [admin, setAdmin] = useState<AdminUser | null>(null);
    const [checking, setChecking] = useState(true);
    const hydrate = async () => {
        try {
            setAdmin(await adminApi.me());
        } catch {
            setAdmin(null);
        } finally {
            setChecking(false);
        }
    };
    useEffect(() => {
        void hydrate();
    }, []);
    if (checking) return <main className="grid min-h-screen place-items-center text-slate-600">Checking administrator
        session…</main>;
    return <Routes><Route path="/login"
                          element={admin ? <Navigate to="/dashboard" replace/> : <LoginPage onLogin={hydrate}/>}/>
        <Route path="/dashboard" element={admin ? <DashboardPage admin={admin} onLogout={() => setAdmin(null)}/> :
            <Navigate to="/login" replace/>}/><Route path="*" element={<Navigate to={admin ? "/dashboard" : "/login"}
                                                                                 replace/>}/></Routes>;
}
