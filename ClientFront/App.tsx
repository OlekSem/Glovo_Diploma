import {StatusBar} from 'expo-status-bar';
import {Button, Image, Separators, StyleSheet, Text, View} from 'react-native';
import {SafeAreaProvider} from "react-native-safe-area-context";


import WelcomeScreen from "./src/features/auth/screens/WelcomeScreen";
import LoginScreen from "./src/features/auth/screens//login/LoginScreen";
import EmailScreen from "@/features/auth/screens/singup/EmailScreen";

import "./global.css";
import {createNativeStackNavigator} from "expo-router/native-stack";
import {NavigationContainer} from "expo-router/build/fork/NavigationContainer";

export type AuthStackParamList = {
    Welcome: undefined;
    Login: undefined;
    SignUp: undefined;
};

const Stack = createNativeStackNavigator<AuthStackParamList>();

export default function App() {
    return (
        <SafeAreaProvider>
            <NavigationContainer>
                <StatusBar style="auto"/>
                <Stack.Navigator
                    initialRouteName={"Welcome"}
                    screenOptions={
                        {headerShown: false}
                    }
                >
                    <Stack.Screen name={"Welcome"} component={WelcomeScreen}/>
                    <Stack.Screen name={"Login"} component={LoginScreen}/>
                    <Stack.Screen name={"SignUp"} component={EmailScreen}/>
                </Stack.Navigator>
            </NavigationContainer>
        </SafeAreaProvider>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#fff',
        alignItems: 'center',
        justifyContent: 'center',
    },
});
