export interface Company {
    id: string;
    name: string;
    description: string;
    ownerId: string;
    dateCreated: string;
    status: "Pending" | "Approved" | "Canceled";
    moderationComment?: string | null;
    reviewedAt?: string | null;
}

export interface CreateCompanyDto {
    name: string;
    description: string;
}
