import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import APP_ENV from "../../env";
import type {Company, CompanyBranch, CreateCompanyBranchDto, CreateCompanyDto, UpdateCompanyDto} from "../interfaces/company";

export const companyApi = createApi({
    reducerPath: "companyApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${APP_ENV.API_URL}/api/partner/companies`,
        credentials: "include",
    }),
    tagTypes: ["Companies", "CompanyBranches"],
    endpoints: (builder) => ({
        createCompany: builder.mutation<Company, CreateCompanyDto>({
            query: (payload) => ({ url: "", method: "POST", body: payload }),
            invalidatesTags: ["Companies"],
        }),
        getMyCompanies: builder.query<Company[], void>({
            query: () => "/mine",
            providesTags: ["Companies"],
        }),
        getMyCompanyRequests: builder.query<Company[], void>({
            query: () => "/mine/requests",
            providesTags: ["Companies"],
        }),
        updateCompany: builder.mutation<Company, {companyId: string; company: UpdateCompanyDto}>({
            query: ({companyId, company}) => ({url: `/${companyId}`, method: "PUT", body: company}),
            invalidatesTags: ["Companies"],
        }),
        getCompanyBranches: builder.query<CompanyBranch[], string>({
            query: (companyId) => `/${companyId}/branches`,
            providesTags: (_result, _error, companyId) => [{type: "CompanyBranches", id: companyId}],
        }),
        createCompanyBranch: builder.mutation<CompanyBranch, {companyId: string; branch: CreateCompanyBranchDto}>({
            query: ({companyId, branch}) => ({url: `/${companyId}/branches`, method: "POST", body: branch}),
            invalidatesTags: (_result, _error, {companyId}) => [{type: "CompanyBranches", id: companyId}],
        }),
    }),
});

export const {
    useCreateCompanyMutation,
    useUpdateCompanyMutation,
    useGetMyCompaniesQuery,
    useGetMyCompanyRequestsQuery,
    useGetCompanyBranchesQuery,
    useCreateCompanyBranchMutation,
} = companyApi;
