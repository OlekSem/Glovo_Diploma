export type CompanyStatus = "Pending" | "Approved" | "Canceled";

export interface AdminUser { id: string; username: string; email: string; firstName?: string | null; lastName?: string | null; }
export interface CompanyRequest {
  id: string; name: string; description: string; status: CompanyStatus; moderationComment?: string | null;
  dateCreated: string; reviewedAt?: string | null; ownerId: string; ownerName: string; ownerEmail: string; ownerPhone: string;
}
