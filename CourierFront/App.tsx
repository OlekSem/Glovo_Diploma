import {StatusBar} from 'expo-status-bar';
import {Button, Image, Separators, StyleSheet, Text, View} from 'react-native';
import {SafeAreaProvider} from "react-native-safe-area-context";
import {NavigationContainer} from "@react-navigation/native";
import {createNativeStackNavigator} from "@react-navigation/native-stack";

import WelcomeScreen from "./src/features/auth/screens/WelcomeScreen";
import LoginScreen from "./src/features/auth/screens/LoginScreen";
import SignUpScreen from "./src/features/auth/screens/SignUpScreen";

import "./global.css";

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
          <Stack.Screen name={"SignUp"} component={SignUpScreen}/>
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
