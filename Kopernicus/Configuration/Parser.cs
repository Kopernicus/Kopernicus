/**
 * Kopernicus Planetary System Modifier
 * Copyright (C) 2014 Bryce C Schroeder (bryce.schroeder@gmail.com), Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * http://www.ferazelhosting.net/~bryce/contact.html
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
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Kopernicus
{
	namespace Configuration
	{
		/**
		 * Attribute used to tag a property or field which can be targeted by the parser
		 **/
		[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
		public class ParserTarget : Attribute
		{
			// Storage key in config node tree.  If null, key is determined with reflection
			public string fieldName = null;

			// Flag indiciating whether the presence of this value is required
			public bool optional = true;

			// Flag indiciating whether the contents of the config tree can be merged
			// via reflection with a potentially present field.  If the field is null,
			// this flag is disregarged
			public bool allowMerge = true;

			// Constructor sets name
			public ParserTarget(string fieldName = null)
			{
				this.fieldName = fieldName;
			}
		}

		/**
		 * Attribute indicating this parser target should be processed before calling Apply()
		 **/
		[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
		public class PreApply : Attribute
		{
			public PreApply()
			{

			}
		}

		/* Types of config node entries */
		public enum ConfigType
		{
			Value,
			Node
		}

		/* Attribute indicating the type of config node data this can load from - node or value */
		[AttributeUsage(AttributeTargets.Class)]
		public class RequireConfigType : Attribute
		{
			public ConfigType type { get; private set; }
			public RequireConfigType (ConfigType type)
			{
				this.type = type;
			}
		}
		
		/**
		 * Exception representing a missing field
		 **/
		public class TargetMissingException : Exception
		{
			public TargetMissingException (string message) : base(message)
			{
				
			}
		}
		
		/**
		 * Exception representing a field having the wrong storage type (i.e. string field is set to node)
		 **/
		public class TargetTypeMismatchException : Exception
		{
			public TargetTypeMismatchException (string message) : base(message)
			{
				
			}
		}

		/**
		 * Interface a class can implement to get events from the parser
		 **/
		public interface IParserEventSubscriber
		{
			// Apply Event - called after [PreApply] targets have been processed, but before the rest
			void Apply (ConfigNode node);    

			// Post Apply Event = called after all targets have been applied
			void PostApply (ConfigNode node);
		}

		/**
		 * Interface a class can implment to support conversion from a string
		 **/
		public interface IParsable
		{
			// Set value from string
			void SetFromString (string s);
		}

		/**
		 * Class which manages loading from config nodes via reflection and 
		 * attribution
		 **/
		public class Parser
		{
			// Create an object form a configuration node (Generic)
			public static T CreateObjectFromConfigNode <T> (ConfigNode node) where T : class, new()
			{
				// Allocate an instance of "type"
				T o = new T ();
				
				// Populate this object with the load object from configuration
				LoadObjectFromConfigurationNode (o, node);
				
				// Return the new object
				return o;
			}

			// Create an object form a configuration node (Runtime type identification)
			public static object CreateObjectFromConfigNode (Type type, ConfigNode node)
			{
				// Allocate an instance of "type"
				object o = Activator.CreateInstance (type);

				// Populate this object with the load object from configuration
				LoadObjectFromConfigurationNode (o, node);

				// Return the new object
				return o;
			}

			/**
			 * Load data for an object's ParserTarget fields and properties from a configuration node
			 * 
			 * @param o Object for which to load data.  Needs to be instatiated object
			 * @param node Configuration node from which to load data
			 **/
			public static void LoadObjectFromConfigurationNode (object o, ConfigNode node)
			{
				// Get the object as a parser event subscriber (will be null if 'o' does not conform)
				IParserEventSubscriber subscriber = o as IParserEventSubscriber;

				// Generate two lists -> those tagged preapply and those not
				List<MemberInfo> preapplyMembers = new List<MemberInfo> ();
				List<MemberInfo> postapplyMembers = new List<MemberInfo> ();

				// Discover members tagged with parser attributes
				foreach (MemberInfo member in o.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) 
				{
					// Is this member a valid target?
					if(member.GetCustomAttributes((typeof (ParserTarget)), true).Length > 0)
					{
						// If this collection has the preapply attribute, we need to process it
						if(member.GetCustomAttributes((typeof (PreApply)), true).Length > 0)
							preapplyMembers.Add(member);
						else
							postapplyMembers.Add(member);
					}
				}

				// Process the preapply members
				foreach (MemberInfo member in preapplyMembers)
					LoadObjectMemberFromConfigurationNode (member, o, node);

				// Call Apply
				if (subscriber != null) 
					subscriber.Apply (node); 
				
				// Process the postapply members
				foreach (MemberInfo member in postapplyMembers)
					LoadObjectMemberFromConfigurationNode (member, o, node);

				// Call PostApply
				if (subscriber != null)
					subscriber.PostApply (node);
			}

			/**
			 * Load data for ParserTarget field or property from a configuration node
			 * 
			 * @param member Member to load data for
			 * @param o Instance of the object which owns member
			 * @param node Configuration node from which to load data
			 **/
			private static void LoadObjectMemberFromConfigurationNode (MemberInfo member, object o, ConfigNode node)
			{
				// Get the parser target, only one is allowed so it will be first
				ParserTarget target = (member.GetCustomAttributes ((typeof(ParserTarget)), true) as ParserTarget[]) [0];
				//Debug.Log ("[Kopernicus]: Configuration.Parser: ParserTarget = (" + target.fieldName + ", optional = " + target.optional + ", allowsMerge = " + target.allowMerge + ")");
				
				// Figure out if this field exists and if we care
				bool isNode = node.HasNode (target.fieldName);
				bool isValue = node.HasValue (target.fieldName);
				
				// Obtain the type the member is (can only be field or property)
				Type targetType = null;
				object targetValue = null;
				if (member.MemberType == MemberTypes.Field) {
					targetType = (member as FieldInfo).FieldType;
					targetValue = (member as FieldInfo).GetValue (o);
				} else {
					targetType = (member as PropertyInfo).PropertyType;
					if ((member as PropertyInfo).CanRead)
						targetValue = (member as PropertyInfo).GetValue (o, null);
				}
				//Debug.Log ("[Kopernicus]: Configuration.Parser:   Type = " + targetType + ", isNode = " + isNode + ", isValue = " + isValue);
				
				// Verify that this situation is okay
				if (!(target.allowMerge && targetValue != null) && (!target.optional && (!isNode && !isValue)))
				{
					// Error - non optional field is missing
					throw new TargetMissingException ("Missing non-optional field: " + o.GetType () + "." + target.fieldName);
				}

				// Does this node have a required config source type (and if so, check if valid)
				RequireConfigType[] attributes = member.GetCustomAttributes (typeof(RequireConfigType), true) as RequireConfigType[];
				if (attributes.Length > 0) 
				{
					if((attributes[0].type == ConfigType.Node && !isNode) || (attributes[0].type == ConfigType.Value && !isValue))
					{
						throw new TargetTypeMismatchException (target.fieldName + " requires config value of " + attributes[0].type);
					}
				}
				
				// If this object is a value (attempt no merge here)
				if(isValue)
				{
					// If the target is a string, it works natively
					if(targetType.Equals(typeof (string)))
					{
						targetValue = node.GetValue(target.fieldName);
					}
					
					// Figure out if this object is a parsable type
					else if((typeof (IParsable)).IsAssignableFrom(targetType))
					{
						// Create a new object
						IParsable targetParsable = (IParsable) Activator.CreateInstance(targetType);
						targetParsable.SetFromString(node.GetValue(target.fieldName));
						targetValue = targetParsable;
					}

					// Throw exception or print error
					else
					{
						Debug.LogError("[Kopernicus]: Configuration.Parser: ParserTarget \"" + target.fieldName + "\" is a non parsable type: " + targetType);
					}
				}
				
				// If this object is a node (potentially merge)
				else if(isNode)
				{
					// We need to get an instance of the object we are trying to populate
					// If we are not allowed to merge, or the object does not exist, make a new instance
					if(targetValue == null || !target.allowMerge)
					{
						targetValue = CreateObjectFromConfigNode(targetType, node.GetNode(target.fieldName));
					}
					
					// Otherwise we can merge this value
					else
					{
						LoadObjectFromConfigurationNode(targetValue, node.GetNode(target.fieldName));
					}
				}

				// Didn't exist....
				else return;
				
				// If the member type is a field, set the value
				if(member.MemberType == MemberTypes.Field)
				{
					(member as FieldInfo).SetValue(o, targetValue);
				}

				// If the member wasn't a field, it must be a property.  If the property is writable, set it.
				else if((member as PropertyInfo).CanWrite)
				{
					(member as PropertyInfo).SetValue(o, targetValue, null);
				}
			}

			// End class
		}
	}
}

