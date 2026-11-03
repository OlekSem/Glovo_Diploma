import {API_URL} from "../../env";

export class ApiError extends Error {
    constructor(message: string, public status: number) {
        super(message);
    }
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
    const response = await fetch(`${API_URL}/api/admin${path}`, {
        credentials: "include",
        headers: {"Content-Type": "application/json", ...(options.headers ?? {})}, ...options
    });
    if (!response.ok) {
        const payload = await response.json().catch(() => null) as { message?: string } | null;
        throw new ApiError(payload?.message ?? "The request could not be completed.", response.status);
    }
    return response.status === 204 ? undefined as T : response.json() as Promise<T>;
}
