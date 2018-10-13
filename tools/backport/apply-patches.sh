#!/bin/bash

BACKPORT=$1

# Check if we are in the right directory
if [ ! -d "src" ]
then
    echo "The command must be run from the top level directory!"
fi

# Download the patches
wget -O backport.zip https://github.com/Kopernicus/Kopernicus-Backport/archive/ksp-$BACKPORT.zip
unzip backport.zip
mv Kopernicus-Backport-ksp-$BACKPORT ksp-$BACKPORT

# Apply the patches
find ksp-$BACKPORT -path "*.patch" | while read patchfile
do
    file=${patchfile#"ksp-$BACKPORT/"}
    file=${file%".patch"}
    patch -tuf $file $patchfile
done

# Which Kopernicus version is this based on?
source ksp-$BACKPORT/backport.sh

# Clean up the dependencies
rm build/GameData/ModuleManager.*.dll
rm -r build/GameData/ModularFlightIntegrator
rm -r build/GameData/Kopernicus/Shaders

# Download the referenced Kopernicus
wget -O kopernicus.zip https://github.com/Kopernicus/Kopernicus/releases/download/release-$VERSION/Kopernicus-$VERSION.zip
unzip kopernicus.zip -d ksp-$BACKPORT

# Move the new dependencies
cp ksp-$BACKPORT/GameData/ModuleManager.*.dll build/GameData/
cp -r ksp-$BACKPORT/GameData/ModularFlightIntegrator build/GameData/
cp -r ksp-$BACKPORT/GameData/Kopernicus/Shaders build/GameData/Kopernicus

# Cleanup
rm backport.zip
rm kopernicus.zip
rm -r ksp-$BACKPORT