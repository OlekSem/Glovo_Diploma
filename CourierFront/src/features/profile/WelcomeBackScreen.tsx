import {SafeAreaView} from "react-native-safe-area-context";
import GlovoIcon from "@/components/GlovoIcon";
import {TouchableOpacity, Text} from "react-native";
import {useRouter} from "expo-router";

export default function WelcomeBackScreen() {
  const router = useRouter();

  return(
    <SafeAreaView>
      <GlovoIcon/>
      <TouchableOpacity
        onPress={() => router.push('/(tabs)')}>
        <Text>
          Let's go!
        </Text>
      </TouchableOpacity>
    </SafeAreaView>
  )
}