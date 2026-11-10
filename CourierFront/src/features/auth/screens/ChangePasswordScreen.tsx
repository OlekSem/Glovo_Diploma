import React, { useRef, useState } from "react";
import {
  KeyboardAvoidingView,
  Platform,
  TextInput,
  TouchableOpacity,
  View,
  Text,
  ActivityIndicator,
  ScrollView,
  TextInputProps,
  Alert,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { Control, Controller, useForm } from "react-hook-form";
import * as z from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "expo-router";
import { Ionicons } from "@expo/vector-icons";

import { useChangePasswordMutation } from "@/store/service/apiAccount";
import { deleteSecureStore } from "@/utils/secureStore"; // adjust to your helper

type InputRef = React.ElementRef<typeof TextInput>;

const GLOVO_YELLOW = "#FFC244";
const GLOVO_GREEN = "#00A082";

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Enter your current password"),
    newPassword: z
      .string()
      .min(8, "At least 8 characters")
      .regex(/[A-Z]/, "Add an uppercase letter")
      .regex(/[a-z]/, "Add a lowercase letter")
      .regex(/\d/, "Add a digit"),
    confirmPassword: z.string().min(1, "Repeat the new password"),
  })
  .refine((d) => d.newPassword === d.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords don't match",
  })
  .refine((d) => d.newPassword !== d.currentPassword, {
    path: ["newPassword"],
    message: "New password must be different from the current one",
  });

type FormData = z.infer<typeof changePasswordSchema>;

type FieldProps = {
  control: Control<FormData>;
  name: keyof FormData;
  label: string;
  error?: string;
  inputRef?: React.Ref<InputRef>;
  onSubmitEditing?: () => void;
} & Omit<TextInputProps, "value" | "onChangeText" | "onBlur" | "secureTextEntry">;

function PasswordField({
                         control,
                         name,
                         label,
                         error,
                         inputRef,
                         onSubmitEditing,
                         ...inputProps
                       }: FieldProps) {
  const [focused, setFocused] = useState(false);
  const [visible, setVisible] = useState(false);

  const borderClass = error
    ? "border-red-500"
    : focused
      ? "border-[#00A082]"
      : "border-transparent";

  return (
    <View className="mb-4">
      <Text className="text-sm font-semibold text-gray-700 mb-2">{label}</Text>
      <Controller
        control={control}
        name={name}
        render={({ field: { onChange, onBlur, value } }) => (
          <View
            className={`flex-row items-center h-14 bg-gray-100 rounded-2xl px-4 border-2 ${borderClass}`}
          >
            <TextInput
              ref={inputRef}
              className="flex-1 text-base text-gray-900"
              placeholderTextColor="#A3A3A3"
              secureTextEntry={!visible}
              autoCapitalize="none"
              autoCorrect={false}
              value={value}
              onChangeText={onChange}
              onFocus={() => setFocused(true)}
              onBlur={() => {
                setFocused(false);
                onBlur();
              }}
              onSubmitEditing={onSubmitEditing}
              {...inputProps}
            />
            <TouchableOpacity onPress={() => setVisible((v) => !v)} hitSlop={10} activeOpacity={0.6}>
              <Ionicons
                name={visible ? "eye-outline" : "eye-off-outline"}
                size={22}
                color="#8E8E93"
              />
            </TouchableOpacity>
          </View>
        )}
      />
      {error && <Text className="text-xs text-red-500 mt-1">{error}</Text>}
    </View>
  );
}
type FieldKey = keyof FormData;

const FIELD_MAP: Record<string, FieldKey> = {
  currentpassword: "currentPassword",
  newpassword: "newPassword",
};

function getChangePasswordError(err: unknown): { field?: FieldKey; message: string } {
  const data = (err as { data?: any })?.data;

  // 1. ASP.NET model validation: { errors: { CurrentPassword: ["..."] } }
  if (data?.errors && !Array.isArray(data.errors) && typeof data.errors === "object") {
    for (const [key, messages] of Object.entries<any>(data.errors)) {
      const field = FIELD_MAP[key.toLowerCase()];
      const text = Array.isArray(messages) ? messages.filter(Boolean).join("\n") : String(messages);
      if (field && text) return { field, message: text };
    }
    return { message: data.title ?? "Validation failed." };
  }

  // 2. Identity errors: { message, errors: [{ code, description }] }
  if (Array.isArray(data?.errors) && data.errors.length) {
    const mismatch = data.errors.find((e: any) => e?.code === "PasswordMismatch");
    if (mismatch) {
      return { field: "currentPassword", message: "Current password is incorrect." };
    }
    return {
      field: "newPassword",
      message: data.errors.map((e: any) => e?.description).filter(Boolean).join("\n"),
    };
  }

  // 3. Plain { message }
  if (typeof data?.message === "string") return { message: data.message };

  return { message: "Network error. Please try again." };
}

