#!/usr/bin/env bash

set -e

echo "Updating version"

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
cd $SCRIPT_DIR

FILE=$(find . -maxdepth 1 -name "*.csproj" | head -n 1)

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

#build
echo "Building"

MODEDLUSDEVLOC="./../../Resources/Packages/"
#"$HOME/.Modedlus/Core/"
BUILDLOC="$MODEDLUSDEVLOC"
echo "build location: $BUILDLOC"

dotnet publish -c Release -o $BUILDLOC
status=$?

if [ $status -eq 0 ]; then
    echo "Build succeeded"
else
    echo "Build Faild"
    echo "Reson: $status."
    exit $status
fi 

echo "Build"

#final info
echo ""
echo "Build was succeesfull"
echo "Package at $BUILDLOC"
exit
