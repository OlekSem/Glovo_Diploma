
import { Platform } from "react-native";
import * as SecureStore from "expo-secure-store";

const WEB_ACCESS_STORAGE = "accessToken";
const WEB_REFRESH_STORAGE = "refreshToken";

export async function saveSecureStore(
  key: string,
  value: string
): Promise<void> {
  if (Platform.OS === "web") {
    localStorage.setItem(key, value);
    return;
  }

  await SecureStore.setItemAsync(key, value);
}

export async function getSecureStore(
  key: string
): Promise<string | null> {
  if (Platform.OS === "web") {
    return localStorage.getItem(key);
  }

  return await SecureStore.getItemAsync(key);
}

export async function deleteSecureStore(
  key: string
): Promise<void> {
  if (Platform.OS === "web") {
    localStorage.removeItem(key);
    return;
  }

  await SecureStore.deleteItemAsync(key);
}
