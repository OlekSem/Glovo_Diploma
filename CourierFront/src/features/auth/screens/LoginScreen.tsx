import {Button, Separators, TouchableOpacity, View} from "react-native";
import {Text} from "react-native";
import {SafeAreaProvider, SafeAreaView} from "react-native-safe-area-context";

export default function WelcomeScreen() {
  return(
    <SafeAreaProvider>

    <SafeAreaView>
      <Text style={{fontSize: 40, margin: "auto"}}>
        Welcome screen!
      </Text>
      <TouchableOpacity
        onPress={navigation.navigate('')}
      >

      </TouchableOpacity>
    </SafeAreaView>
    </SafeAreaProvider>
  )
}