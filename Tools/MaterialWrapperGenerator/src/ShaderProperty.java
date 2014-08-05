

import java.beans.Introspector;
import java.util.LinkedList;
import java.util.Scanner;
import java.util.regex.MatchResult;

/**
 *
 * @author nathaniel
 */
public class ShaderProperty 
{
    // Shader property properties
    private String name        = "";
    private String key         = "";
    private String description = "";
    private String type        = "";
    private String initializer = "";
    
    // Initializing constructor
    public ShaderProperty(String key, String type, String initializer, String description)
    {
        // Store the provided information
        this.key = key;
        this.type = type;
        this.initializer = initializer;
        this.description = description;
        
        // Compute a shorthand name for this object
        this.name = Introspector.decapitalize(key.replaceFirst("_", ""));
    }
    
    /**
     * @return List of strings representing the lines of code synthesized for the property ID storage
     */
    public LinkedList<String> synthesizePropertyIdStorage()
    {
                // String to hold property constants
        LinkedList<String> p = new LinkedList<>();
        
        // Synthesize a comment
        p.add("// " + description + ", default = " + initializer + "\n");
        
        // Synthesize the string constant for the key
        p.add("private const string " + name + "Key = \"" + key + "\";\n");
        
        // Synthesize the property id storage 
        p.add("public int " + name + "ID { get; private set; }\n\n");
        
        // Return the generated property
        return p;
    }
    
    /**
     * @return List of strings representing the lines of code synthesize for property storage initialization
     */
    public LinkedList<String> synthesizePropertyIdInitialization()
    {
        // String to hold property constants
        LinkedList<String> p = new LinkedList<>();
        
        // Synthesize the property id storage 
        p.add(name + "ID = Shader.PropertyToID(" + name + "Key);\n");
        
        // Return the generated property
        return p;
    }
    
    /**
     * @return 
     * @throws java.lang.Exception 
     */
    public LinkedList<String> synthesizeProperty() throws Exception
    {
        // String to hold property constants
        LinkedList<String> p = new LinkedList<>();
        
        // Synthesize a comment
        p.add("// " + description + ", default = " + initializer + "\n");
        
        // Operation depends on type
        if("Color".equals(type))
        {
            p.add("public Color " + name + "\n");
            p.add("{\n");
            p.add("    get { return GetColor (Properties.Instance." + name + "ID); }\n");
            p.add("    set { SetColor (Properties.Instance." + name + "ID, value); }\n");
            p.add("}\n\n");
        } 
        
        // If the type is a float
        else if("Float".equals(type))
        {
            p.add("public float " + name + "\n");
            p.add("{\n");
            p.add("    get { return GetFloat (Properties.Instance." + name + "ID); }\n");
            p.add("    set { SetFloat (Properties.Instance." + name + "ID, value); }\n");
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
            
            p.add("public float " + name + "\n");
            p.add("{\n");
            p.add("    get { return GetFloat (Properties.Instance." + name + "ID); }\n");
            p.add("    set { SetFloat (Properties.Instance." + name + "ID, Mathf.Clamp(value," + r.group(1) + "f," + r.group(2) + "f)); }\n");
            p.add("}\n\n");
            
            rangeFinder.close();
        }
        
        // If the type is a vector
        else if("Vector".equals(type))
        {
            p.add("public Vector4 " + name + "\n");
            p.add("{\n");
            p.add("    get { return GetVector (Properties.Instance." + name + "ID); }\n");
            p.add("    set { SetVector (Properties.Instance." + name + "ID, value); }\n");
            p.add("}\n\n");
        }
        
        // If the type is a 2D texture
        else if("2D".equals(type))
        {
            p.add("public Texture2D " + name + "\n");
            p.add("{\n");
            p.add("    get { return GetTexture (Properties.Instance." + name + "ID) as Texture2D; }\n");
            p.add("    set { SetTexture (Properties.Instance." + name + "ID, value); }\n");
            p.add("}\n\n");
        }
        
        // If the type is a 3D texture
        else if("3D".equals(type))
        {
            p.add("public Texture3D " + name + "\n");
            p.add("{\n");
            p.add("    get { return GetTexture (Properties.Instance." + name + "ID) as Texture3D; }\n");
            p.add("    set { SetTexture (Properties.Instance." + name + "ID, value); }\n");
            p.add("}\n\n");
        }
        
        // If the type is a cubemap texture
        else if("CUBE".equals(type))
        {
            p.add("public Cubemap " + name + "\n");
            p.add("{\n");
            p.add("    get { return GetTexture (Properties.Instance." + name + "ID) as Cubemap; }\n");
            p.add("    set { SetTexture (Properties.Instance." + name + "ID, value); }\n");
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
}
