import React, {useState} from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  TouchableWithoutFeedback, Keyboard
} from "react-native";
import {useForm, Controller} from "react-hook-form";
import {zodResolver} from "@hookform/resolvers/zod";
import * as z from "zod";

import {SafeAreaView} from "react-native-safe-area-context";
import {useRouter} from "expo-router";

const emailStepSchema = z.object({
  email: z.string().min(1, "The email is required").email("Incorrect email format"),
});

type EmailStepFormData = z.infer<typeof emailStepSchema>;

export default function EmailScreen() {
  const router = useRouter();

  const {
    control,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<EmailStepFormData>({
    resolver: zodResolver(emailStepSchema),
    defaultValues: {
      email: "",
    },
    mode: "onChange",
  });

  const onSubmit = (data: EmailStepFormData) => {
    // Переходимо до кроку OTP і передаємо email через параметри маршруту

    router.push({
      pathname: "/(auth)/signup/otp",
      params: { email: data.email }
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

              {/* Górny paseк z nawigacją (Spójny з LoginScreen) */}
              <View className="mt-4 flex-row items-center">
                <TouchableOpacity onPress={() => router.back()} className="p-2 -ml-2">
                  <Text className="text-xl font-bold text-gray-900">←</Text>
                </TouchableOpacity>
                <Text className="text-2xl font-bold text-gray-900 ml-4">Continue with your email</Text>
              </View>

              {/* Sekcja formularza (Wyśrodkowana) */}
              <View className="flex-1 justify-center my-6">

                {/* Поле Email */}
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
                          onChange(text.trimEnd());
                        }}
                        value={value}
                        autoFocus={true} // Автоматично відкриває клавіатуру для кращого UX
                      />
                    )}
                  />
                  {errors.email && (
                    <Text className="text-xs text-red-500 mt-1">{errors.email.message}</Text>
                  )}
                </View>

              </View>

              {/* Sekcja dolna (Przycisk akcji) */}
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
              </View>

            </View>
          </TouchableWithoutFeedback>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}