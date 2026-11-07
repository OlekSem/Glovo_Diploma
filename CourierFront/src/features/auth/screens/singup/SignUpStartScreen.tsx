import { View, Text, TouchableOpacity } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useRouter } from "expo-router";
import { Ionicons } from "@expo/vector-icons";
import GlovoIcon from "@/components/GlovoIcon";

export default function SignUpStartScreen() {
  const router = useRouter();

  return (
    <SafeAreaView className="flex-1 bg-white">
      <View className="flex-1 p-6 justify-between">

        <View className="mt-2 flex-row items-center">
          <TouchableOpacity
            onPress={() => router.replace("/(auth)")}
            className="p-2 -ml-2"
            activeOpacity={0.6}
          >
            <Ionicons name="arrow-back" size={24} color="#000000" />
          </TouchableOpacity>
        </View>

        <View className={"w-full flex-row justify-center"}>
          <GlovoIcon/>
        </View>
        <Text className={"text-3xl font-bold text-center"}>Sign up</Text>


        <View className="mb-6 items-center">
          <TouchableOpacity
            onPress={() => router.push("/(auth)/signup/email")}
            activeOpacity={0.8}
            className="w-full h-14 bg-yellow-400 rounded-full justify-center items-center mb-6"
          >
            <Text className="text-white text-base font-semibold">
              Start with email
            </Text>
          </TouchableOpacity>

          <Text className="text-sm text-gray-900 mb-1">
            Already have an account?
          </Text>
          <TouchableOpacity
            onPress={() => router.push("/(auth)/login")}
            activeOpacity={0.6}
          >
            <Text className="text-sm text-gray-900 font-bold underline">
              Sign in
            </Text>
          </TouchableOpacity>
        </View>

      </View>
    </SafeAreaView>
  );
}
