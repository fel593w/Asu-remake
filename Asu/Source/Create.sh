#!/bin/bash

echo "Creating new project"

NAME=$1
VERSION="0.0.1"

echo $NAME
echo $VERSION

cp ./PluginExampel ./$NAME -r
mv ./$NAME/PluginExampel.csproj ./$NAME/$NAME.csproj

exit