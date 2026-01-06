#!/usr/bin/env bash

set -e

echo ""
echo "Updating version"
echo ""

# Source - https://stackoverflow.com/a
# Posted by dogbane, modified by community. See post 'Timeline' for change history
# Retrieved 2026-01-05, License - CC BY-SA 4.0

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

echo "Curnet LOC: $SCRIPT_DIR"
echo "Warning! if this does not mach the location, please file an issue with contex"
echo ""

if [ ! -f $SYMLINKLOC ]; then
    echo "Builds path not find, creating path at: $SCRIPT_DIR/Builds"
    echo ""
    mkdir "$SCRIPT_DIR/Builds"
fi

PACKFILE=$SCRIPT_DIR/BootLoader/BootLoader.csproj

echo "csproj file: $PACKFILE"

echo ""

# Build
echo "Building"

VERSIONLOC="$SCRIPT_DIR/Builds/Build"
BUILDLOC="$VERSIONLOC/WW2-FPS"
echo "build location: $BUILDLOC"
mkdir -p $BUILDLOC

rm -r $BUILDLOC/* || true

PLATFORM=${1:-linux-x64}

# dotnet publish $PACKFILE -c Release -o $BUILDLOC/ModedlusFiles -r $PLATFORM 
# dotnet pack $PACKFILE -o $BUILDLOC 
dotnet publish $PACKFILE -c Release -o $BUILDLOC --self-contained true -r $PLATFORM 

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

# Asets

echo ""
echo "Copying Resources"

cp -r "$SCRIPT_DIR/Resources" "$BUILDLOC/Resources"

echo ""

# symlink

echo "Config symlinkg SymLink"

SYMLINKLOC="$SCRIPT_DIR/Builds/Latest"

if [ -d $SYMLINKLOC ]; then
    echo "Symlink already created, removing previus one"
    rm $SYMLINKLOC
fi

echo "Creating Sym Link"
ln -s $BUILDLOC $SYMLINKLOC

# Runs The Program
echo ""
echo "Running Build"
echo ""

$BUILDLOC/BootLoader

exit
