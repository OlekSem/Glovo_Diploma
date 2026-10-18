import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import APP_ENV from "../../env/index";
import type {
    AuthTokenResponseDto,
    LoginPartnerDto,
    MessageResponse,
    PartnerUser,
    RegisterPartnerDto
} from "../interfaces/auth.ts";




export const authApi = createApi({
    reducerPath: "authApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${APP_ENV.API_URL}/api/partner/auth`,
        credentials: "include", // Enables sending/receiving HttpOnly cookies
    }),
    tagTypes: ["Auth", "User"],
    endpoints: (builder) => ({
        register: builder.mutation<AuthTokenResponseDto, RegisterPartnerDto>({
            query: (credentials) => ({
                url: "/register",
                method: "POST",
                body: credentials,
            }),
            invalidatesTags: ["User"],
        }),

        login: builder.mutation<AuthTokenResponseDto, LoginPartnerDto>({
            query: (credentials) => ({
                url: "/login",
                method: "POST",
                body: credentials,
            }),
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
    useLoginMutation,
    useGetMeQuery,
    useLazyGetMeQuery,
    useRefreshMutation,
    useLogoutMutation,
} = authApi;