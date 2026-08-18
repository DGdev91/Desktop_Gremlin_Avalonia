#!/bin/bash
# Regenerates the Android launcher icon (adaptive icon foreground + legacy fallbacks) from the
# desktop app's tray icon, so both platforms keep the same branding. Requires ImageMagick.
# Re-run this any time Desktop_Gremlin/SpriteSheet/System/ico.ico changes.
set -e
cd "$(dirname "$0")"

SRC="../Desktop_Gremlin/SpriteSheet/System/ico.ico[5]" # 256x256 frame, has transparency
BG_COLOR="#5DBA3C"                                     # sampled from the icon's cap

TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
magick "$SRC" -trim +repage "$TMP/trimmed.png"

# Adaptive icon foreground (API 26+): subject centered, scaled to 66% of a 108dp canvas so it
# isn't clipped by circular/squircle launcher masks.
gen_foreground() {
    local size=$1 dir=$2
    mkdir -p "Resources/mipmap-$dir"
    local inner=$((size * 66 / 100))
    magick "$TMP/trimmed.png" -resize "${inner}x${inner}" -background none -gravity center \
        -extent "${size}x${size}" "Resources/mipmap-$dir/ic_launcher_foreground.png"
}
gen_foreground 108 mdpi
gen_foreground 162 hdpi
gen_foreground 216 xhdpi
gen_foreground 324 xxhdpi
gen_foreground 432 xxxhdpi

# Legacy (pre-API26) launcher icons: flattened background + subject, square and round variants.
gen_legacy() {
    local size=$1 dir=$2
    mkdir -p "Resources/mipmap-$dir"
    local inner=$((size * 80 / 100))
    magick -size "${size}x${size}" "xc:$BG_COLOR" \
        \( "$TMP/trimmed.png" -resize "${inner}x${inner}" \) -gravity center -compose over -composite \
        "Resources/mipmap-$dir/ic_launcher.png"
    magick "Resources/mipmap-$dir/ic_launcher.png" \
        \( -size "${size}x${size}" xc:none -fill white -draw "circle $((size / 2)),$((size / 2)) $((size / 2)),0" \) \
        -compose DstIn -composite "Resources/mipmap-$dir/ic_launcher_round.png"
}
gen_legacy 48 mdpi
gen_legacy 72 hdpi
gen_legacy 96 xhdpi
gen_legacy 144 xxhdpi
gen_legacy 192 xxxhdpi

echo "Icons regenerated under Resources/mipmap-*"
