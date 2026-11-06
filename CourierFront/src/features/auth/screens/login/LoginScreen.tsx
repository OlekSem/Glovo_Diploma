import {
  KeyboardAvoidingView,
  Platform,
  TextInput,
  TouchableOpacity,
  View,
  Text,
  ActivityIndicator,
  TouchableWithoutFeedback,
  Keyboard,
  ScrollView,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "expo-router";
import { useLoginMutation } from "@/store/service/apiAccount";
import { saveSecureStore } from "@/utils/secureStore";
import {Ionicons} from "@expo/vector-icons";

const loginSchema = z.object({
  email: z.string().min(1, "The email is required").email("The email format is incorrect"),
  password: z.string().min(6, "The password should be at least 6 characters long"),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function LoginScreen() {
  const [showPassword, setShowPassword] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const router = useRouter();
  const [login, { isLoading }] = useLoginMutation();

  const {
    control,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
    mode: "onChange",
  });

  const onSubmit = async (data: LoginFormData) => {
    setApiError(null);
    try {
      const response = await login({
        email: data.email,
        password: data.password,
      }).unwrap();

      const { accessToken, refreshToken } = response.tokens;

      if (!accessToken || !refreshToken) {
        throw new Error("Authentication tokens were not returned.");
      }

      await saveSecureStore("accessToken", accessToken);
      await saveSecureStore("refreshToken", refreshToken);

      router.replace("/(tabs)");
    } catch (err: any) {
      const errorMessage =
        err?.data?.message ||
        err?.message ||
        "Oops! Email or password incorrect, try again.";

      setApiError(errorMessage);
    }
  };

  return (
    <SafeAreaView className="flex-1 bg-white">
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : "height"}
        className="flex-1"
      >
        <ScrollView
          contentContainerStyle={{ flexGrow: 1 }}
          bounces={false}
          keyboardShouldPersistTaps="handled"
        >
          <TouchableWithoutFeedback onPress={Keyboard.dismiss}>
            <View className="flex-1 p-4 justify-between">

              {/* Górny pasek z nawigacją */}
              <View className="mt-4 flex-row items-center">
                <TouchableOpacity onPress={() => router.push('/(auth)/loginStart')} className="p-2 -ml-2">
                  <Text className="text-xl font-bold text-gray-900">←</Text>
                </TouchableOpacity>
                <Text className="text-2xl font-bold text-gray-900 ml-4">Continue with your email</Text>
              </View>

              {/* Sekcja formularza (Wyśrodkowana) */}
              <View className="flex-1 justify-center my-6">

                {/* Pole Email */}
                <View className="mb-5">
                  <Text className="text-sm text-gray-500 mb-2">Your email</Text>
                  <Controller
                    control={control}
                    name="email"
                    render={({ field: { onChange, onBlur, value } }) => (
                      <TextInput
                        className={`w-full h-14 bg-gray-100 rounded-xl px-4 text-base text-gray-900 border ${
                          errors.email ? "border-red-500" : "border-transparent"
                        }`}
                        placeholder="example@example.com"
                        placeholderTextColor="#A3A3A3"
                        autoCapitalize="none"
                        keyboardType="email-address"
                        textContentType="emailAddress"
                        onBlur={onBlur}
                        onChangeText={(text) => {
                          onChange(text.trimEnd())
                        }}
                        value={value}
                      />
                    )}
                  />
                  {errors.email && (
                    <Text className="text-xs text-red-500 mt-1">{errors.email.message}</Text>
                  )}
                </View>

                {/* Pole Hasło */}
                <View className="mb-4">
                  <Text className="text-sm text-gray-500 mb-2">Your password</Text>
                  <Controller
                    control={control}
                    name="password"
                    render={({ field: { onChange, onBlur, value } }) => (
                      <View className="relative w-full justify-center">
                        <TextInput
                          className={`w-full h-14 bg-gray-100 rounded-xl pl-4 pr-12 text-base text-gray-900 border ${
                            errors.password ? "border-red-500" : "border-transparent"
                          }`}
                          placeholder="Password"
                          placeholderTextColor="#A3A3A3"
                          secureTextEntry={!showPassword}
                          autoCapitalize="none"
                          autoCorrect={false} // iOS: wyłącza autokorektę hasła
                          textContentType="password" // iOS: pozwala na współpracę z pękiem kluczy iCloud
                          onBlur={onBlur}
                          onChangeText={onChange}
                          value={value}
                        />
                        <TouchableOpacity
                          className="absolute right-4 h-14 justify-center items-center" // Wyśrodkowanie w pionie pasujące do h-14 inputa
                          onPress={() => setShowPassword(!showPassword)}
                          activeOpacity={0.6}
                        >
                          <Ionicons
                            name={showPassword ? "eye-outline" : "eye-off-outline"}
                            size={22}
                            color="#8E8E93"
                          />
                        </TouchableOpacity>
                      </View>
                    )}
                  />

                  {errors.password && (
                    <Text className="text-xs text-red-500 mt-1">{errors.password.message}</Text>
                  )}
                </View>

                {apiError && (
                  <View className="flex-row items-center mb-4 px-1">
                    <Text className="text-xs text-red-500 font-medium">⚠️ {apiError}</Text>
                  </View>
                )}
              </View>

              {/* Sekcja dolna (Przyciski akcji) */}
              <View className="mb-4">
                <TouchableOpacity
                  onPress={handleSubmit(onSubmit)}
                  disabled={!isValid || isLoading}
                  className={`w-full h-14 rounded-full justify-center items-center ${
                    isValid && !isLoading ? "bg-black active:opacity-90" : "bg-gray-300 opacity-60"
                  }`}
                >
                  {isLoading ? (
                    <ActivityIndicator color="#fff" />
                  ) : (
                    <Text className="text-white text-base font-semibold">Continue</Text>
                  )}
                </TouchableOpacity>

                <TouchableOpacity className="mt-6">
                  <Text className="text-sm text-gray-900 font-semibold text-center underline">
                    Forgot your password?
                  </Text>
                </TouchableOpacity>
              </View>

            </View>
          </TouchableWithoutFeedback>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}
