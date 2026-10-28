export interface PartnerUser {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    phone: string;
    roles: string[];
    dateCreated: string;
}

export interface RegisterPartnerDto {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phone: string;
}

export interface LoginPartnerDto {
    email: string;
    password: string;
}

export interface AuthPartnerResponseDto {
    partner: PartnerUser;
}

export interface MessageResponse {
    message: string;
}

export interface RegistrationResponse extends MessageResponse {
    userId?: string;
}

export interface ConfirmEmailDto {
    userId: string;
    code: string;
}

export interface EmailDto {
    email: string;
}

export interface ResetPasswordDto {
    email: string;
    token: string;
    newPassword: string;
}

export interface ChangePasswordDto {
    currentPassword: string;
    newPassword: string;
}

export interface UpdatePartnerProfileDto {
    firstName: string;
    lastName: string;
    phone: string;
}