export default function ChangePasswordScreen() {
  const router = useRouter();
  const [changePassword, { isLoading }] = useChangePasswordMutation();

  const newPasswordRef = useRef<InputRef>(null);
  const confirmRef = useRef<InputRef>(null);

  const {
    control,
    handleSubmit,
    setError,
    formState: { errors, isValid },
  } = useForm<FormData>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: { currentPassword: "", newPassword: "", confirmPassword: "" },
    mode: "onChange",
  });

  const onSubmit = async (data: FormData) => {
    try {
      await changePassword({
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
      }).unwrap();

      // The backend revoked the refresh token, so drop the local tokens too
      await deleteSecureStore("accessToken");
      await deleteSecureStore("refreshToken");

      Alert.alert("Password changed", "Please log in again with your new password.", [
        { text: "OK", onPress: () => router.replace("/(auth)/login") },
      ]);
    } catch (err) {
      const { field, message } = getChangePasswordError(err);
      if (field) {
        setError(field, { type: "server", message });
      } else {
        Alert.alert("Couldn't change password", message);
      }
    }
  };

  const disabled = !isValid || isLoading;

  return (
    <SafeAreaView edges={["top"]} className="flex-1" style={{ backgroundColor: GLOVO_YELLOW }}>
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : "height"}
        className="flex-1"
      >
        <ScrollView
          contentContainerStyle={{ flexGrow: 1 }}
          bounces={false}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          {/* Yellow header */}
          <View className="px-5 pt-2 pb-8">
            <TouchableOpacity
              onPress={() => router.back()}
              hitSlop={12}
              className="w-10 h-10 rounded-full bg-white/60 items-center justify-center"
            >
              <Text className="text-xl font-bold text-gray-900">←</Text>
            </TouchableOpacity>

            <Text className="text-3xl font-extrabold text-gray-900 mt-6">Change password</Text>
            <Text className="text-base text-gray-800 mt-1">
              You'll be logged out and need to sign in again.
            </Text>
          </View>

          {/* White form card */}
          <View className="flex-1 bg-white rounded-t-[32px] px-5 pt-8 pb-6">
            <PasswordField
              control={control}
              name="currentPassword"
              label="Current password"
              placeholder="Current password"
              error={errors.currentPassword?.message}
              textContentType="password"
              returnKeyType="next"
              autoFocus
              onSubmitEditing={() => newPasswordRef.current?.focus()}
            />

            <PasswordField
              control={control}
              name="newPassword"
              label="New password"
              placeholder="At least 8 characters"
              error={errors.newPassword?.message}
              inputRef={newPasswordRef}
              textContentType="newPassword"
              returnKeyType="next"
              onSubmitEditing={() => confirmRef.current?.focus()}
            />

            <PasswordField
              control={control}
              name="confirmPassword"
              label="Repeat new password"
              placeholder="Repeat new password"
              error={errors.confirmPassword?.message}
              inputRef={confirmRef}
              textContentType="newPassword"
              returnKeyType="done"
              onSubmitEditing={handleSubmit(onSubmit)}
            />

            <Text className="text-xs text-gray-500">
              Use 8+ characters with an uppercase letter, a lowercase letter and a digit.
            </Text>

            <View className="flex-1" />

            <TouchableOpacity
              onPress={handleSubmit(onSubmit)}
              disabled={disabled}
              activeOpacity={0.85}
              className="w-full h-14 rounded-full justify-center items-center mt-8"
              style={{ backgroundColor: disabled ? "#E5E5E5" : GLOVO_YELLOW }}
            >
              {isLoading ? (
                <ActivityIndicator color="#1A1A1A" />
              ) : (
                <Text
                  className={`text-base font-bold ${disabled ? "text-gray-400" : "text-gray-900"}`}
                >
                  Change password
                </Text>
              )}
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}