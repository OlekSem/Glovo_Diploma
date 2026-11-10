import { SafeAreaView } from "react-native-safe-area-context";
import { ActivityIndicator, Text, TouchableOpacity, View } from "react-native";
import { useGetMeQuery, useLogoutMutation } from "../../store/service/apiAccount";
import { getSecureStore } from "../../utils/secureStore";

export const LogoutButton = () => {
  const [triggerLogout, { isLoading }] = useLogoutMutation();

  const handleLogout = async () => {
    const refreshToken = (await getSecureStore("refreshToken")) || "";

    try {
      await triggerLogout({ refreshToken }).unwrap();
    } catch (err) {
      // Handled by onQueryStarted lifecycle
    }
  };

  return (
    <TouchableOpacity
      onPress={handleLogout}
      disabled={isLoading}
      className={`mt-6 p-4 rounded-xl items-center justify-center ${isLoading ? 'bg-gray-400' : 'bg-red-500'}`}
    >
      {isLoading ? (
        <ActivityIndicator size="small" color="white" />
      ) : (
        <Text className="text-white font-bold text-base">Log Out</Text>
      )}
    </TouchableOpacity>
  );
};

export default function MeScreen() {
  const { data: user, isLoading, isError, error, refetch } = useGetMeQuery();

  if (isLoading) {
    return (
      <SafeAreaView className="flex-1 justify-center items-center">
        <ActivityIndicator size={"large"} color={"black"} />
      </SafeAreaView>
    );
  }

  if (isError) {
    console.log("GET ME ERROR:", error);

    return (
      <SafeAreaView className="flex-1 justify-center items-center p-4">
        <Text className="text-gray-600 text-center text-lg">Failed to load your profile.</Text>
        <TouchableOpacity onPress={() => refetch()} className="mt-4 p-2">
          <Text className="text-blue-500 underline text-base">Try again</Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView className="flex-1 p-6 bg-white justify-between">
      <View className="items-center mt-8">
        <Text className="text-gray-500 text-lg">Welcome, courier!</Text>
        <Text className="text-3xl font-extrabold text-black mt-2">
          {user?.firstName} {user?.lastName}
        </Text>
        <Text className="text-gray-400 mt-1">{user?.email}</Text>
      </View>

      <LogoutButton />
    </SafeAreaView>
  );
}
