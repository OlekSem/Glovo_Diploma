import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

interface AuthState {
    isAuthenticated: boolean;
    isAdmin: boolean;
}

const initialState: AuthState = {
    isAuthenticated: false,
    isAdmin: false,
};

const authSlice = createSlice({
    name: "auth",
    initialState,
    reducers: {
        loginSuccess: (state, action: PayloadAction<{ isAdmin?: boolean } | void>) => {
            state.isAuthenticated = true;
            state.isAdmin = action.payload?.isAdmin ?? false;
        },
        logout: (state) => {
            state.isAuthenticated = false;
            state.isAdmin = false;
        },
    },
});

export const { loginSuccess, logout } = authSlice.actions;
export default authSlice.reducer;