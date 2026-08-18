#!/bin/bash
# Builds the Android app. Requires the Android SDK, the .NET Android workload and JDK 17
# (newer JDKs are not supported by the Android build tooling).
#
# Usage: ./buildandroid.sh [target] [configuration]
#   target: Build (default), Install, or Run (installs then launches on the connected device/emulator)
#   configuration: Debug (default) or Release
#
# Override paths via env vars if yours differ from the defaults below, e.g.:
#   ANDROID_SDK_DIR=/path/to/Sdk JAVA_17_DIR=/path/to/jdk17 ./buildandroid.sh Install
#
# A Release build is signed with your own keystore - set these env vars first:
#   ANDROID_KEYSTORE_PATH, ANDROID_KEYSTORE_ALIAS, ANDROID_KEYSTORE_PASSWORD, ANDROID_KEY_PASSWORD
# Example:
#   ANDROID_KEYSTORE_PATH=/path/to/my.keystore ANDROID_KEYSTORE_ALIAS=myalias \
#   ANDROID_KEYSTORE_PASSWORD=xxx ANDROID_KEY_PASSWORD=xxx ./buildandroid.sh Build Release

set -e

ANDROID_SDK_DIR="${ANDROID_SDK_DIR:-~/Android/Sdk}"
JAVA_17_DIR="${JAVA_17_DIR:-/usr/lib/jvm/java-17-openjdk}"
TARGET="${1:-Build}"
CONFIGURATION="${2:-Debug}"
MSBUILD_TARGET="$TARGET"
[ "$TARGET" = "Run" ] && MSBUILD_TARGET="Install"

SIGNING_ARGS=()
if [ "$CONFIGURATION" = "Release" ]; then
    if [ -z "$ANDROID_KEYSTORE_PATH" ] || [ -z "$ANDROID_KEYSTORE_ALIAS" ] || [ -z "$ANDROID_KEYSTORE_PASSWORD" ] || [ -z "$ANDROID_KEY_PASSWORD" ]; then
        echo "Release build requires ANDROID_KEYSTORE_PATH, ANDROID_KEYSTORE_ALIAS, ANDROID_KEYSTORE_PASSWORD and ANDROID_KEY_PASSWORD to be set." >&2
        exit 1
    fi
    SIGNING_ARGS=(
        -p:AndroidKeyStore=true
        -p:AndroidSigningKeyStore="$ANDROID_KEYSTORE_PATH"
        -p:AndroidSigningKeyAlias="$ANDROID_KEYSTORE_ALIAS"
        -p:AndroidSigningStorePass="$ANDROID_KEYSTORE_PASSWORD"
        -p:AndroidSigningKeyPass="$ANDROID_KEY_PASSWORD"
    )
    # MSBuild's incremental "up-to-date" check for the sign step doesn't notice when only the
    # -p: signing properties above change, so a stale bin/obj can silently produce an APK that
    # LOOKS signed (file gets a "-Signed" name) but isn't. Always build Release from a clean state.
    rm -rf Desktop_Gremlin.Android/bin/Release Desktop_Gremlin.Android/obj/Release
fi

dotnet build Desktop_Gremlin.Android/Desktop_Gremlin.Android.csproj -c "$CONFIGURATION" -t:"$MSBUILD_TARGET" \
    -p:AndroidSdkDirectory="$ANDROID_SDK_DIR" \
    -p:JavaSdkDirectory="$JAVA_17_DIR" \
    "${SIGNING_ARGS[@]}"

if [ "$TARGET" = "Run" ]; then
    PACKAGE="com.desktopgremlin.app"
    ACTIVITY=$("$ANDROID_SDK_DIR/platform-tools/adb" shell cmd package resolve-activity --brief "$PACKAGE" | tail -1)
    "$ANDROID_SDK_DIR/platform-tools/adb" shell am start -n "$ACTIVITY"
fi
