#!/usr/bin/env bash

set -e

echo "Updating version"

FILE=$(find . -maxdepth 3 -name "*Modedlus.csproj" | head -n 1)

echo "csproj file: $FILE"

# Extract the version line
VERSION_LINE=$(grep -m1 "<VersionPrefix>" "$FILE")
echo $VERSION_LINE
CURRENT_VERSION=$(echo "$VERSION_LINE" | sed -E 's/.*<VersionPrefix>([0-9]+\.[0-9]+\.[0-9]+)<\/VersionPrefix>.*/\1/')
echo $CURRENT_VERSION

echo "Curent version: $CURRENT_VERSION"

# Split into major, minor, patch
IFS='.' read -r MAJOR MINOR PATCH <<< "$CURRENT_VERSION"
NEW_PATCH=$((PATCH + 1))
NEW_VERSION="$MAJOR.$MINOR.$NEW_PATCH"

echo "New version: $NEW_VERSION"

# Replace in file
sed -i.bak -E "s/<VersionPrefix>[0-9]+\.[0-9]+\.[0-9]+<\/VersionPrefix>/<VersionPrefix>$NEW_VERSION<\/VersionPrefix>/" "$FILE"

echo "Updated version: $CURRENT_VERSION -> $NEW_VERSION"

echo ""

# Build
echo "Building"

BUILDLOC="./Builds/$NEW_VERSION/"
echo "build location: $BUILDLOC"

PLATFORM=${1:-linux-x64}

dotnet publish $FILE -c Release -o $BUILDLOC --self-contained true -r $PLATFORM


status=$?

if [ $status -eq 0 ]; then
    echo "Build succeeded"
else
    echo "Build Faild"
    echo "Reson: $status."
    exit $status
fi 

echo "Build"
echo ""

# Moves Libs
echo "move libs"

LIBVERSION="lib64"
LIBLOCFINAL="$BUILDLOC"
LIBLOC="$LIBLOCFINAL$LIBVERSION"

# list of libraries without extension
libs=("libFAudio" "libFNA3D" "libSDL2-2.0" "libtheorafile")

# possible extensions
exts=("so.0" "dll" "so")

for lib in "${libs[@]}"; do
    for ext in "${exts[@]}"; do
        src="$LIBLOC/$lib.$ext"
        if [ -f "$src" ]; then
            mv "$src" "$LIBLOCFINAL/$lib.$ext"
            echo "Moved $lib.$ext"
        fi
    done
done

# Final info
echo ""
echo "Build was succeesfull"
echo "Package at $BUILDLOC"

exit
