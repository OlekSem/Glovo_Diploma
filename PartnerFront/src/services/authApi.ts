import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import APP_ENV from "../../env/index";
import type {
    AuthPartnerResponseDto,
    ChangePasswordDto,
    ConfirmEmailDto,
    EmailDto,
    LoginPartnerDto,
    MessageResponse,
    PartnerUser,
    RegisterPartnerDto,
    RegistrationResponse,
    ResetPasswordDto,
} from "../interfaces/auth.ts";




export const authApi = createApi({
    reducerPath: "authApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${APP_ENV.API_URL}/api/partner/auth`,
        credentials: "include", // Enables sending/receiving HttpOnly cookies
    }),
    tagTypes: ["Auth", "User"],
    endpoints: (builder) => ({
        register: builder.mutation<RegistrationResponse, RegisterPartnerDto>({
            query: (credentials) => ({
                url: "/register",
                method: "POST",
                body: credentials,
            }),
            invalidatesTags: ["User"],
        }),

        confirmEmail: builder.mutation<MessageResponse, ConfirmEmailDto>({
            query: (payload) => ({ url: "/confirm-email", method: "POST", body: payload }),
        }),

        resendConfirmationEmail: builder.mutation<RegistrationResponse, EmailDto>({
            query: (payload) => ({ url: "/resend-confirmation-email", method: "POST", body: payload }),
        }),

        login: builder.mutation<AuthPartnerResponseDto, LoginPartnerDto>({
            query: (credentials) => ({
                url: "/login",
                method: "POST",
                body: credentials,
            }),
            invalidatesTags: ["User"],
        }),

        forgotPassword: builder.mutation<MessageResponse, EmailDto>({
            query: (payload) => ({ url: "/forgot-password", method: "POST", body: payload }),
        }),

        resetPassword: builder.mutation<MessageResponse, ResetPasswordDto>({
            query: (payload) => ({ url: "/reset-password", method: "POST", body: payload }),
        }),

        changePassword: builder.mutation<MessageResponse, ChangePasswordDto>({
            query: (payload) => ({ url: "/change-password", method: "POST", body: payload }),
            invalidatesTags: ["User"],
        }),

        getMe: builder.query<PartnerUser, void>({
            query: () => "/me",
            providesTags: ["User"],
        }),

        refresh: builder.mutation<MessageResponse, void>({
            query: () => ({
                url: "/refresh",
                method: "POST",
            }),
        }),

        logout: builder.mutation<MessageResponse, void>({
            query: () => ({
                url: "/logout",
                method: "POST",
            }),
            invalidatesTags: ["User"],
        }),
    }),
});

export const {
    useRegisterMutation,
    useConfirmEmailMutation,
    useResendConfirmationEmailMutation,
    useLoginMutation,
    useForgotPasswordMutation,
    useResetPasswordMutation,
    useChangePasswordMutation,
    useGetMeQuery,
    useLazyGetMeQuery,
    useRefreshMutation,
    useLogoutMutation,
} = authApi;
