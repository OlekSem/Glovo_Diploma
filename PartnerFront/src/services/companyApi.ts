import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import APP_ENV from "../../env";
import type { Company, CreateCompanyDto } from "../interfaces/company";

export const companyApi = createApi({
    reducerPath: "companyApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${APP_ENV.API_URL}/api/partner/companies`,
        credentials: "include",
    }),
    tagTypes: ["Companies"],
    endpoints: (builder) => ({
        createCompany: builder.mutation<Company, CreateCompanyDto>({
            query: (payload) => ({ url: "", method: "POST", body: payload }),
            invalidatesTags: ["Companies"],
        }),
        getMyCompanies: builder.query<Company[], void>({
            query: () => "/mine",
            providesTags: ["Companies"],
        }),
    }),
});

export const { useCreateCompanyMutation, useGetMyCompaniesQuery } = companyApi;
