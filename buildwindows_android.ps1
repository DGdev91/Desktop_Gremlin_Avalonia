# Builds the Android app. Requires the Android SDK, the .NET Android workload and JDK 17
# (newer JDKs are not supported by the Android build tooling).
#
# Usage: .\buildwindows_android.ps1 [-Target Build|Install|Run] [-Configuration Debug|Release]
#   Target: Build (default), Install, or Run (installs then launches on the connected device/emulator)
#   Configuration: Debug (default) or Release
#
# Override paths via env vars if the defaults below don't match your setup, e.g.:
#   $env:ANDROID_SDK_DIR = "C:\path\to\Sdk"; $env:JAVA_17_DIR = "C:\path\to\jdk17"
#
# A Release build is signed with your own keystore - set these env vars first:
#   ANDROID_KEYSTORE_PATH, ANDROID_KEYSTORE_ALIAS, ANDROID_KEYSTORE_PASSWORD, ANDROID_KEY_PASSWORD
# Example:
#   $env:ANDROID_KEYSTORE_PATH = "C:\path\to\my.keystore"; $env:ANDROID_KEYSTORE_ALIAS = "myalias"
#   $env:ANDROID_KEYSTORE_PASSWORD = "xxx"; $env:ANDROID_KEY_PASSWORD = "xxx"
#   .\buildwindows_android.ps1 -Target Build -Configuration Release

param(
    [string]$Target = "Build",
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$AndroidSdkDir = if ($env:ANDROID_SDK_DIR) { $env:ANDROID_SDK_DIR } else { "$env:LOCALAPPDATA\Android\Sdk" }
$Java17Dir = if ($env:JAVA_17_DIR) { $env:JAVA_17_DIR } else { $env:JAVA_HOME }

$MsBuildTarget = $Target
if ($Target -eq "Run") { $MsBuildTarget = "Install" }

$SigningArgs = @()
if ($Configuration -eq "Release") {
    if (-not $env:ANDROID_KEYSTORE_PATH -or -not $env:ANDROID_KEYSTORE_ALIAS -or -not $env:ANDROID_KEYSTORE_PASSWORD -or -not $env:ANDROID_KEY_PASSWORD) {
        Write-Error "Release build requires ANDROID_KEYSTORE_PATH, ANDROID_KEYSTORE_ALIAS, ANDROID_KEYSTORE_PASSWORD and ANDROID_KEY_PASSWORD to be set."
        exit 1
    }
    $SigningArgs = @(
        "-p:AndroidKeyStore=true",
        "-p:AndroidSigningKeyStore=$env:ANDROID_KEYSTORE_PATH",
        "-p:AndroidSigningKeyAlias=$env:ANDROID_KEYSTORE_ALIAS",
        "-p:AndroidSigningStorePass=$env:ANDROID_KEYSTORE_PASSWORD",
        "-p:AndroidSigningKeyPass=$env:ANDROID_KEY_PASSWORD"
    )
    # MSBuild's incremental "up-to-date" check for the sign step doesn't notice when only the
    # -p: signing properties above change, so a stale bin/obj can silently produce an APK that
    # LOOKS signed (file gets a "-Signed" name) but isn't. Always build Release from a clean state.
    Remove-Item -Recurse -Force "Desktop_Gremlin.Android\bin\Release" -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force "Desktop_Gremlin.Android\obj\Release" -ErrorAction SilentlyContinue
}

dotnet build Desktop_Gremlin.Android/Desktop_Gremlin.Android.csproj -c $Configuration -t:$MsBuildTarget `
    -p:AndroidSdkDirectory=$AndroidSdkDir `
    -p:JavaSdkDirectory=$Java17Dir `
    @SigningArgs

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ($Target -eq "Run") {
    $Package = "com.desktopgremlin.app"
    $Activity = & "$AndroidSdkDir\platform-tools\adb.exe" shell cmd package resolve-activity --brief $Package | Select-Object -Last 1
    & "$AndroidSdkDir\platform-tools\adb.exe" shell am start -n $Activity
}
