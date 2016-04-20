/**
 * MaterialWrapperGenerator
 * Copyright (C) 2014 Nathaniel R. Lewis (linux.robotdude@gmail.com)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 */

import java.io.File;
import java.io.FileNotFoundException;
import java.io.PrintWriter;
import java.util.LinkedList;
import java.util.Scanner;
import java.util.regex.MatchResult;
import java.util.regex.Pattern;

/**
 *
 * @author nathaniel
 */
public class MaterialWrapperGenerator
{
    public static LinkedList<String> synthesizeInternalPropertiesClass(LinkedList<ShaderProperty> properties, String shaderName)
    {
        // Create a linked list for lines of code
        LinkedList<String> p = new LinkedList<>();

        // Synthesize the shader accessor
        p.add("// Internal property ID tracking object\n");
        p.add("protected class Properties\n");
        p.add("{\n");

        // Synthesize the shader for material property
        p.add("    // Return the shader for this wrapper\n");
        p.add("    private const string shaderName = \"" + shaderName + "\";\n");
        p.add("    public static Shader shader\n");
        p.add("    {\n");
        p.add("        get { return Shader.Find (shaderName); }\n");
        p.add("    }\n\n");

        // Synthesize the class properties
        for(ShaderProperty property : properties)
        {
            // Format the code for this property
            for(String line : property.synthesizePropertyIdStorage())
            {
                p.add("    " + line);
            }
        }

        // Synthesize single accessor / allocator
        p.add("    // Singleton instance\n");
        p.add("    private static Properties singleton = null;\n");
        p.add("    public static Properties Instance\n");
        p.add("    {\n");
        p.add("        get\n");
        p.add("        {\n");
        p.add("            // Construct the singleton if it does not exist\n");
        p.add("            if(singleton == null)\n");
        p.add("                singleton = new Properties();\n");
        p.add("\n");
        p.add("            return singleton;\n");
        p.add("        }\n");
        p.add("    }\n\n");

        // Synthesize constructor
        p.add("    private Properties()\n");
        p.add("    {\n");
        for(ShaderProperty property : properties)
        {
            // Format the code for this property
            for(String line : property.synthesizePropertyIdInitialization())
            {
                p.add("        " + line);
            }
        }
        p.add("    }\n");
        p.add("}\n\n");

        // Return the lines of code
        return p;
    }

