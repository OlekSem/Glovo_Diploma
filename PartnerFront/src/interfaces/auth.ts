export interface PartnerUser {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    dateCreated: string;
}

export interface RegisterPartnerDto {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
}

export interface LoginPartnerDto {
    email: string;
    password: string;
}

export interface AuthTokenResponseDto {
    user: PartnerUser;
}

export interface MessageResponse {
    message: string;
}