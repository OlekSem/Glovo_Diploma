import {configureStore, combineReducers} from '@reduxjs/toolkit';
import {authApi} from "../services/authApi.ts";
import {type TypedUseSelectorHook, useDispatch, useSelector} from "react-redux";
import authReducer from "../services/authSlice.ts"
import {appApi} from "../services/appApi.ts";
// import {appApi} from "../services/appApi.ts";
// import {authorizedUserApi} from "../services/AuthorizedUser/AuthorizedUserService.ts";

const rootReducer = combineReducers({
    [authApi.reducerPath]: authApi.reducer,
    [appApi.reducerPath]: appApi.reducer,
    auth: authReducer
    // [authorizedUserApi.reducerPath]: authorizedUserApi.reducer,
});

export const setupStore = () => {
    return configureStore({
        reducer: rootReducer,
        middleware: (getDefaultMiddleware) =>
            getDefaultMiddleware().concat(
                authApi.middleware,
                appApi.middleware
                // authorizedUserApi.middleware
            ),

    });
};


export type RootState = ReturnType<typeof rootReducer>;
export type AppStore = ReturnType<typeof setupStore>;
export type AppDispatch = AppStore['dispatch'];

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;