export interface ClientUser {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    dateCreated: string;
}

export interface RegisterClientDto {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
}

export interface LoginClientDto {
    email: string;
    password: string;
}

export interface AuthClientResponseDto {
    user: ClientUser;
}

export interface MessageResponse {
    message: string;
}