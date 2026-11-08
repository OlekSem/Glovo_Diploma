import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import APP_ENV from "../../env";
import type {BranchMenuCategory, BranchMenuProduct, Company, CompanyBranch, CompanyProduct, CreateCompanyBranchDto, CreateCompanyDto, CreateProductAddonGroupDto, CreateProductDto, ProductAddonGroup, ProductCategory, UpdateCompanyDto} from "../interfaces/company";

export const companyApi = createApi({
    reducerPath: "companyApi",
    baseQuery: fetchBaseQuery({
        baseUrl: `${APP_ENV.API_URL}/api/partner/companies`,
        credentials: "include",
    }),
    tagTypes: ["Companies", "CompanyBranches", "ProductCategories", "Products", "ProductAddons"],
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
        updateCompanyBranch: builder.mutation<CompanyBranch, {companyId: string; branchId: string; branch: CreateCompanyBranchDto}>({
            query: ({companyId, branchId, branch}) => ({url: `/${companyId}/branches/${branchId}`, method: "PUT", body: branch}),
            invalidatesTags: ["CompanyBranches"],
        }),
        getProductCategories: builder.query<ProductCategory[], string>({
            query: (companyId) => `/${companyId}/catalog/categories`,
            providesTags: (_result, _error, companyId) => [{type: "ProductCategories", id: companyId}],
        }),
        createProductCategory: builder.mutation<ProductCategory, {companyId: string; name: string}>({
            query: ({companyId, name}) => ({url: `/${companyId}/catalog/categories`, method: "POST", body: {name}}),
            invalidatesTags: (_result, _error, {companyId}) => [{type: "ProductCategories", id: companyId}],
        }),
        deleteProductCategory: builder.mutation<void, {companyId: string; categoryId: string}>({
            query: ({companyId, categoryId}) => ({url: `/${companyId}/catalog/categories/${categoryId}`, method: "DELETE"}),
            invalidatesTags: (_result, _error, {companyId}) => [{type: "ProductCategories", id: companyId}],
        }),
        updateProductCategory: builder.mutation<ProductCategory, {companyId: string; categoryId: string; name: string}>({
            query: ({companyId, categoryId, name}) => ({url: `/${companyId}/catalog/categories/${categoryId}`, method: "PUT", body: {name}}),
            invalidatesTags: (_result, _error, {companyId}) => [{type: "ProductCategories", id: companyId}],
        }),
        reorderProductCategories: builder.mutation<void, {companyId: string; categoryIds: string[]}>({
            query: ({companyId, categoryIds}) => ({url: `/${companyId}/catalog/categories/order`, method: "PUT", body: {categoryIds}}),
            invalidatesTags: (_result, _error, {companyId}) => [{type: "ProductCategories", id: companyId}],
        }),
        getProducts: builder.query<CompanyProduct[], {companyId: string; categoryId: string}>({
            query: ({companyId, categoryId}) => `/${companyId}/catalog/categories/${categoryId}/products`,
            providesTags: (_result, _error, {categoryId}) => [{type: "Products", id: categoryId}],
        }),
        createProduct: builder.mutation<CompanyProduct, {companyId: string; categoryId: string; product: CreateProductDto}>({
            query: ({companyId, categoryId, product}) => ({url: `/${companyId}/catalog/categories/${categoryId}/products`, method: "POST", body: product}),
            invalidatesTags: (_result, _error, {categoryId}) => [{type: "Products", id: categoryId}],
        }),
        deleteProduct: builder.mutation<void, {companyId: string; categoryId: string; productId: string}>({
            query: ({companyId, productId}) => ({url: `/${companyId}/catalog/products/${productId}`, method: "DELETE"}),
            invalidatesTags: (_result, _error, {categoryId}) => [{type: "Products", id: categoryId}],
        }),
        getProductAddons: builder.query<ProductAddonGroup[], {companyId: string; productId: string}>({
            query: ({companyId, productId}) => `/${companyId}/catalog/products/${productId}/addons`,
            providesTags: (_result, _error, {productId}) => [{type: "ProductAddons", id: productId}],
        }),
        createProductAddonGroup: builder.mutation<ProductAddonGroup, {companyId: string; productId: string; group: CreateProductAddonGroupDto}>({
            query: ({companyId, productId, group}) => ({url: `/${companyId}/catalog/products/${productId}/addons`, method: "POST", body: group}),
            invalidatesTags: (_result, _error, {productId}) => [{type: "ProductAddons", id: productId}],
        }),
        deleteProductAddonGroup: builder.mutation<void, {companyId: string; productId: string; addonGroupId: string}>({
            query: ({companyId, addonGroupId}) => ({url: `/${companyId}/catalog/addons/${addonGroupId}`, method: "DELETE"}),
            invalidatesTags: (_result, _error, {productId}) => [{type: "ProductAddons", id: productId}],
        }),
        updateProductAddonGroup: builder.mutation<ProductAddonGroup, {companyId: string; productId: string; addonGroupId: string; group: CreateProductAddonGroupDto}>({
            query: ({companyId, addonGroupId, group}) => ({url: `/${companyId}/catalog/addons/${addonGroupId}`, method: "PUT", body: group}),
            invalidatesTags: (_result, _error, {productId}) => [{type: "ProductAddons", id: productId}],
        }),
        getBranchMenuCategories: builder.query<BranchMenuCategory[], {companyId: string; branchId: string}>({ query: ({companyId, branchId}) => `/${companyId}/branches/${branchId}/menu/categories`, providesTags: ["CompanyBranches"] }),
        setBranchMenuCategory: builder.mutation<void, {companyId: string; branchId: string; categoryId: string; isAvailable: boolean}>({ query: ({companyId, branchId, categoryId, isAvailable}) => ({url: `/${companyId}/branches/${branchId}/menu/categories/${categoryId}`, method: "PUT", body: {isAvailable}}), invalidatesTags: ["CompanyBranches"] }),
        getBranchMenuProducts: builder.query<BranchMenuProduct[], {companyId: string; branchId: string; categoryId: string}>({ query: ({companyId, branchId, categoryId}) => `/${companyId}/branches/${branchId}/menu/categories/${categoryId}/products`, providesTags: ["CompanyBranches"] }),
        setBranchMenuProduct: builder.mutation<void, {companyId: string; branchId: string; productId: string; isAvailable: boolean}>({ query: ({companyId, branchId, productId, isAvailable}) => ({url: `/${companyId}/branches/${branchId}/menu/products/${productId}`, method: "PUT", body: {isAvailable}}), invalidatesTags: ["CompanyBranches"] }),
    }),
});

export const {
    useCreateCompanyMutation,
    useUpdateCompanyMutation,
    useGetMyCompaniesQuery,
    useGetMyCompanyRequestsQuery,
    useGetCompanyBranchesQuery,
    useCreateCompanyBranchMutation,
    useUpdateCompanyBranchMutation,
    useGetProductCategoriesQuery,
    useCreateProductCategoryMutation,
    useDeleteProductCategoryMutation,
    useUpdateProductCategoryMutation,
    useReorderProductCategoriesMutation,
    useGetProductsQuery,
    useCreateProductMutation,
    useDeleteProductMutation,
    useGetProductAddonsQuery,
    useCreateProductAddonGroupMutation,
    useDeleteProductAddonGroupMutation,
    useUpdateProductAddonGroupMutation,
    useGetBranchMenuCategoriesQuery, useSetBranchMenuCategoryMutation, useGetBranchMenuProductsQuery, useSetBranchMenuProductMutation,
} = companyApi;
