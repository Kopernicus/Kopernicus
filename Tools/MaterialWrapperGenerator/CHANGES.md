Change Log 
========================

Version 1.1
-----------
Properties for shaders can be accessed by a numerical id which can be retrived with a static call to Shader.PropertyToID(string).  This is a significantly faster process than looking them up with strings.  The generator now synthesizes a singleton property storage class for the material wrapper to hold these IDs.  Exact IDs are retrieved in the singleton's constructor.

<pre>
protected class Properties 
{
    private const string propertyNameKey = “_propertyName”;
    public int propertyNameId { get; private set; }

    private const string shaderName = “Path/To/Shader”;
    public Shader shader
    {
        get { return Shader.Find(shaderName); }
    }
    public Properties()
    {
        propertyNameId = Properties.shader.PropertyToID(propertyNameKey);
    }

    private static Properties singleton = null;
    public static Properties Instance
    {
        get
        {
            if(singleton == null) singleton = new Properties();
            return singleton;
        }
    }
}
</pre>


Version 1.0
-----------
Initial Release
