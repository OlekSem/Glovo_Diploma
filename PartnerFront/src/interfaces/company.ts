export interface Company {
    id: string;
    name: string;
    description: string;
    ownerId: string;
    dateCreated: string;
}

export interface CreateCompanyDto {
    name: string;
    description: string;
}
