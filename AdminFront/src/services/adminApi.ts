import type {AdminUser, CompanyRequest, CompanyStatus} from "../interfaces/admin";
import {api} from "./api";

export const adminApi = {
    login: (identifier: string, password: string) => api<AdminUser>("/auth/login", {
        method: "POST",
        body: JSON.stringify({identifier, password})
    }),
    me: () => api<AdminUser>("/auth/me"),
    logout: () => api<{ message: string }>("/auth/logout", {method: "POST"}),
    companies: (status: CompanyStatus | "ALL") => api<CompanyRequest[]>(`/companies${status === "ALL" ? "" : `?status=${status}`}`),
    moderate: (id: string, action: "approve" | "cancel", comment: string) => api<CompanyRequest>(`/companies/${id}/${action}`, {
        method: "POST",
        body: JSON.stringify({comment})
    }),
};
