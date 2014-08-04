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
import java.beans.Introspector;

/**
 *
 * @author nathaniel
 */
public class MaterialWrapperGenerator
{
    /**
     * @param result Match result from a regular expression parser for the property
     * @return 
     */
    public static LinkedList<String> generatePropertyForMatchResult(MatchResult result) throws Exception
    {
        // String to hold property constants
        LinkedList<String> p = new LinkedList<>();
        
        // Extract the important values of the property
        String key = result.group(1);
        String description = result.group(2);
        String type = result.group(3);
        String initializer = result.group(4);
        
        // Generate the short hand name of this key
        String property = Introspector.decapitalize(key.replaceFirst("_", ""));
        
        // Synthesize a comment
        p.add("// " + description + ", default = " + initializer + "\n");
        
        // Synthesize the string constant for the key
        p.add("private const string " + property + "Key = \"" + key + "\";\n");
        
        // Operation depends on type
        if("Color".equals(type))
        {
            p.add("public Color " + property + "\n");
            p.add("{\n");
            p.add("    get { return GetColor (" + property + "Key); }\n");
            p.add("    set { SetColor (" + property + "Key, value); }\n");
            p.add("}\n\n");
        } 
        
        // If the type is a float
        else if("Float".equals(type))
        {
            p.add("public float " + property + "\n");
            p.add("{\n");
            p.add("    get { return GetFloat (" + property + "Key); }\n");
            p.add("    set { SetFloat (" + property + "Key, value); }\n");
            p.add("}\n\n");
        }
        
        // If the type is a range
        else if(type.contains("Range"))
        {
            // Use a regular expression to parse this
            String rangeRegex = "Range\\((.+),(.+)\\)";
            Scanner rangeFinder = new Scanner(type);
            rangeFinder.findInLine(rangeRegex);
            MatchResult r = rangeFinder.match();
            
            p.add("public float " + property + "\n");
            p.add("{\n");
            p.add("    get { return GetFloat (" + property + "Key); }\n");
            p.add("    set { SetFloat (" + property + "Key, Mathf.Clamp(value," + r.group(1) + "f," + r.group(2) + "f)); }\n");
            p.add("}\n\n");
            
            rangeFinder.close();
        }
        
        // If the type is a vector
        else if("Vector".equals(type))
        {
            p.add("public Vector4 " + property + "\n");
            p.add("{\n");
            p.add("    get { return GetVector (" + property + "Key); }\n");
            p.add("    set { SetVector (" + property + "Key, value); }\n");
            p.add("}\n\n");
        }
        
        // If the type is a 2D texture
        else if("2D".equals(type))
        {
            p.add("public Texture2D " + property + "\n");
            p.add("{\n");
            p.add("    get { return GetTexture (" + property + "Key) as Texture2D; }\n");
            p.add("    set { SetTexture (" + property + "Key, value); }\n");
            p.add("}\n\n");
        }
        
        // If the type is a 3D texture
        else if("3D".equals(type))
        {
            p.add("public Texture3D " + property + "\n");
            p.add("{\n");
            p.add("    get { return GetTexture (" + property + "Key) as Texture3D; }\n");
            p.add("    set { SetTexture (" + property + "Key, value); }\n");
            p.add("}\n\n");
        }
        
        // If the type is a cubemap texture
        else if("CUBE".equals(type))
        {
            p.add("public Cubemap " + property + "\n");
            p.add("{\n");
            p.add("    get { return GetTexture (" + property + "Key) as Cubemap; }\n");
            p.add("    set { SetTexture (" + property + "Key, value); }\n");
            p.add("}\n\n");
        }
        
        // Error
        else
        {
            // Don't process
            throw new Exception("Key: " + key + " has an unrecognized type");
        }
        
        // Return the generated property
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
        LinkedList<String> content = new LinkedList<>();
        
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
            String property = nameFinder.findInLine(shaderNamePattern);
            
            // If we found a result in this line
            if(property != null)
            {
                // Get the match result from the name finder
                MatchResult result = nameFinder.match();

                // Declare name
                System.out.println(" --> Shader Name = " + result.group(1));
                
                // Synthesize the shader for material property
                content.add("// Return the shader for this wrapper\n");
                content.add("private const string shaderName = \"" + result.group(1) + "\";\n");
                content.add("private static Shader shaderForMaterial\n");
                content.add("{\n");
                content.add("    get { return Shader.Find (shaderName); }\n");
                content.add("}\n\n");
                
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
        System.out.println(" --> Generating Parameters");
        while(parameters.hasNextLine())
        {
            // Get the results of running the regular expresison
            String property = parameters.findInLine(propertyPattern);
            
            // If we found a result in this line
            if(property != null)
            {
                MatchResult result = parameters.match();

                // Loop through all of the lines in parameters
                content.addAll(MaterialWrapperGenerator.generatePropertyForMatchResult(result));
            }
            
            // Advance to the next line
            parameters.nextLine();
        }
        parameters.close();
        
        // Synthesize the constructors
        System.out.println(" --> Sythesizing Constructor");
        content.add("public " + shaderName + "() : base(shaderForMaterial)\n");
        content.add("{\n");
        content.add("}\n\n");
        content.add("public " + shaderName + "(string contents) : base(contents)\n");
        content.add("{\n");
        content.add("    base.shader = shaderForMaterial;\n");
        content.add("}\n\n");
        content.add("public " + shaderName + "(Material material) : base(material)\n");
        content.add("{\n");
        content.add("    // Throw exception if this material was not the proper material\n");
        content.add("    if (material.shader.name != shaderName)\n");
        content.add("        throw new InvalidOperationException(\"" + shaderName + " material requires the \\\"\" + shaderName + \"\\\" shader\");\n");
        content.add("}\n\n");
        
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
        System.out.println(" --> Complete");
    }
    
}
