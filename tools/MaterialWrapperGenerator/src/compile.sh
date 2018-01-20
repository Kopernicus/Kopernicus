#!/bin/bash
JAVA_CLASSES="MaterialWrapperGenerator.class ShaderProperty.class"
JAVA_SOURCES="MaterialWrapperGenerator.java ShaderProperty.java"
MANIFEST_FILE=manifest.mf
JAR_FILE=../MaterialWrapperGenerator.jar

echo "Compiling $JAVA_SOURCES"
javac $JAVA_SOURCES
echo "Building $JAR_FILE"
jar cmf $MANIFEST_FILE $JAR_FILE $JAVA_CLASSES
echo "Cleaning Up"
rm -rf $JAVA_CLASSES
echo "Done"
