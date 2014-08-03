#!/bin/bash
JAVA_CLASS=MaterialWrapperGenerator
MANIFEST_FILE=manifest.mf
JAR_FILE=../MaterialWrapperGenerator.jar

echo "Compiling $JAVA_FILE"
javac $JAVA_CLASS.java
echo "Building $JAR_FILE"
jar cmf $MANIFEST_FILE $JAR_FILE $JAVA_CLASS.class
echo "Cleaning Up"
rm -rf $JAVA_CLASS.class
echo "Done"