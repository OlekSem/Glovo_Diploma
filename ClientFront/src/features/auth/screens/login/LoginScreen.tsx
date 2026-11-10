import React, {useRef, useState} from "react";
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
} from "react-native";
import {SafeAreaView} from "react-native-safe-area-context";
import {Control, Controller, useForm} from "react-hook-form";
import * as z from "zod";
import {zodResolver} from "@hookform/resolvers/zod";
import {useRouter} from "expo-router";
import {Ionicons} from "@expo/vector-icons";

import {useLoginMutation, useResendConfirmationEmailMutation} from "../../../../store/service/apiAccount";
import {saveSecureStore} from "../../../../utils/secureStore";
import {getApiErrorMessage} from "../../../../utils/apiError";

type InputRef = React.ElementRef<typeof TextInput>;

const GLOVO_YELLOW = "#FFC244";
const GLOVO_GREEN = "#00A082";

const loginSchema = z.object({
  email: z.string().min(1, "The email is required").email("The email format is incorrect"),
  password: z.string().min(6, "The password should be at least 6 characters long"),
});

type LoginFormData = z.infer<typeof loginSchema>;

type FieldProps = {
  control: Control<LoginFormData>;
  name: keyof LoginFormData;
  label: string;
  error?: string;
  inputRef?: React.Ref<InputRef>;
  onSubmitEditing?: () => void;
  rightElement?: React.ReactNode;
  transform?: (text: string) => string;
} & Omit<TextInputProps, "value" | "onChangeText" | "onBlur">;

function Field({
                 control,
                 name,
                 label,
                 error,
                 inputRef,
                 onSubmitEditing,
                 rightElement,
                 transform,
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
        render={({field: {onChange, onBlur, value}}) => (
          <View
            className={`flex-row items-center h-14 bg-gray-100 rounded-2xl px-4 border-2 ${borderClass}`}
          >
            <TextInput
              ref={inputRef}
              className="flex-1 text-base text-gray-900"
              placeholderTextColor="#A3A3A3"
              value={value}
              onChangeText={(t) => onChange(transform ? transform(t) : t)}
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

export default function LoginScreen() {
  const [showPassword, setShowPassword] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const router = useRouter();
  const passwordRef = useRef<InputRef>(null);

  const [login, {isLoading}] = useLoginMutation();
  const [resendCode, {isLoading: isSendingCode}] = useResendConfirmationEmailMutation();

  const {
    control,
    handleSubmit,
    getValues,
    formState: {errors, isValid},
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {email: "", password: ""},
    mode: "onChange",
  });

  const onSubmit = async (data: LoginFormData) => {
    setApiError(null);
    try {
      const response = await login({
        email: data.email,
        password: data.password,
      }).unwrap();

      const {accessToken, refreshToken} = response.tokens;

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

  const onConfirmEmail = async () => {
    const email = getValues("email").trim();
    if (!email) {
      setApiError("Enter your email first.");
      return;
    }

    setApiError(null);
    try {
      await resendCode({email}).unwrap();
      router.push({pathname: "/(auth)/signup/otp", params: {email}});
    } catch (err) {
      setApiError(getApiErrorMessage(err));
    }
  };

  const disabled = !isValid || isLoading;

  return (
    <SafeAreaView
      edges={["top"]}
      className="flex-1"
      style={{backgroundColor: GLOVO_YELLOW}}
    >
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : "height"}
        className="flex-1"
      >
        <ScrollView
          contentContainerStyle={{flexGrow: 1}}
          bounces={false}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          {/* Yellow header */}
          <View className="px-5 pt-2 pb-8">
            <TouchableOpacity
              onPress={() => router.push("/(auth)/loginStart")}
              hitSlop={12}
              className="w-10 h-10 rounded-full bg-white/60 items-center justify-center"
            >
              <Text className="text-xl font-bold text-gray-900">←</Text>
            </TouchableOpacity>

            <Text className="text-3xl font-extrabold text-gray-900 mt-6">
              Welcome back
            </Text>
            <Text className="text-base text-gray-800 mt-1">
              Log in to continue ordering with Glovo.
            </Text>
          </View>

          {/* White form card */}
          <View className="flex-1 bg-white rounded-t-[32px] px-5 pt-8 pb-6">
            <Field
              control={control}
              name="email"
              label="Email"
              placeholder="example@example.com"
              error={errors.email?.message}
              autoCapitalize="none"
              autoCorrect={false}
              keyboardType="email-address"
              textContentType="emailAddress"
              returnKeyType="next"
              transform={(t) => t.trimEnd()}
              onSubmitEditing={() => passwordRef.current?.focus()}
            />

            <Field
              control={control}
              name="password"
              label="Password"
              placeholder="Password"
              error={errors.password?.message}
              inputRef={passwordRef}
              secureTextEntry={!showPassword}
              autoCapitalize="none"
              autoCorrect={false}
              textContentType="password"
              returnKeyType="done"
              onSubmitEditing={handleSubmit(onSubmit)}
              rightElement={
                <TouchableOpacity
                  onPress={() => setShowPassword((v) => !v)}
                  hitSlop={10}
                  activeOpacity={0.6}
                >
                  <Ionicons
                    name={showPassword ? "eye-outline" : "eye-off-outline"}
                    size={22}
                    color="#8E8E93"
                  />
                </TouchableOpacity>
              }
            />

            {apiError && (
              <View className="flex-row items-center mb-2 px-1">
                <Text className="text-xs text-red-500 font-medium">⚠️ {apiError}</Text>
              </View>
            )}
            <View className="flex-row justify-between mt-1">
              <TouchableOpacity
                onPress={onConfirmEmail}
                disabled={isSendingCode}
                hitSlop={10}
              >
                {isSendingCode ? (
                  <ActivityIndicator color={GLOVO_GREEN}/>
                ) : (
                  <Text className="text-sm font-bold" style={{color: GLOVO_GREEN}}>
                    Confirm my email
                  </Text>
                )}
              </TouchableOpacity>

              <TouchableOpacity
                onPress={() => router.push("/(auth)/forgot-password")}
                hitSlop={10}
              >
                <Text className="text-sm font-bold" style={{color: GLOVO_GREEN}}>
                  Forgot your password?
                </Text>
              </TouchableOpacity>
            </View>

            <View className="flex-1"/>
            <TouchableOpacity
              onPress={handleSubmit(onSubmit)}
              disabled={disabled}
              activeOpacity={0.85}
              className="w-full h-14 rounded-full justify-center items-center mt-8"
              style={{backgroundColor: disabled ? "#E5E5E5" : GLOVO_YELLOW}}
            >
              {isLoading ? (
                <ActivityIndicator color="#1A1A1A"/>
              ) : (
                <Text
                  className={`text-base font-bold ${
                    disabled ? "text-gray-400" : "text-gray-900"
                  }`}
                >
                  Log in
                </Text>
              )}
            </TouchableOpacity>

            <TouchableOpacity
              onPress={() => router.replace("/(auth)/signup")}
              className="mt-4 items-center"
            >
              <Text className="text-sm text-gray-600">
                New to Glovo?{" "}
                <Text className="font-bold" style={{color: GLOVO_GREEN}}>
                  Sign up
                </Text>
              </Text>
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}