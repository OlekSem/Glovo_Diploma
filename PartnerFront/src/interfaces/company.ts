export interface Company {
    id: string;
    name: string;
    description: string;
    ownerId: string;
    dateCreated: string;
    status: "Pending" | "Approved" | "Canceled";
    moderationComment?: string | null;
    reviewedAt?: string | null;
    category?: string | null;
    subcategory?: string | null;
    iconUrl?: string | null;
    bannerUrl?: string | null;
}

export interface CreateCompanyDto {
    name: string;
    description: string;
}

export interface UpdateCompanyDto {
    name: string;
    description: string;
    category?: string | null;
    subcategory?: string | null;
}

export interface CompanyBranch {
    id: string;
    companyId: string;
    phone: string;
    email: string;
    locality: string;
    region: string;
    address: string;
    postalCode: string;
    dateCreated: string;
}

export interface CreateCompanyBranchDto {
    phone: string;
    email: string;
    locality: string;
    region: string;
    address: string;
    postalCode: string;
}
