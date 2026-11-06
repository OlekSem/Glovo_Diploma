import React, { useEffect, useRef, useState } from "react";
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ActivityIndicator,
  Animated,
  Easing,
  Alert,
} from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useLocalSearchParams, useRouter } from "expo-router";

import {
  useConfirmEmailMutation,
  useResendConfirmationEmailMutation,
} from "@/store/service/apiAccount";
import { getApiErrorMessage } from "@/utils/apiError";

type InputRef = React.ElementRef<typeof TextInput>;

const GLOVO_YELLOW = "#FFC244";
const GLOVO_GREEN = "#00A082";
const CODE_LENGTH = 6;
const RESEND_SECONDS = 60;

export default function OtpScreen() {
  const router = useRouter();
  const { email } = useLocalSearchParams<{ email: string }>();

  const [confirmEmail, { isLoading }] = useConfirmEmailMutation();
  const [resendCode, { isLoading: isResending }] = useResendConfirmationEmailMutation();

  const inputRef = useRef<InputRef>(null);
  const shake = useRef(new Animated.Value(0)).current;

  const [code, setCode] = useState("");
  const [focused, setFocused] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [cooldown, setCooldown] = useState(RESEND_SECONDS);

  // Resend countdown
  useEffect(() => {
    if (cooldown <= 0) return;
    const t = setTimeout(() => setCooldown((s) => s - 1), 1000);
    return () => clearTimeout(t);
  }, [cooldown]);

  const runShake = () => {
    shake.setValue(0);
    Animated.sequence(
      [10, -10, 8, -8, 4, 0].map((toValue) =>
        Animated.timing(shake, {
          toValue,
          duration: 50,
          easing: Easing.linear,
          useNativeDriver: true,
        })
      )
    ).start();
  };

  const verify = async (value: string) => {
    if (isLoading) return;
    try {
      await confirmEmail({ email: String(email), code: value }).unwrap();
      router.replace("/(auth)/login");
    } catch (err) {
      setError(getApiErrorMessage(err));
      runShake();
      setCode("");
      // blur + delayed focus forces the keyboard to reopen reliably
      inputRef.current?.blur();
      setTimeout(() => inputRef.current?.focus(), 100);
    }
  };

  const onChangeCode = (text: string) => {
    const digits = text.replace(/\D/g, "").slice(0, CODE_LENGTH);
    setCode(digits);
    if (error) setError(null);
    if (digits.length === CODE_LENGTH) verify(digits);
  };

  const onResend = async () => {
    if (cooldown > 0 || isResending) return;
    try {
      await resendCode({ email: String(email) }).unwrap();
      setCode("");
      setError(null);
      setCooldown(RESEND_SECONDS);
      inputRef.current?.blur();
      setTimeout(() => inputRef.current?.focus(), 100);
    } catch (err) {
      Alert.alert("Couldn't resend code", getApiErrorMessage(err));
    }
  };

  const activeIndex = Math.min(code.length, CODE_LENGTH - 1);

  return (
    <SafeAreaView edges={["top"]} className="flex-1" style={{ backgroundColor: GLOVO_YELLOW }}>
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : "height"}
        className="flex-1"
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

          <Text className="text-3xl font-extrabold text-gray-900 mt-6">Check your email</Text>
          <Text className="text-base text-gray-800 mt-1">
            We sent a 6-digit code to{"\n"}
            <Text className="font-bold">{email}</Text>
          </Text>
        </View>

        {/* White card */}
        <View className="flex-1 bg-white rounded-t-[32px] px-5 pt-10 pb-6">
          <Animated.View style={{ transform: [{ translateX: shake }] }}>
            <Pressable onPress={() => inputRef.current?.focus()}>
              <View className="flex-row justify-between">
                {Array.from({ length: CODE_LENGTH }).map((_, i) => {
                  const digit = code[i] ?? "";
                  const isActive = focused && i === activeIndex && !isLoading;

                  const borderColor = error
                    ? "#EF4444"
                    : isActive
                      ? GLOVO_GREEN
                      : digit
                        ? "#D4D4D4"
                        : "transparent";

                  return (
                    <View
                      key={i}
                      className="h-16 rounded-2xl bg-gray-100 items-center justify-center"
                      style={{
                        width: "15%",
                        borderWidth: 2,
                        borderColor,
                        backgroundColor: isActive ? "#F0FBF8" : "#F3F4F6",
                      }}
                    >
                      <Text className="text-2xl font-bold text-gray-900">{digit}</Text>
                    </View>
                  );
                })}
              </View>
            </Pressable>

            {/* Hidden input that receives all the typing / paste / autofill */}
            <TextInput
              ref={inputRef}
              value={code}
              onChangeText={onChangeCode}
              onFocus={() => setFocused(true)}
              onBlur={() => setFocused(false)}
              keyboardType="number-pad"
              textContentType="oneTimeCode"
              autoComplete="sms-otp"
              maxLength={CODE_LENGTH}
              autoFocus
              caretHidden
              style={{ position: "absolute", opacity: 0, width: 1, height: 1 }}
            />
          </Animated.View>

          <View className="h-6 mt-4 items-center justify-center">
            {isLoading ? (
              <ActivityIndicator color={GLOVO_GREEN} />
            ) : error ? (
              <Text className="text-sm text-red-500 text-center">{error}</Text>
            ) : null}
          </View>

          <View className="flex-1" />

          {/* Resend */}
          <View className="items-center mb-2">
            <Text className="text-sm text-gray-500">Didn't get the code?</Text>
            <TouchableOpacity
              onPress={onResend}
              disabled={cooldown > 0 || isResending}
              hitSlop={10}
              className="mt-1"
            >
              {isResending ? (
                <ActivityIndicator color={GLOVO_GREEN} />
              ) : cooldown > 0 ? (
                <Text className="text-sm font-semibold text-gray-400">
                  Resend in 0:{String(cooldown).padStart(2, "0")}
                </Text>
              ) : (
                <Text className="text-sm font-bold" style={{ color: GLOVO_GREEN }}>
                  Resend code
                </Text>
              )}
            </TouchableOpacity>
          </View>

          {/* Manual confirm, for when the code is typed but auto-submit failed */}
          <TouchableOpacity
            onPress={() => verify(code)}
            disabled={code.length < CODE_LENGTH || isLoading}
            activeOpacity={0.85}
            className="w-full h-14 rounded-full justify-center items-center mt-4"
            style={{
              backgroundColor: code.length < CODE_LENGTH || isLoading ? "#E5E5E5" : GLOVO_YELLOW,
            }}
          >
            <Text
              className={`text-base font-bold ${
                code.length < CODE_LENGTH || isLoading ? "text-gray-400" : "text-gray-900"
              }`}
            >
              Verify
            </Text>
          </TouchableOpacity>
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}