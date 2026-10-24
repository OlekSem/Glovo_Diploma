export interface PartnerUser {
    id: string;
    email: string;
    firstName: string;
    lastName: string | null;
    image: string | null;
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

export interface AuthPartnerResponseDto {
    partner: PartnerUser;
}

export interface MessageResponse {
    message: string;
}

export interface RegistrationResponse extends MessageResponse {
    devUserId?: string;
    devToken?: string;
}

export interface ConfirmEmailDto {
    userId: string;
    token: string;
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