    /**
     * @param args the command line arguments
     * @throws java.io.FileNotFoundException
     */
    public static void main(String[] args) throws FileNotFoundException, Exception
    {
        // Check properties
        if(args.length < 2)
        {
            System.err.println("Usage: java MaterialWrapperGenerator <path to .shader file>  <namespace of .cs file>");
            return;
        }

        // Shader property regular expression & compile to pattern
        String propertyRegex = " (.+) \\(\"(.+)\", (.+)\\) = (.+)";
        Pattern propertyPattern = Pattern.compile(propertyRegex);

        // Shader name regular expression
        String shaderNameRegex = "Shader \"(.+)\"";
        Pattern shaderNamePattern = Pattern.compile(shaderNameRegex);

        // List to hold all of our strings
        LinkedList<ShaderProperty> shaderProperties = new LinkedList<>();
        LinkedList<String>         content          = new LinkedList<>();
        LinkedList<String>         loaderContent    = new LinkedList<>();
        String                     shaderResource   = "";

        // Open the shader file (get the filename with no extension and no whitespace)
        File shader = new File(args[0]);
        String shaderName = shader.getName().replaceFirst("[.][^.]+$", "").replaceAll("(\\s|/|\\-|,)", "");

        // Declare our intent
        System.out.println("Processing \"" + args[0] + "\" ==> " + shaderName + ".cs");

        // Find the shader name of this material (ex. Terrain/PQS/Ocean Surface Quad")
        Scanner nameFinder = new Scanner(shader);
        while(nameFinder.hasNextLine())
        {
            // Get the results of running the regular expresison
            if(nameFinder.findInLine(shaderNamePattern) != null)
            {
                // Retrieve the shader name
                MatchResult result = nameFinder.match();
                shaderResource = result.group(1);

                // Found the name, get out
                break;
            }

            // Advance to the next line
            nameFinder.nextLine();
        }
        nameFinder.close();

        // Open the parameters file, search for parameters with regex
        Scanner parameters = new Scanner(shader);

        // Iterate through the shader file looking for strings that match our pattern
        System.out.println(" --> Discovering Parameters for \"" + shaderResource + "\"");
        while(parameters.hasNextLine())
        {
            // Get the results of running the regular expresison
            String property = parameters.findInLine(propertyPattern);

            // If we found a result in this line
            if(property != null)
            {
                // Get the match result for this property
                MatchResult result = parameters.match();

                // Create a shader property to hold (and generate useful) information
                shaderProperties.add(new ShaderProperty(result.group(1), result.group(3), result.group(4), result.group(2)));
            }

            // Advance to the next line
            parameters.nextLine();
        }
        parameters.close();

        // Synthesize the internal properties class
        System.out.println(" --> Synthesizing Property Storage Class");
        content.addAll(MaterialWrapperGenerator.synthesizeInternalPropertiesClass(shaderProperties, shaderResource));
		
        // Synthesize UsesSameShader function
        System.out.println(" --> Synthesizing UsesSameShader function");
        content.add("// Is some random material this material\n");
        content.add("public static bool UsesSameShader(Material m)\n");
        content.add("{\n");
        content.add("	return m.shader.name == Properties.shader.name;\n");
        content.add("}\n\n");

        // Synthesize the accessor properties
        System.out.println(" --> Synthesizing Properties");
        for(ShaderProperty sProperty : shaderProperties)
        {
            content.addAll(sProperty.synthesizeProperty());
        }

        // Synthesize the constructors
        System.out.println(" --> Sythesizing Constructor");
        content.add("public " + shaderName + "() : base(Properties.shader)\n");
        content.add("{\n");
        content.add("}\n\n");
        content.add("public " + shaderName + "(string contents) : base(contents)\n");
        content.add("{\n");
        content.add("    base.shader = Properties.shader;\n");
        content.add("}\n\n");
        content.add("public " + shaderName + "(Material material) : base(material)\n");
        content.add("{\n");
        content.add("    // Throw exception if this material was not the proper material\n");
        content.add("    if (material.shader.name != Properties.shader.name)\n");
        content.add("        throw new InvalidOperationException(\"Type Mismatch: " + shaderResource + " shader required\");\n");
        content.add("}\n\n");

        // Synthesize loader properties
        System.out.println(" --> Synthesizing Loader Properties");
        for(ShaderProperty sProperty : shaderProperties)
        {
            loaderContent.addAll(sProperty.synthesizePropertySetter());
        }

        // Synthesize loader constructor
        System.out.println(" --> Synthesizing Loader Constructor");
        loaderContent.add("// Constructors\n");
        loaderContent.add("public " + shaderName + "Loader () : base() { }\n");
        loaderContent.add("public " + shaderName + "Loader (string contents) : base (contents) { }\n");
        loaderContent.add("public " + shaderName + "Loader (Material material) : base(material) { }\n");

        // Synthesize the final file
        System.out.println(" --> Writing " + shaderName + ".cs");
        PrintWriter writer = new PrintWriter(shaderName + ".cs", "UTF-8");
        writer.print("// Material wrapper generated by shader translator tool\n");
        writer.print("using System;\n");
        writer.print("using System.Reflection;\n");
        writer.print("using UnityEngine;\n\n");
        writer.print("namespace " + args[1] + "\n{\n");
        writer.print("    namespace MaterialWrapper\n    {\n");
        writer.print("        public class " + shaderName + " : Material\n        {\n");
        for(String line : content)
        {
            writer.print("            " + line);
        }
        writer.print("        }\n");
        writer.print("    }\n");
        writer.print("}\n");
        writer.close();

        // Syncthesize the configuration system file
        System.out.println(" --> Writing " + shaderName + "Loader.cs");
        writer = new PrintWriter(shaderName + "Loader.cs", "UTF-8");
        writer.print("// Material wrapper generated by shader translator tool\n");
        writer.print("using System;\n");
        writer.print("using System.Reflection;\n");
        writer.print("using UnityEngine;\n\n");
        writer.print("using " + args[1] + ".MaterialWrapper;\n\n");
        writer.print("namespace " + args[1] + "\n{\n");
        writer.print("    namespace Configuration\n    {\n");
        writer.print("        public class " + shaderName + "Loader : " + shaderName + "\n        {\n");
        for(String line : loaderContent)
        {
            writer.print("            " + line);
        }
        writer.print("        }\n");
        writer.print("    }\n");
        writer.print("}\n");
        writer.close();

        System.out.println(" --> Complete");
    }

}
