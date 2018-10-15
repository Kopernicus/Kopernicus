#!/bin/bash

BACKPORT=$1

# Check if we are in the right directory
if [ ! -d "src" ]
then
    echo "The command must be run from the top level directory!"
fi

# Download the patches
wget -O backport.zip https://github.com/Kopernicus/Kopernicus-Backport/archive/master.zip
unzip backport.zip
mv Kopernicus-Backport-master backport-master

# Which Kopernicus versions are we working with?
source version
temp=${VERSION//-/.}
numbers=(`echo $temp | sed 's/\./\n/g'`)
BASE="${numbers[0]}.${numbers[1]}.${numbers[2]}"
MFIBASE=$MFI

source backport-master/versions/ksp-$BACKPORT
temp=${VERSION//-/.}
numbers=(`echo $temp | sed 's/\./\n/g'`)
BACKPORT="${numbers[0]}.${numbers[1]}.${numbers[2]}"
MFIBACKPORT=$MFI

# Apply the patches
find backport-master/patches -path "*.patch" | while read patchfile
do
    sed -i "s/{FLAG}/$FLAG/g" $patchfile
    sed -i "s/{BACKPORT}/${numbers[0]}.${numbers[1]}.${numbers[2]}/g" $patchfile
    sed -i "s/{MFIBACKPORT}/$MFIBACKPORT/g" $patchfile
    sed -i "s/{MFIBASE}/$MFIBASE/g" $patchfile
    sed -i "s/{BASE}/$BASE/g" $patchfile

    file=${patchfile#"backport-master/patches/"}
    file=${file%".patch"}
    patch -tuf $file $patchfile
done

# Clean up the dependencies
rm build/GameData/ModuleManager.*.dll
rm -r build/GameData/ModularFlightIntegrator
rm -r build/GameData/Kopernicus/Shaders

# Download the referenced Kopernicus
wget -O kopernicus.zip https://github.com/Kopernicus/Kopernicus/releases/download/release-$VERSION/Kopernicus-$VERSION.zip
unzip kopernicus.zip -d backport-master

# Move the new dependencies
cp backport-master/GameData/ModuleManager.*.dll build/GameData/
cp -r backport-master/GameData/ModularFlightIntegrator build/GameData/
cp -r backport-master/GameData/Kopernicus/Shaders build/GameData/Kopernicus

# Cleanup
rm backport.zip
rm kopernicus.zip
rm -r backport-master