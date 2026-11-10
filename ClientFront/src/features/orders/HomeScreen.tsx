import {SafeAreaView} from "react-native-safe-area-context";
import {View, Text} from "react-native";

export default function HomeScreen() {
  return(
    <SafeAreaView>
      <View>
        <Text className={"text-center text-4xl font-bold"}>Orders</Text>
      </View>
      <View className={"h-full flex justify-center"}>
        <Text className={"text-center"}>There are no orders right now</Text>
      </View>
    </SafeAreaView>
  )
}