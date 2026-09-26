#!/bin/bash
# KaçAtaKaç — iPhone simülatörü için derleme (Xcode 26).
# Önce Unity: python3 tools/kak_bridge.py invoke KakBuild SwitchToIos → refresh → invoke KakBuild BuildIosSimulator
# Sonra:      tools/kak_ios_sim.sh [--run ["iPhone 17 Pro"]]
#   Builds/iOS-Sim/Unity-iPhone.xcodeproj → imzasız simülatör derlemesi (Apple hesabı gerekmez) → .app yolu
#   --run: simülatörü açar, uygulamayı kurar ve başlatır (xcrun simctl)
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJ="$ROOT/Builds/iOS-Sim"
DERIVED="$PROJ/DerivedData"
LOG="$PROJ/xcodebuild.log"

[ -d "$PROJ/Unity-iPhone.xcodeproj" ] || { echo "HATA: $PROJ/Unity-iPhone.xcodeproj yok (önce Unity'de BuildIosSimulator)"; exit 1; }

echo "xcodebuild (simülatör, imzasız) → $LOG"
if ! xcodebuild -project "$PROJ/Unity-iPhone.xcodeproj" -scheme Unity-iPhone -configuration Release \
      -sdk iphonesimulator -destination "generic/platform=iOS Simulator" -derivedDataPath "$DERIVED" \
      CODE_SIGNING_ALLOWED=NO build > "$LOG" 2>&1; then
    grep -E "error:|BUILD FAILED" "$LOG" | head -20
    exit 1
fi
grep -E "BUILD SUCCEEDED" "$LOG" | tail -1

APP="$(find "$DERIVED/Build/Products/Release-iphonesimulator" -maxdepth 1 -name "*.app" | head -1)"
echo "APP=$APP"

if [ "${1:-}" = "--run" ]; then
    DEVICE="${2:-iPhone 17 Pro}"
    xcrun simctl boot "$DEVICE" 2>/dev/null || true
    open -a Simulator
    xcrun simctl install "$DEVICE" "$APP"
    xcrun simctl launch "$DEVICE" com.atakaan.kacatakac
fi
