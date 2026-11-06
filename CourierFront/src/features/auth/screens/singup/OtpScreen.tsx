import {
  KeyboardAvoidingView,
  Platform,
  TextInput,
  TouchableOpacity,
  View,
  Text,
  TouchableWithoutFeedback,
  Keyboard,
  ScrollView,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useRef } from "react";
import { Controller, useForm } from "react-hook-form";
import * as z from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter, useLocalSearchParams } from "expo-router";

// Схема валідації ізольована для 6-значного OTP-коду
const otpStepSchema = z.object({
  otp: z.string().length(6, "The code must be exactly 6 digits long"),
});

type OtpStepFormData = z.infer<typeof otpStepSchema>;

export default function OtpStepScreen() {
  const router = useRouter();

  // Отримуємо email, переданий з попереднього кроку (EmailStep)
  const { email } = useLocalSearchParams<{ email: string }>();

  // Масив референсів для 6 інпутів
  const otpInputsRef = useRef<Array<TextInput | null>>([]);

  const {
    control,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<OtpStepFormData>({
    resolver: zodResolver(otpStepSchema),
    defaultValues: {
      otp: "",
    },
    mode: "onChange",
  });

  const onSubmit = (data: OtpStepFormData) => {
    // Переходимо до третього кроку (встановлення пароля)
    router.push({
      pathname: "/(auth)/signup/password",
      params: { email, otpCode: data.otp }
    });
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

              {/* Верхня панель навігації */}
              <View className="mt-4 flex-row items-center">
                <TouchableOpacity onPress={() => router.back()} className="p-2 -ml-2">
                  <Text className="text-xl font-bold text-gray-900">←</Text>
                </TouchableOpacity>
                <Text className="text-2xl font-bold text-gray-900 ml-4">Continue with your email</Text>
              </View>

              {/* Центральна секція форми */}
              <View className="flex-1 justify-center my-6">
                <Text className="text-sm text-gray-500 mb-3 pl-1">Code</Text>

                <Controller
                  control={control}
                  name="otp"
                  render={({ field: { onChange, value } }) => {
                    const otpArray = value.split("");
                    return (
                      <View>
                        {/* Рендеринг 6 окремих круглих інпутів */}
                        <View className="flex-row justify-between px-1 mb-2">
                          {Array.from({ length: 6 }).map((_, index) => (
                            <TextInput
                              key={index}
                              ref={(ref) => (otpInputsRef.current[index] = ref)}
                              className={`w-12 h-12 bg-gray-100 rounded-full text-center text-xl font-bold text-gray-900 border ${
                                errors.otp ? "border-red-500" : "border-transparent"
                              }`}
                              keyboardType="number-pad"
                              maxLength={1}
                              value={otpArray[index] || ""}
                              autoFocus={index === 0}
                              onChangeText={(text) => {
                                const newOtpArray = [...otpArray];
                                newOtpArray[index] = text;
                                const combinedOtp = newOtpArray.join("");
                                onChange(combinedOtp);

                                // Автоматичний фокус на наступне коло (до індексу 5 включно)
                                if (text && index < 5) {
                                  otpInputsRef.current[index + 1]?.focus();
                                }
                              }}
                              onKeyPress={({ nativeEvent }) => {
                                // Повернення на попередній інпут при натисканні Backspace
                                if (nativeEvent.key === "Backspace" && !otpArray[index] && index > 0) {
                                  const newOtpArray = [...otpArray];
                                  newOtpArray[index - 1] = "";
                                  onChange(newOtpArray.join(""));
                                  otpInputsRef.current[index - 1]?.focus();
                                }
                              }}
                            />
                          ))}
                        </View>

                        {errors.otp && (
                          <Text className="text-xs text-red-500 mt-1 pl-1">{errors.otp.message}</Text>
                        )}
                      </View>
                    );
                  }}
                />
              </View>

              {/* Нижня секція дій */}
              <View className="mb-4">
                <TouchableOpacity
                  onPress={handleSubmit(onSubmit)}
                  disabled={!isValid}
                  className={`w-full h-14 rounded-full justify-center items-center ${
                    isValid ? "bg-black active:opacity-90" : "bg-gray-300 opacity-60"
                  }`}
                >
                  <Text className="text-white text-base font-semibold">Continue</Text>
                </TouchableOpacity>

                {/* Блок повторного відправлення */}
                <View className="items-center mt-6">
                  <Text className="text-sm text-gray-400 mb-1">Wrong email?</Text>
                  <TouchableOpacity onPress={() => router.back()}>
                    <Text className="text-sm text-gray-900 font-semibold underline">
                      Send another one.
                    </Text>
                  </TouchableOpacity>
                </View>
              </View>

            </View>
          </TouchableWithoutFeedback>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}
