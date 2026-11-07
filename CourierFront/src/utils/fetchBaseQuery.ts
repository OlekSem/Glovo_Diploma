
import {
  BaseQueryApi,
  BaseQueryFn,
  FetchArgs,
  FetchBaseQueryError,
  fetchBaseQuery,
} from "@reduxjs/toolkit/query/react";

import {
  saveSecureStore,
  deleteSecureStore,
  getSecureStore,
} from "./secureStore";

import APP_ENV from "./env";

import {
  logout,
  setAccessToken,
  setRefreshToken,
} from "@/store/slices/authSlice";

import { router } from "expo-router";

interface CustomFetchArgs extends FetchArgs {
  meta?: {
    isBlob?: boolean;
    isMultipart?: boolean;
  };
}

interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
}

const baseQuery = fetchBaseQuery({
  baseUrl: `${APP_ENV.API_URL}`,

  prepareHeaders: async (headers) => {
    const token = await getSecureStore("accessToken");

    if (token) {
      headers.set("Authorization", `Bearer ${token}`);
    }

    return headers;
  },
});

let refreshPromise: ReturnType<typeof baseQuery> | null = null;
let redirecting = false;

export const baseQueryWithReauth: BaseQueryFn<
  string | CustomFetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {

  const isBlob =
    typeof args !== "string" && args.meta?.isBlob === true;

  const requestUrl = typeof args === "string" ? args : args.url;

  const isAuthRequest =
    /\/courier\/auth\/(login|register|refresh|forgot-password|reset-password|confirm-email|resend-confirmation-email)/i.test(
      requestUrl
    );

  const requestArgs =
    isBlob && typeof args !== "string"
      ? {
        ...args,
        responseHandler: (response: Response) => response.blob(),
      }
      : args;

  let result = await baseQuery(requestArgs, api, extraOptions);

  // Do not attempt refresh for login or other public authentication requests.
  if (result.error?.status !== 401 || isAuthRequest) {
    return result;
  }

  const refreshToken = await getSecureStore("refreshToken");

  if (!refreshToken) {
    await logoutAndRedirect(api);
    return result;
  }

  try {
    // Share one refresh request between simultaneous 401 responses.
    if (!refreshPromise) {
      refreshPromise = baseQuery(
        {
          url: "/courier/auth/refresh",
          method: "POST",
          body: { refreshToken },
        },
        api,
        extraOptions
      );
    }

    const currentRefreshPromise = refreshPromise;
    const refreshResult = await currentRefreshPromise;

    if (refreshPromise === currentRefreshPromise) {
      refreshPromise = null;
    }

    if (refreshResult.error) {
      await logoutAndRedirect(api);
      return result;
    }

    const tokens = refreshResult.data as RefreshResponse;

    if (!tokens?.accessToken || !tokens?.refreshToken) {
      await logoutAndRedirect(api);
      return result;
    }

    // Save both rotated tokens before retrying the original request.
    await saveSecureStore("accessToken", tokens.accessToken);
    await saveSecureStore("refreshToken", tokens.refreshToken);

    api.dispatch(setAccessToken(tokens.accessToken));
    api.dispatch(setRefreshToken(tokens.refreshToken));

    // Retry original request with the new access token.
    result = await baseQuery(requestArgs, api, extraOptions);

    return result;
  } catch (error) {
    refreshPromise = null;
    await logoutAndRedirect(api);

    return {
      error: {
        status: "CUSTOM_ERROR",
        error: "Authentication failed. Please log in again.",
        data: error,
      },
    };
  }
};

async function logoutAndRedirect(api: BaseQueryApi) {
  await deleteSecureStore("accessToken");
  await deleteSecureStore("refreshToken");

  api.dispatch(logout());

  if (redirecting) return;

  redirecting = true;

  router.replace("/(auth)");

  // Allow a future login session to redirect normally.
  setTimeout(() => {
    redirecting = false;
  }, 1000);
}
