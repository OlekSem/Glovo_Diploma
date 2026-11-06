import React, { useState } from "react";
import { View, Text, TextInput, TouchableOpacity, KeyboardAvoidingView, Platform, ScrollView } from "react-native";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";

import {SafeAreaView} from "react-native-safe-area-context";
import {useRouter} from "expo-router";

// 1. Schematy walidacji dla poszczególnych kroków
const emailSchema = z.object({
  email: z.string().min(1, "Email jest wymagany").email("Niepoprawny format adresu email"),
});

const otpSchema = z.object({
  otp: z.string().length(6, "The code must be 6 numbers long"),
});

const passwordSchema = z.object({
  password: z.string()
    .min(8, "Minimum 8 znaków")
    .regex(/[A-Z]/, "Minimum jedna wielka litera")
    .regex(/[0-9]/, "Minimum jedna cyfra"),
});

export default function PasswordScreen() {
  const [step, setStep] = useState<"email" | "otp" | "password" | "success">("email");
  const [showPassword, setShowPassword] = useState(false);
  const router = useRouter();

  // Krok 1: Formularz Email
  const emailForm = useForm<{ email: string }>({
    resolver: zodResolver(emailSchema),
    defaultValues: { email: "" },
    mode: "onChange",
  });

  // Krok 2: Formularz OTP
  const otpForm = useForm<{ otp: string }>({
    resolver: zodResolver(otpSchema),
    defaultValues: { otp: "" },
    mode: "onChange",
  });

  // Krok 3: Formularz Hasła
  const passwordForm = useForm<{ password: string }>({
    resolver: zodResolver(passwordSchema),
    defaultValues: { password: "" },
    mode: "onChange",
  });

  // Obserwowanie hasła w locie do dynamicznej listy wymagań z makiety
  const watchedPassword = passwordForm.watch("password", "");
  const passwordRequirements = {
    length: watchedPassword.length >= 8,
    capital: /[A-Z]/.test(watchedPassword),
    number: /[0-9]/.test(watchedPassword),
  };

  const handleEmailSubmit = (data: { email: string }) => {
    console.log("Email step complete:", data);
    setStep("otp");
  };

  const handleOtpSubmit = (data: { otp: string }) => {
    console.log("OTP verification complete:", data);
    setStep("password");
  };

  const handlePasswordSubmit = (data: { password: string }) => {
    console.log("Registration complete! Saving password...", data);
    setStep("success");
  };

  // Obsługa powrotu (Strzałka w lewo)
  const handleBack = () => {
    if (step === "otp") setStep("email");
    else if (step === "password") setStep("otp");
    else router.back();
  };

  return (
    <SafeAreaView className="flex-1 bg-white">
      <KeyboardAvoidingView behavior={Platform.OS === "ios" ? "padding" : "height"} className="flex-1">
        <ScrollView contentContainerStyle={{ flexGrow: 1 }} className="px-6">

          {step !== "success" && (
            <View className="mt-4 flex-row items-center">
              <TouchableOpacity onPress={handleBack} className="p-2 -ml-2">
                <Text className="text-xl font-bold text-gray-900">←</Text>
              </TouchableOpacity>
              <Text className="text-2xl font-bold text-gray-900 ml-4">
                {step === "password" ? "Create password" : "Continue with your email"}
              </Text>
            </View>
          )}

          <View className="flex-1 justify-center mt-6">

            {step === "email" && (
              <View>
                <Text className="text-sm text-gray-500 mb-2">Your email</Text>
                <Controller
                  control={emailForm.control}
                  name="email"
                  render={({ field: { onChange, onBlur, value } }) => (
                    <TextInput
                      className="w-full h-14 bg-gray-100 rounded-xl px-4 text-base text-gray-900"
                      placeholder="name.surname@gmail.com"
                      placeholderTextColor="#A3A3A3"
                      autoCapitalize="none"
                      keyboardType="email-address"
                      onBlur={onBlur}
                      onChangeText={onChange}
                      value={value}
                    />
                  )}
                />
                <TouchableOpacity
                  onPress={emailForm.handleSubmit(handleEmailSubmit)}
                  disabled={!emailForm.formState.isValid}
                  className={`w-full h-14 rounded-full justify-center items-center mt-6 ${
                    emailForm.formState.isValid ? "bg-gray-400" : "bg-gray-300 opacity-60"
                  }`}
                >
                  <Text className="text-white text-base font-semibold">Continue</Text>
                </TouchableOpacity>
              </View>
            )}

            {/* KROK 2: KOD OTP (4 okręgi / pola tekstowe z makiety) */}
            {step === "otp" && (
              <View className="items-center">
                <Text className="text-sm text-gray-500 mb-4 text-center">
                  Enter the 4-digit code sent to your email
                </Text>
                <Controller
                  control={otpForm.control}
                  name="otp"
                  render={({ field: { onChange, value } }) => (
                    <TextInput
                      className="w-48 h-14 bg-gray-100 rounded-xl text-center text-2xl font-bold tracking-[20px] pl-5 text-gray-900"
                      placeholder="0000"
                      placeholderTextColor="#D4D4D4"
                      keyboardType="number-pad"
                      maxLength={4}
                      onChangeText={onChange}
                      value={value}
                    />
                  )}
                />
                <TouchableOpacity
                  onPress={otpForm.handleSubmit(handleOtpSubmit)}
                  disabled={!otpForm.formState.isValid}
                  className={`w-full h-14 rounded-full justify-center items-center mt-8 ${
                    otpForm.formState.isValid ? "bg-gray-400" : "bg-gray-300 opacity-60"
                  }`}
                >
                  <Text className="text-white text-base font-semibold">Verify code</Text>
                </TouchableOpacity>
                <TouchableOpacity className="mt-4">
                  <Text className="text-sm text-gray-500 underline">Send another one</Text>
                </TouchableOpacity>
              </View>
            )}

            {/* KROK 3: TWORZENIE HASŁA Z WALIDACJĄ W WYMAGAŃ */}
            {step === "password" && (
              <View>
                <Text className="text-sm text-gray-500 mb-2">Your password</Text>
                <Controller
                  control={passwordForm.control}
                  name="password"
                  render={({ field: { onChange, onBlur, value } }) => (
                    <View className="relative w-full mb-4">
                      <TextInput
                        className="w-full h-14 bg-gray-100 rounded-xl pl-4 pr-12 text-base text-gray-900"
                        placeholder="create password"
                        placeholderTextColor="#A3A3A3"
                        secureTextEntry={!showPassword}
                        autoCapitalize="none"
                        onBlur={onBlur}
                        onChangeText={onChange}
                        value={value}
                      />
                      <TouchableOpacity className="absolute right-4 top-4" onPress={() => setShowPassword(!showPassword)}>
                        <Text className="text-sm text-gray-400">{showPassword ? "👁️" : "🙈"}</Text>
                      </TouchableOpacity>
                    </View>
                  )}
                />

                {/* Lista dynamicznych wymagań z makiety */}
                <View className="space-y-2 mb-6 px-1">
                  <Text className={`text-sm ${passwordRequirements.length ? "text-green-600" : "text-gray-400"}`}>
                    {passwordRequirements.length ? "✓" : "○"} 8 characters minimum
                  </Text>
                  <Text className={`text-sm ${passwordRequirements.capital ? "text-green-600" : "text-gray-400"}`}>
                    {passwordRequirements.capital ? "✓" : "○"} a capital letter
                  </Text>
                  <Text className={`text-sm ${passwordRequirements.number ? "text-green-600" : "text-gray-400"}`}>
                    {passwordRequirements.number ? "✓" : "○"} a number
                  </Text>
                </View>

                <TouchableOpacity
                  onPress={passwordForm.handleSubmit(handlePasswordSubmit)}
                  disabled={!passwordForm.formState.isValid}
                  className={`w-full h-14 rounded-full justify-center items-center ${
                    passwordForm.formState.isValid ? "bg-gray-400" : "bg-gray-300 opacity-60"
                  }`}
                >
                  <Text className="text-white text-base font-semibold">Continue</Text>
                </TouchableOpacity>
              </View>
            )}

            {/* KROK 4: EKRAN SUKCESU */}
            {step === "success" && (
              <View className="items-center justify-center py-10">
                <View className="w-24 h-24 bg-gray-900 rounded-full items-center justify-center mb-6">
                  <Text className="text-white text-4xl">✓</Text>
                </View>
                <Text className="text-2xl font-bold text-gray-900 text-center mb-2">
                  Your account was successfully created
                </Text>
                <TouchableOpacity
                  onPress={() => navigation.navigate("/")}
                  className="w-full h-14 bg-gray-400 rounded-full justify-center items-center mt-8"
                >
                  <Text className="text-white text-base font-semibold">Let's go</Text>
                </TouchableOpacity>
              </View>
            )}

          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}