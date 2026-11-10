import React, { useRef, useState } from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  ActivityIndicator,
  Alert,
  TextInputProps,
} from "react-native";
import { useForm, Controller, Control } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { SafeAreaView } from "react-native-safe-area-context";
import { useRouter } from "expo-router";

import { useRegisterMutation } from "../../../../store/service/apiAccount";
import { getApiErrorMessage } from "../../../../utils/apiError";

const GLOVO_YELLOW = "#FFC244";
const GLOVO_GREEN = "#00A082";

export const signupSchema = z.object({
  firstName: z.string().trim().min(1, "First name is required"),
  lastName: z.string().trim().min(1, "Last name is required"),
  email: z.string().trim().email("Enter a valid email"),
  password: z
    .string()
    .min(8, "At least 8 characters")
    .regex(/[A-Z]/, "Add an uppercase letter")
    .regex(/[a-z]/, "Add a lowercase letter")
    .regex(/\d/, "Add a digit"),
});

export type SignupFormData = z.infer<typeof signupSchema>;

type FieldProps = {
  control: Control<SignupFormData>;
  name: keyof SignupFormData;
  label: string;
  error?: string;
  inputRef?: React.RefObject<TextInput | null>;
  onSubmitEditing?: () => void;
  rightElement?: React.ReactNode;
} & Omit<TextInputProps, "value" | "onChangeText" | "onBlur">;

function Field({
                 control,
                 name,
                 label,
                 error,
                 inputRef,
                 onSubmitEditing,
                 rightElement,
                 ...inputProps
               }: FieldProps) {
  const [focused, setFocused] = useState(false);

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
            {rightElement}
          </View>
        )}
      />
      {error && <Text className="text-xs text-red-500 mt-1">{error}</Text>}
    </View>
  );
}

export default function SignupScreen() {
  const router = useRouter();
  const [register, { isLoading }] = useRegisterMutation();
  const [showPassword, setShowPassword] = useState(false);

  const lastNameRef = useRef<TextInput>(null);
  const emailRef = useRef<TextInput>(null);
  const passwordRef = useRef<TextInput>(null);

  const {
    control,
    handleSubmit,
    setError,
    formState: { errors, isValid },
  } = useForm<SignupFormData>({
    resolver: zodResolver(signupSchema),
    defaultValues: { firstName: "", lastName: "", email: "", password: "" },
    mode: "onChange",
  });

  const onSubmit = async (data: SignupFormData) => {
    try {
      const res = await register({
        firstName: data.firstName.trim(),
        lastName: data.lastName.trim(),
        email: data.email.trim(),
        password: data.password,
      }).unwrap();

      router.push({
        pathname: "/(auth)/signup/otp",
        params: { email: data.email.trim() },
      });
    } catch (err) {
      const message = getApiErrorMessage(err);

      if (message.toLowerCase().includes("email")) {
        setError("email", { type: "server", message });
      } else if (message.toLowerCase().includes("password")) {
        setError("password", { type: "server", message });
      } else {
        Alert.alert("Registration failed", message);
      }
    }
  };

  const disabled = !isValid || isLoading;

  return (
    <SafeAreaView
      edges={["top"]}
      className="flex-1"
      style={{ backgroundColor: GLOVO_YELLOW }}
    >
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

            <Text className="text-3xl font-extrabold text-gray-900 mt-6">
              Become a client
            </Text>
            <Text className="text-base text-gray-800 mt-1">
              Create your account and start ordering with Glovo.
            </Text>
          </View>

          {/* White form card */}
          <View className="flex-1 bg-white rounded-t-[32px] px-5 pt-8 pb-6">
            <View className="flex-row gap-3">
              <View className="flex-1">
                <Field
                  control={control}
                  name="firstName"
                  label="First name"
                  placeholder="John"
                  error={errors.firstName?.message}
                  autoCapitalize="words"
                  textContentType="givenName"
                  returnKeyType="next"
                  onSubmitEditing={() => lastNameRef.current?.focus()}
                />
              </View>
              <View className="flex-1">
                <Field
                  control={control}
                  name="lastName"
                  label="Last name"
                  placeholder="Doe"
                  error={errors.lastName?.message}
                  inputRef={lastNameRef}
                  autoCapitalize="words"
                  textContentType="familyName"
                  returnKeyType="next"
                  onSubmitEditing={() => emailRef.current?.focus()}
                />
              </View>
            </View>

            <Field
              control={control}
              name="email"
              label="Email"
              placeholder="example@example.com"
              error={errors.email?.message}
              inputRef={emailRef}
              autoCapitalize="none"
              autoCorrect={false}
              keyboardType="email-address"
              textContentType="emailAddress"
              returnKeyType="next"
              onSubmitEditing={() => passwordRef.current?.focus()}
            />

            <Field
              control={control}
              name="password"
              label="Password"
              placeholder="At least 8 characters"
              error={errors.password?.message}
              inputRef={passwordRef}
              secureTextEntry={!showPassword}
              autoCapitalize="none"
              autoCorrect={false}
              textContentType="newPassword"
              returnKeyType="done"
              onSubmitEditing={handleSubmit(onSubmit)}
              rightElement={
                <TouchableOpacity
                  onPress={() => setShowPassword((v) => !v)}
                  hitSlop={10}
                >
                  <Text
                    className="text-sm font-semibold"
                    style={{ color: GLOVO_GREEN }}
                  >
                    {showPassword ? "Hide" : "Show"}
                  </Text>
                </TouchableOpacity>
              }
            />

            <Text className="text-xs text-gray-500 mb-6">
              Use 8+ characters with an uppercase letter, a lowercase letter and a digit.
            </Text>

            <View className="flex-1" />

            <TouchableOpacity
              onPress={handleSubmit(onSubmit)}
              disabled={disabled}
              activeOpacity={0.85}
              className="w-full h-14 rounded-full justify-center items-center"
              style={{
                backgroundColor: disabled ? "#E5E5E5" : GLOVO_YELLOW,
              }}
            >
              {isLoading ? (
                <ActivityIndicator color="#1A1A1A" />
              ) : (
                <Text
                  className={`text-base font-bold ${
                    disabled ? "text-gray-400" : "text-gray-900"
                  }`}
                >
                  Continue
                </Text>
              )}
            </TouchableOpacity>

            <TouchableOpacity
              onPress={() => router.replace("/(auth)/login")}
              className="mt-4 items-center"
            >
              <Text className="text-sm text-gray-600">
                Already have an account?{" "}
                <Text className="font-bold" style={{ color: GLOVO_GREEN }}>
                  Log in
                </Text>
              </Text>
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}