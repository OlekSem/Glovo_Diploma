import React from "react";
import { Provider } from "react-redux";
import { Slot } from "expo-router";
import { store } from "@/store"; // твій redux store
import { SafeAreaProvider } from "react-native-safe-area-context";
import { StatusBar } from "expo-status-bar";
import '@/../global.css';// або "@/global.css" залежно від конфігу

export default function RootLayout() {
    return (
        <Provider store={store}>
            <SafeAreaProvider>
                <StatusBar style="auto" />
                <Slot />
            </SafeAreaProvider>
        </Provider>
    );
}