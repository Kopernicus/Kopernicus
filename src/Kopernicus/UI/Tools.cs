using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kopernicus;
using Kopernicus.UI;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    namespace UI
    {
        public static class Tools
        {
            /// <summary>
            /// Returns all ParserTargets in a specific type
            /// </summary>
            public static Dictionary<ParserTarget, MemberInfo> GetParserTargets(Type parserType)
            {
                // Create the dictionary
                Dictionary<ParserTarget, MemberInfo> targets = new Dictionary<ParserTarget, MemberInfo>();

                // Get all fields and properties from the type
                MemberInfo[] members = parserType.GetMembers()
                    .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property).ToArray();

                // Iterate over the members to check if they are ParserTargets
                foreach (MemberInfo memberInfo in members)
                {
                    // Get all ParserTargets from the current member
                    ParserTarget[] parserTargets = GetAttributes<ParserTarget>(memberInfo);

                    if (parserTargets == null || !parserTargets.Any())
                    {
                        continue;
                    }

                    // And add them to the dictionary
                    targets.Add(parserTargets[parserTargets.Length - 1], memberInfo);
                }

                return targets;
            }

            /// <summary>
            /// Returns all ParserTargets in a specific type
            /// </summary>
            public static Dictionary<ParserTarget, MemberInfo> GetParserTargets<T>()
            {
                return GetParserTargets(typeof(T));
            }

            /// <summary>
            /// Returns whether the ParserTarget describes a single value or a range of values
            /// </summary>
            public static Boolean IsCollection(ParserTarget parserTarget)
            {
                return parserTarget is ParserTargetCollection;
            }

            /// <summary>
            /// Returns the real type of a MemberInfo
            /// </summary>
            public static Type MemberType(MemberInfo member)
            {
                if (member.MemberType == MemberTypes.Field)
                {
                    return ((FieldInfo) member).FieldType;
                }

                if (member.MemberType == MemberTypes.Property)
                {
                    return ((PropertyInfo) member).PropertyType;
                }

                return null;
            }

            /// <summary>
            /// If a ParserTarget has a description, return it
            /// </summary>
            public static String GetDescription(MemberInfo memberInfo)
            {
                KittopiaDescription[] descriptions = GetAttributes<KittopiaDescription>(memberInfo);
                if (descriptions == null || !descriptions.Any())
                {
                    return null;
                }

                return descriptions[0].description;
            }

            /// <summary>
            /// Gets the config type that is required by the member
            /// </summary>
            public static ConfigType GetConfigType(Type memberType)
            {
                // Add exceptions for String and ConfigNode
                if (memberType == typeof(String))
                {
                    return ConfigType.Value;
                }

                if (memberType == typeof(ConfigNode))
                {
                    return ConfigType.Node;
                }

                // Get the RequireConfigType Attribute
                RequireConfigType[] configTypes =
                    memberType.GetCustomAttributes(typeof(RequireConfigType), false) as RequireConfigType[];

                if (configTypes == null || !configTypes.Any())
                {
                    throw new InvalidOperationException("Member needs to have a parsable type: " + memberType);
                }

                return configTypes[0].Type;
            }

            /// <summary>
            /// Returns the value that is assigned to a MemberInfo
            /// </summary>
            public static Object GetValue(MemberInfo member, Object reference)
            {
                if (member.MemberType == MemberTypes.Field)
                {
                    return ((FieldInfo) member).GetValue(reference);
                }

                if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo info = (PropertyInfo) member;
                    if (info.CanRead)
                    {
                        return info.GetValue(reference, null);
                    }
                }

                return null;
            }

            /// <summary>
            /// Sets the value that is assigned to a MemberInfo
            /// </summary>
            public static void SetValue(MemberInfo member, Object reference, Object value)
            {
                if (member.MemberType == MemberTypes.Field)
                {
                    ((FieldInfo) member).SetValue(reference, value);
                }

                if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo info = (PropertyInfo) member;
                    if (info.CanWrite)
                    {
                        info.SetValue(reference, value, null);
                    }
                }
            }

            /// <summary>
            /// Return whether the member has an ignore marker attached
            /// </summary>
            public static Boolean HasAttribute<T>(MemberInfo memberInfo)
                where T : Attribute
            {
                Object[] values =
                    memberInfo.GetCustomAttributes(typeof(T), false);
                return values.Any();
            }

            /// <summary>
            /// Return whether the member has an ignore marker attached
            /// </summary>
            public static T[] GetAttributes<T>(MemberInfo memberInfo) where T : Attribute
            {
                return memberInfo.GetCustomAttributes(typeof(T), false) as T[];
            }

            /// <summary>
            /// Converts a value to a parsable string representation
            /// </summary>
            public static String FormatParsable(Object value)
            {
                if (value == null)
                {
                    return null;
                }

                if (value is String)
                {
                    return (String) value;
                }

                if (value is IParsable)
                {
                    return ((IParsable) value).ValueToString();
                }

                return value.ToString();
            }

            /// <summary>
            /// Sets the value of a parsertarget using a string
            /// </summary>
            public static void SetValueFromString(MemberInfo member, Object reference, String value)
            {
                // Get the current value
                Object current = GetValue(member, reference);
                Object backup = current;

                try
                {
                    // Get the type of the member
                    Type memberType = MemberType(member);

                    // Is the member a string member?
                    if (memberType == typeof(String))
                    {
                        // Simply assign the new value
                        SetValue(member, reference, value);
                        return;
                    }

                    // Is the member a parsable type?
                    if (typeof(IParsable).IsAssignableFrom(memberType))
                    {
                        // Is the member null?
                        if (current == null)
                        {
                            SetValue(member, reference, current = Activator.CreateInstance(memberType));
                        }

                        // Now we can parse the value
                        IParsable parser = (IParsable) current;
                        parser.SetFromString(value);

                        // Reapply
                        SetValue(member, reference, parser);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    SetValue(member, reference, backup);
                    return;
                }

                // The member wasn't parsable
                throw new InvalidOperationException("The member wasn't parsable");
            }

            /// <summary>
            /// Sets the value of a parsertarget using a string
            /// </summary>
            public static Object SetValueFromString(Type memberType, Object current, String value)
            {
                // Get the current value
                Object backup = current;

                try
                {
                    // Is the member a string member?
                    if (memberType == typeof(String))
                    {
                        // Simply assign the new value
                        return current = value;
                    }

                    // Is the member a parsable type?
                    if (typeof(IParsable).IsAssignableFrom(memberType))
                    {
                        // Is the member null?
                        if (current == null)
                        {
                            current = Activator.CreateInstance(memberType);
                        }

                        // Now we can parse the value
                        IParsable parser = (IParsable) current;
                        parser.SetFromString(value);

                        // Reapply
                        return current = parser;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return current = backup;
                }

                // The member wasn't parsable
                throw new InvalidOperationException("The member wasn't parsable");
            }

            /// <summary>
            /// Parses and applies the user input to a ParserTarget
            /// </summary>
            public static String ApplyInput(MemberInfo member, String input, Object reference)
            {
                SetValueFromString(member, reference, input);
                return FormatParsable(GetValue(member, reference)) ?? "";
            }

            /// <summary>
            /// Calls the methods declared as KittopiaDestructor
            /// </summary>
            public static void Destruct(Object value)
            {
                // Get all methods of the object
                MethodInfo[] methods = value.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                for (Int32 i = 0; i < methods.Length; i++)
                {
                    // Is the method a KittopiaDestructor?
                    if (HasAttribute<KittopiaDestructor>(methods[i]))
                    {
                        methods[i].Invoke(value, null);
                    }
                }
            }

            /// <summary>
            /// Calls the methods declared as KittopiaDestructor
            /// </summary>
            public static Object Construct(Type type, CelestialBody body)
            {
                // Get all methods of the object
                ConstructorInfo[] methods =
                    type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                Object value = null;

                for (Int32 i = 0; i < methods.Length; i++)
                {
                    // Is the method a KittopiaDestructor?
                    if (HasAttribute<KittopiaConstructor>(methods[i]))
                    {
                        KittopiaConstructor attr = GetAttributes<KittopiaConstructor>(methods[i])[0];
                        if (attr.parameter == KittopiaConstructor.Parameter.Empty)
                        {
                            value = methods[i].Invoke(null);
                        }

                        if (attr.parameter == KittopiaConstructor.Parameter.CelestialBody)
                        {
                            value = methods[i].Invoke(new Object[] {body});
                        }
                    }
                }

                if (value == null)
                {
                    value = Activator.CreateInstance(type);
                }
                            
                // Check if the object implements other constructors
                if (typeof(ICreatable<CelestialBody>).IsAssignableFrom(type))
                {
                    ICreatable<CelestialBody> creatable = (ICreatable<CelestialBody>) value;
                    creatable.Create(body);
                }
                else if (typeof(ICreatable).IsAssignableFrom(type))
                {
                    ICreatable creatable = (ICreatable) value;
                    creatable.Create();
                }

                return value;
            }

            /// <summary>
            /// Returns all KittopiaActions in a specific type
            /// </summary>
            public static Dictionary<KittopiaAction, MethodInfo> GetKittopiaActions(Type parserType)
            {
                // Create the dictionary
                Dictionary<KittopiaAction, MethodInfo> targets = new Dictionary<KittopiaAction, MethodInfo>();

                // Get all methods from the type
                MethodInfo[] members =
                    parserType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                // Iterate over the members to check if they are KittopiaActions
                foreach (MethodInfo memberInfo in members)
                {
                    // Get all KittopiaActions from the current member
                    KittopiaAction[] actions = GetAttributes<KittopiaAction>(memberInfo);

                    if (actions == null || !actions.Any())
                    {
                        continue;
                    }

                    // And add them to the dictionary
                    targets.Add(actions[0], memberInfo);
                }

                return targets;
            }

            /// <summary>
            /// Calls a KittopiaAction function
            /// </summary>
            public static void InvokeKittopiaAction(MethodInfo method, Object reference, Action callback)
            {
                // Is the method an enumerator?
                if (typeof(IEnumerator).IsAssignableFrom(method.ReturnType))
                {
                    IEnumerator coroutine = (IEnumerator) method.Invoke(reference, null);
                    HighLogic.fetch.StartCoroutine(CoroutineCallback(coroutine, callback));
                }
                else
                {
                    // Simply invoke the method and call the callback
                    method.Invoke(reference, null);
                    callback();
                }
            }

            /// <summary>
            /// Executes a coroutine and executes code after it finished
            /// </summary>
            private static IEnumerator CoroutineCallback(IEnumerator coroutine, Action callback)
            {
                while(coroutine.MoveNext())
                {
                    yield return coroutine.Current;
                }

                yield return null;

                callback();
            }
        }
    }
}