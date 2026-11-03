import React from "react";
import { Image, TouchableOpacity, View, Text } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import { AuthStackParamList } from "../../../../App";

type Props = NativeStackScreenProps<AuthStackParamList, 'Welcome'>;

export default function WelcomeScreen({ navigation }: Props) {
  return (
    <SafeAreaView className="flex-1 bg-white justify-between px-6">

      {/* Brand Container */}
      <View className="flex-1 justify-center items-center mt-16">
        <Image
          className="w-32 h-32 rounded-full mb-6"
          source={require("../../../../assets/icon.png")}
        />
        <Text className="text-3xl font-bold text-[#1A1A1A] mb-2 text-center">
          Welcome screen!
        </Text>
      </View>

      {/* Button Container */}
      <View className="w-full mb-10">

        {/* Log In Button */}
        <TouchableOpacity
          className="w-full h-14 bg-[#007AFF] rounded-xl justify-center items-center mb-3 active:opacity-80"
          onPress={() => navigation.navigate('Login')}
        >
          <Text className="text-white text-base font-semibold">
            Log in!
          </Text>
        </TouchableOpacity>

        {/* Sign Up Button */}
        <TouchableOpacity
          className="w-full h-14 bg-[#F2F2F7] border border-[#E5E5EA] rounded-xl justify-center items-center active:opacity-70"
          onPress={() => navigation.navigate('SignUp')}
        >
          <Text className="text-[#1A1A1A] text-base font-semibold">
            Sign up!
          </Text>
        </TouchableOpacity>

      </View>

    </SafeAreaView>
  );
}
