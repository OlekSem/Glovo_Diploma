// components/GlovoIcon.tsx

import React from "react";
import { View, Image, useWindowDimensions } from "react-native";

interface GlovoIconProps {
  size?: number;
}

export default function GlovoIcon({ size }: GlovoIconProps) {
  const { width } = useWindowDimensions();

  const iconSize = size ?? width * 0.25;

  return (
    <View
      className="items-center justify-center rounded-3xl bg-[#FFC244] overflow-hidden"
      style={{
        width: iconSize,
        height: iconSize,
      }}
    >
      <Image
        source={require("@/../assets/icon.png")}
        resizeMode="contain"
        className="w-[75%] h-[75%]"
      />
    </View>
  );
}