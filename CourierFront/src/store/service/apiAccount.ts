import {logout as logoutAction} from "@/store/slices/authSlice";
import { createApi } from "@reduxjs/toolkit/query/react";
import { baseQueryWithReauth } from "@/utils/fetchBaseQuery";

export interface CourierUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  image: string | null;
  dateCreated: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

export interface LoginResponse {
  message: string;
  user: CourierUser;
  tokens: AuthTokens;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface RegisterResponse {
  message: string;
  userId: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface ConfirmEmailRequest {
  email: string;
  code: string;
}

export interface ConfirmEmailResponse {
  message: string;
}

export const apiAccount = createApi({
  reducerPath: "apiAccount",
  baseQuery: baseQueryWithReauth,
  tagTypes: ["Account"],

  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (body) => ({
        url: "/api/courier/auth/login",
        method: "POST",
        body,
      }),
    }),

    register: builder.mutation<RegisterResponse, RegisterRequest>({
      query: (body) => ({
        url: "/api/courier/auth/register",
        method: "POST",
        body,
      }),
    }),

    refresh: builder.mutation<RefreshResponse, RefreshRequest>({
      query: (body) => ({
        url: "/api/courier/auth/refresh",
        method: "POST",
        body,
      }),
    }),

    logout: builder.mutation<{ message: string }, RefreshRequest>({
      query: (body) => ({
        url: "/api/courier/auth/logout",
        method: "POST",
        body,
      }),
      // Clean up states automatically upon successful logout
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        try {
          await queryFulfilled;

          // 1. Clear secure store tokens & set state.accessToken = null
          dispatch(logoutAction());

          // 2. Completely reset the RTK Query cache (wipes getMe data)
          dispatch(apiAccount.util.resetApiState());
        } catch (error) {
          console.error("Server logout failed:", error);

          // Optional fallback: Force local logout anyway if the server token is invalid/expired
          dispatch(logoutAction());
          dispatch(apiAccount.util.resetApiState());
        }
      },
    }),

    getMe: builder.query<CourierUser, void>({
      query: () => "/api/courier/auth/me",
      providesTags: ["Account"],
    }),

    changePassword: builder.mutation<
      { message: string },
      { currentPassword: string; newPassword: string }
    >({
      query: (body) => ({
        url: "/api/courier/auth/change-password",
        method: "POST",
        body,
      }),
    }),

    confirmEmail: builder.mutation<ConfirmEmailResponse, ConfirmEmailRequest>({
      query: (body) => ({ url: "/api/courier/auth/confirm-email", method: "POST", body }),
    }),

    resendConfirmationEmail: builder.mutation<{ message: string }, { email: string }>({
      query: (body) => ({ url: "/api/courier/auth/resend-confirmation-email", method: "POST", body }),
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useRefreshMutation,
  useLogoutMutation,
  useGetMeQuery,
  useChangePasswordMutation,
  useConfirmEmailMutation,
  useResendConfirmationEmailMutation
} = apiAccount;
