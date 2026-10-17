import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import APP_ENV from "../../env/index";
import type {
    AuthCourierResponseDto,
    CourierUser,
    LoginCourierDto,
    MessageResponse,
    RegisterCourierDto
} from "../interfaces/auth";

export const authApi = createApi({
    reducerPath: "courierAuthApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${APP_ENV.API_URL}/api/courier/auth`,
        credentials: "include", // Enables sending/receiving courier_access_token & courier_refresh_token cookies
    }),
    tagTypes: ["CourierAuth", "CourierUser"],
    endpoints: (builder) => ({
        register: builder.mutation<AuthCourierResponseDto, RegisterCourierDto>({
            query: (credentials) => ({
                url: "/register",
                method: "POST",
                body: credentials,
            }),
            invalidatesTags: ["CourierUser"],
        }),

        login: builder.mutation<AuthCourierResponseDto, LoginCourierDto>({
            query: (credentials) => ({
                url: "/login",
                method: "POST",
                body: credentials,
            }),
            invalidatesTags: ["CourierUser"],
        }),

        getMe: builder.query<CourierUser, void>({
            query: () => "/me",
            providesTags: ["CourierUser"],
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
            invalidatesTags: ["CourierUser"],
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