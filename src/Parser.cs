/**
 * Kopernicus ConfigNode Parser
 * ====================================
 * Created by: Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P.
 * -------------------------------------------------------------
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
 *
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright 2011-2016 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 *
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = System.Object;

namespace Kopernicus
{
    /// <summary>
    /// Class which manages loading from config nodes via reflection and attribution
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Create an object form a configuration node (Generic)
        /// </summary>
        /// <typeparam name="T">The resulting class</typeparam>
        /// <param name="node">The node with the data that should be loaded</param>
        /// <param name="modName">The name of the mod that corresponds to the entry in ParserOptions</param>
        /// <param name="getChilds">Whether getters on the object should get called</param>
        /// <returns></returns>
        public static T CreateObjectFromConfigNode<T>(ConfigNode node, String modName = "Default", Boolean getChilds = true) where T : class, new()
        {
            T o = new T();
            LoadObjectFromConfigurationNode(o, node, modName, getChilds);
            return o;
        }

        /// <summary>
        /// Create an object form a configuration node (Runtime type identification)
        /// </summary>
        /// <param name="type">The resulting class</param>
        /// <param name="node">The node with the data that should be loaded</param>
        /// <param name="modName">The name of the mod that corresponds to the entry in ParserOptions</param>
        /// <param name="getChilds">Whether getters on the object should get called</param>
        /// <returns></returns>
        public static Object CreateObjectFromConfigNode(Type type, ConfigNode node, String modName = "Default", Boolean getChilds = true)
        {
            Object o = Activator.CreateInstance(type);
            LoadObjectFromConfigurationNode(o, node, modName, getChilds);
            return o;
        }

        /// <summary>
        /// Create an object form a configuration node (Runtime type identification) with constructor parameters
        /// </summary>
        /// <param name="type">The resulting class</param>
        /// <param name="node">The node with the data that should be loaded</param>
        /// <param name="arguments">Arguments that should be passed to the constructor</param>
        /// <param name="modName">The name of the mod that corresponds to the entry in ParserOptions</param>
        /// <param name="getChilds">Whether getters on the object should get called</param>
        /// <returns></returns>
        public static Object CreateObjectFromConfigNode(Type type, ConfigNode node, Object[] arguments, String modName = "Default", Boolean getChilds = true)
        {
            Object o = Activator.CreateInstance(type, arguments);
            LoadObjectFromConfigurationNode(o, node, modName, getChilds);
            return o;
        }

        /// <summary>
        /// Load data for an object's ParserTarget fields and properties from a configuration node
        /// </summary>
        /// <param name="o">Object for which to load data.  Needs to be instatiated object</param>
        /// <param name="node">Configuration node from which to load data</param>
        /// <param name="modName">The name of the mod that corresponds to the entry in ParserOptions</param>
        /// <param name="getChilds">Whether getters on the object should get called</param>
        public static void LoadObjectFromConfigurationNode(Object o, ConfigNode node, String modName = "Default", Boolean getChilds = true)
        {
            // Get the object as a parser event subscriber (will be null if 'o' does not conform)
            IParserEventSubscriber subscriber = o as IParserEventSubscriber;

            // Generate two lists -> those tagged preapply and those not
            Dictionary<Boolean, MemberInfo> preapplyMembers = new Dictionary<Boolean, MemberInfo>();
            Dictionary<Boolean, MemberInfo> postapplyMembers = new Dictionary<Boolean, MemberInfo>();

            // Discover members tagged with parser attributes
            foreach (MemberInfo member in o.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                // Is this member a parser target?
                ParserTarget[] attributes = (ParserTarget[]) member.GetCustomAttributes((typeof(ParserTarget)), true);
                if (attributes.Length > 0)
                {
                    // If this member is a collection
                    Boolean isCollection = attributes[0].GetType() == typeof(ParserTargetCollection);

                    // If this member has the preapply attribute, we need to process it
                    if (member.GetCustomAttributes((typeof(PreApply)), true).Length > 0)
                        preapplyMembers.Add(isCollection, member);
                    else
                        postapplyMembers.Add(isCollection, member);
                }
            }

            // Process the preapply members
            foreach (KeyValuePair<bool, MemberInfo> member in preapplyMembers)
            {
                if (member.Key)
                    LoadCollectionMemberFromConfigurationNode(member.Value, o, node, modName, getChilds);
                else
                    LoadObjectMemberFromConfigurationNode(member.Value, o, node, modName, getChilds);
            }

            // Call Apply
            subscriber?.Apply(node);

            // Process the postapply members
            foreach (KeyValuePair<bool, MemberInfo> member in postapplyMembers)
            {
                if (member.Key)
                    LoadCollectionMemberFromConfigurationNode(member.Value, o, node);
                else
                    LoadObjectMemberFromConfigurationNode(member.Value, o, node);
            }

            // Call PostApply
            subscriber?.PostApply(node);
        }

        /// <summary>
        /// Load collection for ParserTargetCollection
        /// </summary>
        /// <param name="member">Member to load data for</param>
        /// <param name="o">Instance of the object which owns member</param>
        /// <param name="node">Configuration node from which to load data</param>
        /// <param name="modName">The name of the mod that corresponds to the entry in ParserOptions</param>
        /// <param name="getChilds">Whether getters on the object should get called</param>
        public static void LoadCollectionMemberFromConfigurationNode(MemberInfo member, Object o, ConfigNode node, String modName = "Default", Boolean getChilds = true)
        {
            // Get the target attribute
            ParserTargetCollection target = ((ParserTargetCollection[]) member.GetCustomAttributes(typeof(ParserTargetCollection), true))[0];

            // Figure out if this field exists and if we care
            Boolean isNode = node.HasNode(target.fieldName);
            Boolean isValue = node.HasValue(target.fieldName);

            // Obtain the type the member is (can only be field or property)
            Type targetType;
            Object targetValue = null;
            if (member.MemberType == MemberTypes.Field)
            {
                targetType = ((FieldInfo) member).FieldType;
                targetValue = getChilds ? ((FieldInfo) member).GetValue(o) : null;
            }
            else
            {
                targetType = ((PropertyInfo) member).PropertyType;
                try
                {
                    if (((PropertyInfo) member).CanRead && getChilds)
                        targetValue = ((PropertyInfo) member).GetValue(o, null);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            // If there was no data found for this node
            if (!isNode && !isValue)
            {
                if (!target.optional && !(target.allowMerge && targetValue != null))
                {
                    // Error - non optional field is missing
                    throw new ParserTargetMissingException("Missing non-optional field: " + o.GetType() + "." + target.fieldName);
                }

                // Nothing to do, so return
                return;
            }

            // If we are dealing with a generic collection
            if (targetType.IsGenericType)
            {
                // If the target is a generic dictionary
                if (typeof(IDictionary).IsAssignableFrom(targetType))
                {
                    throw new Exception("Generic dictionaries are unsupported at this time");
                }

                // If the target is a generic collection
                else if (typeof(IList).IsAssignableFrom(targetType))
                {
                    // We need a node for this decoding
                    if (!isNode)
                    {
                        throw new Exception("Loading a generic list requires sources to be nodes");
                    }

                    // Get the target value as a collection
                    IList collection = targetValue as IList;

                    // Get the internal type of this collection
                    Type genericType = targetType.GetGenericArguments()[0];

                    // Create a new collection if merge is disallowed or if the collection is null
                    if (collection == null || !target.allowMerge)
                    {
                        collection = Activator.CreateInstance(targetType) as IList;
                        targetValue = collection;
                    }

                    // Iterate over all of the nodes in this node
                    foreach (ConfigNode subnode in node.GetNode(target.fieldName).nodes)
                    {
                        // Check for the name significance
                        if (target.nameSignificance == NameSignificance.None)
                        {
                            // Just processes the contents of the node
                            collection?.Add(CreateObjectFromConfigNode(genericType, subnode, modName, target.getChild));
                        }

                        // Otherwise throw an exception because we don't support named ones yet
                        else if (target.nameSignificance == NameSignificance.Type)
                        {
                            // Generate the type from the name
                            Type elementType = ModTypes.FirstOrDefault(t => t.Name == subnode.name);

                            // Add the object to the collection
                            collection?.Add(CreateObjectFromConfigNode(elementType, subnode, modName, target.getChild));
                        }
                    }
                }
            }

            // If we are dealing with a non generic collection
            else
            {
                // Check for invalid scenarios
                if (target.nameSignificance == NameSignificance.None)
                {
                    throw new Exception("Can not infer type from non generic target; can not infer type from zero name significance");
                }
            }

            // If the member type is a field, set the value
            if (member.MemberType == MemberTypes.Field)
            {
                ((FieldInfo) member).SetValue(o, targetValue);
            }

            // If the member wasn't a field, it must be a property.  If the property is writable, set it.
            else if (((PropertyInfo) member).CanWrite)
            {
                ((PropertyInfo) member).SetValue(o, targetValue, null);
            }
        }

        /// <summary>
        /// Load data for ParserTarget field or property from a configuration node
        /// </summary>
        /// <param name="member">Member to load data for</param>
        /// <param name="o">Instance of the object which owns member</param>
        /// <param name="node">Configuration node from which to load data</param>        /// <param name="modName">The name of the mod that corresponds to the entry in ParserOptions</param>
        /// <param name="getChilds">Whether getters on the object should get called</param>
        public static void LoadObjectMemberFromConfigurationNode(MemberInfo member, object o, ConfigNode node, String modName = "Default", Boolean getChilds = true)
        {
            // Get the parser target, only one is allowed so it will be first
            ParserTarget target = ((ParserTarget[]) member.GetCustomAttributes(typeof(ParserTarget), true))[0];

            // Figure out if this field exists and if we care
            Boolean isNode = node.HasNode(target.fieldName);
            Boolean isValue = node.HasValue(target.fieldName);

            // Obtain the type the member is (can only be field or property)
            Type targetType;
            Object targetValue = null;
            if (member.MemberType == MemberTypes.Field)
            {
                targetType = ((FieldInfo) member).FieldType;
                targetValue = getChilds ? ((FieldInfo) member).GetValue(o) : null;
            }
            else
            {
                targetType = ((PropertyInfo) member).PropertyType;
                try
                {
                    if (((PropertyInfo) member).CanRead && getChilds)
                        targetValue = ((PropertyInfo) member).GetValue(o, null);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            // Get settings data
            ParserOptions.Data data = ParserOptions.options[modName];

            // Log
            data.logCallback("Parsing Target " + target.fieldName + " in (" + o.GetType() + ") as (" + targetType + ")");

            // If there was no data found for this node
            if (!isNode && !isValue)
            {
                if (!target.optional && !(target.allowMerge && targetValue != null))
                {
                    // Error - non optional field is missing
                    throw new ParserTargetMissingException("Missing non-optional field: " + o.GetType() + "." + target.fieldName);
                }

                // Nothing to do, so DONT return!
                return;
            }

            // Does this node have a required config source type (and if so, check if valid)
            RequireConfigType[] attributes = (RequireConfigType[]) member.GetCustomAttributes(typeof(RequireConfigType), true);
            if (attributes.Length > 0)
            {
                if ((attributes[0].type == ConfigType.Node && !isNode) || (attributes[0].type == ConfigType.Value && !isValue))
                {
                    throw new ParserTargetTypeMismatchException(target.fieldName + " requires config value of " + attributes[0].type);
                }
            }

            // If this object is a value (attempt no merge here)
            if (isValue)
            {
                // The node value
                String nodeValue = node.GetValue(target.fieldName);

                // Merge all values of the node
                if (target.getAll != null)
                {
                    nodeValue = String.Join(target.getAll, node.GetValues(target.fieldName));
                }

                // If the target is a string, it works natively
                if (targetType == typeof(String))
                {
                    targetValue = nodeValue;
                }

                // Figure out if this object is a parsable type
                else if (typeof(IParsable).IsAssignableFrom(targetType))
                {
                    // Create a new object
                    IParsable targetParsable = (IParsable)Activator.CreateInstance(targetType);
                    targetParsable.SetFromString(nodeValue);
                    targetValue = targetParsable;
                }

                // Throw exception or print error
                else
                {
                    data.logCallback("[Kopernicus]: Configuration.Parser: ParserTarget \"" + target.fieldName + "\" is a non parsable type: " + targetType);
                    return;
                }
            }

            // If this object is a node (potentially merge)
            else
            {
                // If the target type is a ConfigNode, this works natively
                if (targetType == typeof(ConfigNode))
                    targetValue = node.GetNode(target.fieldName);

                // We need to get an instance of the object we are trying to populate
                // If we are not allowed to merge, or the object does not exist, make a new instance
                else if (targetValue == null || !target.allowMerge)
                    targetValue = CreateObjectFromConfigNode(targetType, node.GetNode(target.fieldName), modName, target.getChild);

                // Otherwise we can merge this value
                else
                    LoadObjectFromConfigurationNode(targetValue, node.GetNode(target.fieldName), modName, target.getChild);
            }

            // If the member type is a field, set the value
            if (member.MemberType == MemberTypes.Field)
            {
                ((FieldInfo) member).SetValue(o, targetValue);
            }

            // If the member wasn't a field, it must be a property.  If the property is writable, set it.
            else if (((PropertyInfo) member).CanWrite)
            {
                ((PropertyInfo) member).SetValue(o, targetValue, null);
            }
        }

        /// <summary>
        /// Loads ParserTargets from other assemblies in GameData/
        /// </summary>
        public static void LoadParserTargetsExternal(ConfigNode node, String modName = "Default", Boolean getChilds = true)
        {
            LoadExternalParserTargets(node, modName, getChilds);
            foreach (ConfigNode childNode in node.GetNodes())
            {
                LoadParserTargetsExternal(childNode, modName, getChilds);
            }
        }

        /// <summary>
        /// Loads ParserTargets from other assemblies in GameData/
        /// </summary>
        public static void LoadExternalParserTargets(ConfigNode node, String modName = "Default", Boolean getChilds = true)
        {
            // Look for types in other assemblies with the ExternalParserTarget attribute and the parentNodeName equal to this node's name
            try
            {
                foreach (Type type in ModTypes)
                {
                    ParserTargetExternal[] attributes = (ParserTargetExternal[]) type.GetCustomAttributes(typeof(ParserTargetExternal), false);
                    if (attributes.Length == 0) continue;
                    ParserTargetExternal external = attributes[0];
                    if (node.name != external.parentNodeName)
                        continue;
                    String nodeName = external.configNodeName ?? type.Name;

                    // Get settings data
                    ParserOptions.Data data = ParserOptions.options[modName];

                    if (!node.HasNode(nodeName)) continue;
                    try
                    {
                        data.logCallback("Parsing ExternalTarget " + nodeName + " in node " + external.parentNodeName + " from Assembly " + type.Assembly.FullName);
                        ConfigNode nodeToLoad = node.GetNode(nodeName);
                        Object obj = CreateObjectFromConfigNode(type, nodeToLoad, modName, getChilds);
                    }
                    catch (MissingMethodException missingMethod)
                    {
                        data.logCallback("Failed to load ExternalParserTarget " + nodeName + " because it does not have a parameterless constructor");
                        data.errorCallback(missingMethod);
                    }
                    catch (Exception exception)
                    {
                        data.logCallback("Failed to load ExternalParserTarget " + nodeName + " from node " + external.parentNodeName);
                        data.errorCallback(exception);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
            }
        }


        // Custom Assembly query since AppDomain and Assembly loader are not quite what we want in 1.1
        private static List<Type> _ModTypes;
        public static List<Type> ModTypes
        {
            get
            {
                if (_ModTypes == null)
                    GetModTypes();
                return _ModTypes;
            }
        }

        private static void GetModTypes()
        {
            _ModTypes = new List<Type>();
            List<Assembly> asms = new List<Assembly>();
            asms.AddRange(AssemblyLoader.loadedAssemblies.Select(la => la.assembly));
            asms.AddUnique(typeof(PQSMod_VertexSimplexHeightAbsolute).Assembly);
            asms.AddUnique(typeof(PQSLandControl).Assembly);
            foreach (Type t in asms.SelectMany(a => a.GetTypes()))
            {
                _ModTypes.Add(t);
            }
        }
    }
}