import {
  Animated,
  Button,
  KeyboardAvoidingView,
  Platform,
  Separators,
  TextInput,
  TouchableOpacity,
  View
} from "react-native";
import {Text} from "react-native";
import {SafeAreaProvider, SafeAreaView} from "react-native-safe-area-context";
import {NativeStackScreenProps} from "@react-navigation/native-stack";
import {AuthStackParamList} from "../../../../App";
import {useState} from "react";
import {Controller, useForm} from "react-hook-form";

import * as z from "zod";
import {zodResolver} from "@hookform/resolvers/zod";
import ScrollView = Animated.ScrollView;

type Props = NativeStackScreenProps<AuthStackParamList, "Login">;

const loginSchema = z.object({
  email: z.string().min(1, "Email jest wymagany").email("Niepoprawny format adresu email"),
  password: z.string().min(6, "Hasło musi mieć co najmniej 6 znaków"),
});


export default function LoginScreen({navigation}: Props) {
  const [showPassword, setShowPassword] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  type LoginFormData = z.infer<typeof loginSchema>;

  const onSubmit = (data: LoginFormData) => {
    setApiError(null);
    console.log("Dane logowania:", data);

    // Symulacja błędu API z designu (błędny email/hasło)
    if (data.email !== "zlata.machek@gmail.com") {
      setApiError("Oops! Email or password incorrect, try another one");
    } else {
      // Sukces -> Przejście dalej (np. Welcome Back / Dashboard)
      // navigation.navigate('Home');
    }
  };
  const {
    control,
    handleSubmit,
    formState: {errors, isValid},
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
    },
    mode: "onChange",
  });

  return (
    <SafeAreaView className="flex-1 bg-white">
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : "height"}
        className="flex-1"
      >
        <ScrollView contentContainerStyle={{flexGrow: 1}} className="px-6">

          {/* Przycisk Wstecz i Nagłówek */}
          <View className="mt-4 flex-row items-center">
            <TouchableOpacity onPress={() => navigation.goBack()} className="p-2 -ml-2">
              <Text className="text-xl font-bold text-gray-900">←</Text>
            </TouchableOpacity>
            <Text className="text-2xl font-bold text-gray-900 ml-4">Continue with your email</Text>
          </View>

          {/* Formularz */}
          <View className="flex-1 justify-center mt-10">

            {/* Pole Email */}
            <View className="mb-5">
              <Text className="text-sm text-gray-500 mb-2">Your email</Text>
              <Controller
                control={control}
                name="email"
                render={({field: {onChange, onBlur, value}}) => (
                  <TextInput
                    className={`w-full h-14 bg-gray-100 rounded-xl px-4 text-base text-gray-900 border ${
                      errors.email ? "border-red-500" : "border-transparent"
                    }`}
                    placeholder="example@example.com"
                    placeholderTextColor="#A3A3A3"
                    autoCapitalize="none"
                    keyboardType="email-address"
                    onBlur={onBlur}
                    onChangeText={onChange}
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
                render={({field: {onChange, onBlur, value}}) => (
                  <View className="relative w-full">
                    <TextInput
                      className={`w-full h-14 bg-gray-100 rounded-xl pl-4 pr-12 text-base text-gray-900 border ${
                        errors.password ? "border-red-500" : "border-transparent"
                      }`}
                      placeholder="password"
                      placeholderTextColor="#A3A3A3"
                      secureTextEntry={!showPassword}
                      autoCapitalize="none"
                      onBlur={onBlur}
                      onChangeText={onChange}
                      value={value}
                    />
                    {/* Przycisk Pokaż/Ukryj Hasło (Ikona oka z designu) */}
                    <TouchableOpacity
                      className="absolute right-4 top-4"
                      onPress={() => setShowPassword(!showPassword)}
                    >
                      <Text className="text-sm text-gray-400">{showPassword ? "👁️" : "🙈"}</Text>
                    </TouchableOpacity>
                  </View>
                )}
              />
              {errors.password && (
                <Text className="text-xs text-red-500 mt-1">{errors.password.message}</Text>
              )}
            </View>

            {/* Globalny błąd API (Czerwony komunikat z makiety) */}
            {apiError && (
              <View className="flex-row items-center mb-4 px-1">
                <Text className="text-xs text-red-500 font-medium">⚠️ {apiError}</Text>
              </View>
            )}

            {/* Przycisk Continue */}
            <TouchableOpacity
              onPress={handleSubmit(onSubmit)}
              disabled={!isValid}
              className={`w-full h-14 rounded-full justify-center items-center mt-4 ${
                isValid ? "bg-gray-400 active:opacity-90" : "bg-gray-300 opacity-60"
              }`}
            >
              <Text className="text-white text-base font-semibold">Continue</Text>
            </TouchableOpacity>

            {/* Zapomniałeś hasła? */}
            <TouchableOpacity className="align-center mt-6">
              <Text className="text-sm text-gray-900 font-semibold text-center underline">
                Forgot your password?
              </Text>
            </TouchableOpacity>

          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  )
}