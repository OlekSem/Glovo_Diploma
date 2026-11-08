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

export interface ProductCategory {
    id: string;
    companyId: string;
    name: string;
    sortOrder: number;
    dateCreated: string;
}

export interface CompanyProduct {
    id: string;
    companyId: string;
    categoryId: string;
    name: string;
    description: string;
    price: number;
    imageUrl?: string | null;
    calories?: number | null;
    weightOrVolume?: number | null;
    weightType?: "g" | "ml" | null;
    dateCreated: string;
}

export interface ProductAddonItem {
    id: string;
    name: string;
    price: number;
}

export interface ProductAddonGroup {
    id: string;
    productId: string;
    name: string;
    minimumSelection: number;
    maximumSelection: number;
    items: ProductAddonItem[];
    dateCreated: string;
}

export interface CreateProductDto {
    name: string;
    description: string;
    price: number;
    calories?: number | null;
    weightOrVolume?: number | null;
    weightType?: "g" | "ml" | null;
}

export interface CreateProductAddonGroupDto {
    name: string;
    minimumSelection: number;
    maximumSelection: number;
    items: Array<{name: string; price: number}>;
}
export interface BranchMenuCategory { id: string; name: string; isAvailable: boolean; }
export interface BranchMenuProduct { id: string; name: string; description: string; price: number; imageUrl?: string | null; isAvailable: boolean; }
