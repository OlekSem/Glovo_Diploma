import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import APP_ENV from "../../env/index";
import type {
    AuthClientResponseDto,
    ClientUser,
    LoginClientDto,
    MessageResponse,
    RegisterClientDto
} from "../interfaces/auth";

export const authApi = createApi({
    reducerPath: "clientAuthApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${APP_ENV.API_URL}/api/client/auth`,
        credentials: "include", // Sends and receives client_access_token & client_refresh_token cookies
    }),
    tagTypes: ["ClientAuth", "ClientUser"],
    endpoints: (builder) => ({
        register: builder.mutation<AuthClientResponseDto, RegisterClientDto>({
            query: (credentials) => ({
                url: "/register",
                method: "POST",
                body: credentials,
            }),
            invalidatesTags: ["ClientUser"],
        }),

        login: builder.mutation<AuthClientResponseDto, LoginClientDto>({
            query: (credentials) => ({
                url: "/login",
                method: "POST",
                body: credentials,
            }),
            invalidatesTags: ["ClientUser"],
        }),

        getMe: builder.query<ClientUser, void>({
            query: () => "/me",
            providesTags: ["ClientUser"],
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
            invalidatesTags: ["ClientUser"],
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