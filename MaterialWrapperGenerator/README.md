MaterialWrapperGenerator 
========================

Copyright (c) 2014 Nathaniel R. Lewis

MaterialWrapperGenerator is a tool to generate a material wrapper object in C# for Unity .shader files extracted via UnityAssetExplorer.  This is a command line program written in Java, which requires a JRE (Java Runtime Environment) or a JDK (Java Development Kit) to be installed on the system.

The jar file is provided, and can be executed as such:

java -jar MaterialWrapperGenerator.jar <path to .shader file>  <namespace of resulting cs file>


To compile this program yourself, you will need the JDK.  With a terminal, navigate to the src/ directory.  Run the compile script (./compile.sh).  This will generate and overwrite the included .jar file.

MaterialWrapperGenerator is distributed under the same LGPL license included in Kopernicus/.

Here is a fun one liner I used to convert a whole directory of shaders to wrappers:
find ../shaders/*.shader -type f -exec java -jar ~/Desktop/Kopernicus/Tools/MaterialWrapperGenerator/MaterialWrapperGenerator.jar {} Kopernicus \;