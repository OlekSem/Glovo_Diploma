import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import APP_ENV from "../../env/index";

export const appApi = createApi({
    reducerPath: "appApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${APP_ENV.API_URL}/api`,
        credentials: "include", // Required to send HttpOnly cookies with requests
    }),

    // Just an example of how to use RTK
    tagTypes: ["Orders"],
    endpoints: (builder) => ({
        getOrders: builder.query<unknown, void>({
            query: () => "/orders/get",
            providesTags: ["Orders"],
        }),
    }),
});

export const { useGetOrdersQuery } = appApi;