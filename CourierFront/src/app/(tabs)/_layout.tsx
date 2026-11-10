import { Tabs } from "expo-router";
import { useEffect, useState } from "react";
import { ActivityIndicator, View } from "react-native";
import { getSecureStore } from "@/utils/secureStore";
import { router } from "expo-router";
import {NativeTabs} from "expo-router/native-tabs";

export default function TabsLayout() {
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function checkAuth() {
      try {
        const refreshToken = await getSecureStore("refreshToken");

        if (!refreshToken) {
          // Якщо токена немає — користувач НЕ залогінений. Викидаємо на авторизацію.
          router.replace("/(auth)");
        } else {
          setIsLoading(false);
        }
      } catch (error) {
        console.error("Помилка перевірки авторизації в табах:", error);
        router.replace("/(auth)");
      }
    }

    checkAuth();
  }, []);

  if (isLoading) {
    return (
      <View style={{ flex: 1, backgroundColor: "#fff", justifyContent: "center", alignItems: "center" }}>
        <ActivityIndicator size="large" color="#007AFF" />
      </View>
    );
  }

  return (
    <NativeTabs>
      <NativeTabs.Trigger name="index">
        <NativeTabs.Trigger.Label>Home</NativeTabs.Trigger.Label>
        <NativeTabs.Trigger.Icon sf={"house.fill"}/>
      </NativeTabs.Trigger>
      <NativeTabs.Trigger name="profile">
        <NativeTabs.Trigger.Label>Profile</NativeTabs.Trigger.Label>
        <NativeTabs.Trigger.Icon sf={"person.fill"}/>
      </NativeTabs.Trigger>
    </NativeTabs>
  );
}
