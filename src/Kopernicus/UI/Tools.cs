/**
 * Kopernicus Planetary System Modifier
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
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus.UI
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
                targets.Add(parserTargets[0], memberInfo);
            }

            return targets;
        }

        /// <summary>
        /// Returns all ParserTargets in a specific type
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
                return ((FieldInfo)member).FieldType;
            }
            return member.MemberType == MemberTypes.Property ? ((PropertyInfo)member).PropertyType : null;
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

            return descriptions[0].Description;
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
            if (!(memberType.GetCustomAttributes(typeof(RequireConfigType),
                    false) is RequireConfigType[] configTypes) || !configTypes.Any())
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
                return ((FieldInfo)member).GetValue(reference);
            }

            if (member.MemberType != MemberTypes.Property)
            {
                return null;
            }

            PropertyInfo info = (PropertyInfo) member;
            return info.CanRead ? info.GetValue(reference, null) : null;
        }

        /// <summary>
        /// Sets the value that is assigned to a MemberInfo
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static void SetValue(MemberInfo member, Object reference, Object value)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                ((FieldInfo)member).SetValue(reference, value);
            }

            if (member.MemberType != MemberTypes.Property)
            {
                return;
            }
            PropertyInfo info = (PropertyInfo) member;
            if (info.CanWrite)
            {
                info.SetValue(reference, value, null);
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
            switch (value)
            {
                case null:
                    return null;
                case String s:
                    return s;
                case IParsable parsable:
                    return parsable.ValueToString();
                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Sets the value of a ParserTarget using a string
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
        /// Sets the value of a ParserTarget using a string
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static String ApplyInput(MemberInfo member, String input, Object reference)
        {
            SetValueFromString(member, reference, input);
            return FormatParsable(GetValue(member, reference)) ?? "";
        }

        /// <summary>
        /// Calls the methods declared as KittopiaDestructor
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static Object Construct(Type type, CelestialBody body)
        {
            // Get all methods of the object
            ConstructorInfo[] methods =
                type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Object value = null;

            for (Int32 i = 0; i < methods.Length; i++)
            {
                // Is the method a KittopiaDestructor?
                if (!HasAttribute<KittopiaConstructor>(methods[i]))
                {
                    continue;
                }
                KittopiaConstructor attr = GetAttributes<KittopiaConstructor>(methods[i])[0];
                switch (attr.Parameter)
                {
                    case KittopiaConstructor.ParameterType.Empty:
                        value = methods[i].Invoke(null);
                        break;
                    case KittopiaConstructor.ParameterType.CelestialBody:
                        value = methods[i].Invoke(new Object[] { body });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }

            yield return null;

            callback();
        }
    }
}