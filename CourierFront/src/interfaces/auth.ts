export interface CourierUser {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    dateCreated: string;
}

export interface RegisterCourierDto {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
}

export interface LoginCourierDto {
    email: string;
    password: string;
}

export interface AuthCourierResponseDto {
    user: CourierUser;
}

export interface MessageResponse {
    message: string;
}