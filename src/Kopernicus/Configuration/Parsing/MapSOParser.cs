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
using System.Diagnostics.CodeAnalysis;
using Kopernicus.Components;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.OnDemand;
using KSPTextureLoader;
using UnityEngine;

namespace Kopernicus.Configuration.Parsing;

// Parser for a MapSO
[RequireConfigType(ConfigType.Value)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MapSOParserGreyScale<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public void SetFromString(string s)
    {
        // Should we use OnDemand?
        bool useOnDemand = OnDemandStorage.UseOnDemand;

        if (s.StartsWith("BUILTIN/"))
        {
            s = s.Substring(8);
            Value = Utility.FindMapSO<T>(s);
        }
        else
        {
            // are we on-demand? Don't load now.
            if (useOnDemand && typeof(T) == typeof(MapSO))
            {
                s = Utility.ValidateOnDemandTexture(s);

                MapSODemand map = ScriptableObject.CreateInstance<MapSODemand>();
                map.Path = s;
                map.Depth = MapSO.MapDepth.Greyscale;
                map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                OnDemandStorage.AddMap(generatedBody.name, map);
                Value = map as T;
            }
            else // Load the texture
            {
                var options = new TextureLoadOptions
                {
                    Hint = TextureLoadHint.Synchronous,
                    Unreadable = false
                };
                var handle = TextureLoader.LoadTexture<Texture2D>(s, options);
                Texture2D map;
                try
                {
                    map = handle.TakeTexture();
                }
                catch (Exception e)
                {
                    Logger.Active.Log($"Failed to load texture {s}");
                    Logger.Active.LogException(e);
                    return;
                }

                // Create a new map script object
                Value = ScriptableObject.CreateInstance<T>();
                Value.CreateMap(MapSO.MapDepth.Greyscale, map);
            }
        }

        if (Value != null)
        {
            Value.name = s;
        }
    }

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public String ValueToString()
    {
        if (Value == null)
        {
            return null;
        }

        if (GameDatabase.Instance.ExistsTexture(Value.name) || TextureLoader.TextureExists(Value.name))
        {
            return Value.name;
        }

        return "BUILTIN/" + Value.name;
    }

    /// <summary>
    /// Create a new MapSOParser_GreyScale
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MapSOParserGreyScale()
    {

    }

    /// <summary>
    /// Create a new MapSOParser_GreyScale from an already existing Texture
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public MapSOParserGreyScale(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Convert Parser to Value
    /// </summary>
    public static implicit operator T(MapSOParserGreyScale<T> parser)
    {
        return parser.Value;
    }

    /// <summary>
    /// Convert Value to Parser
    /// </summary>
    public static implicit operator MapSOParserGreyScale<T>(T value)
    {
        return new MapSOParserGreyScale<T>(value);
    }
}

// Parser for a MapSO
[RequireConfigType(ConfigType.Value)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MapSOParserHeightAlpha<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public void SetFromString(String s)
    {
        // Should we use OnDemand?
        Boolean useOnDemand = OnDemandStorage.UseOnDemand;

        if (s.StartsWith("BUILTIN/"))
        {
            s = s.Substring(8);
            Value = Utility.FindMapSO<T>(s);
        }
        else
        {
            // are we on-demand? Don't load now.
            if (useOnDemand && typeof(T) == typeof(MapSO))
            {
                s = Utility.ValidateOnDemandTexture(s);

                MapSODemand map = ScriptableObject.CreateInstance<MapSODemand>();
                map.Path = s;
                map.Depth = MapSO.MapDepth.HeightAlpha;
                map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                OnDemandStorage.AddMap(generatedBody.name, map);
                Value = map as T;
            }
            else // Load the texture
            {
                var options = new TextureLoadOptions
                {
                    Hint = TextureLoadHint.Synchronous,
                    Unreadable = false
                };
                var handle = TextureLoader.LoadTexture<Texture2D>(s, options);
                Texture2D map;
                try
                {
                    map = handle.TakeTexture();
                }
                catch (Exception e)
                {
                    Logger.Active.Log($"Failed to load texture {s}");
                    Logger.Active.LogException(e);
                    return;
                }

                // Create a new map script object
                Value = ScriptableObject.CreateInstance<T>();
                Value.CreateMap(MapSO.MapDepth.HeightAlpha, map);
            }
        }

        if (Value != null)
        {
            Value.name = s;
        }
    }

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public String ValueToString()
    {
        if (Value == null)
        {
            return null;
        }

        if (GameDatabase.Instance.ExistsTexture(Value.name) || TextureLoader.TextureExists(Value.name))
        {
            return Value.name;
        }

        return "BUILTIN/" + Value.name;
    }

    /// <summary>
    /// Create a new MapSOParser_GreyScale
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MapSOParserHeightAlpha()
    {

    }

    /// <summary>
    /// Create a new MapSOParser_GreyScale from an already existing Texture
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public MapSOParserHeightAlpha(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Convert Parser to Value
    /// </summary>
    public static implicit operator T(MapSOParserHeightAlpha<T> parser)
    {
        return parser.Value;
    }

    /// <summary>
    /// Convert Value to Parser
    /// </summary>
    public static implicit operator MapSOParserHeightAlpha<T>(T value)
    {
        return new MapSOParserHeightAlpha<T>(value);
    }
}

// Parser for a MapSO RGB
[RequireConfigType(ConfigType.Value)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MapSOParserRGB<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public void SetFromString(String s)
    {
        // Should we use OnDemand?
        Boolean useOnDemand = OnDemandStorage.UseOnDemand;

        if (s.StartsWith("BUILTIN/"))
        {
            s = s.Substring(8);
            Value = Utility.FindMapSO<T>(s);
        }
        else
        {
            // check if OnDemand.
            if (useOnDemand && typeof(T) == typeof(MapSO))
            {
                s = Utility.ValidateOnDemandTexture(s);

                MapSODemand map = ScriptableObject.CreateInstance<MapSODemand>();
                map.Path = s;
                map.Depth = MapSO.MapDepth.RGB;
                map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                OnDemandStorage.AddMap(generatedBody.name, map);
                Value = map as T;
            }
            else
            {
                var options = new TextureLoadOptions
                {
                    Hint = TextureLoadHint.Synchronous,
                    Unreadable = false
                };
                var handle = TextureLoader.LoadTexture<Texture2D>(s, options);
                Texture2D map;
                try
                {
                    map = handle.TakeTexture();
                }
                catch (Exception e)
                {
                    Logger.Active.Log($"Failed to load texture {s}");
                    Logger.Active.LogException(e);
                    return;
                }

                // Create a new map script object
                Value = ScriptableObject.CreateInstance<T>();
                Value.CreateMap(MapSO.MapDepth.RGB, map);
            }
        }

        if (Value != null)
        {
            Value.name = s;
        }
    }

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public String ValueToString()
    {
        if (Value == null)
        {
            return null;
        }

        if (GameDatabase.Instance.ExistsTexture(Value.name) || TextureLoader.TextureExists(Value.name))
        {
            return Value.name;
        }

        return "BUILTIN/" + Value.name;
    }

    /// <summary>
    /// Create a new MapSOParser_RGB
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MapSOParserRGB()
    {

    }

    /// <summary>
    /// Create a new MapSOParser_RGB from an already existing Texture
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public MapSOParserRGB(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Convert Parser to Value
    /// </summary>
    public static implicit operator T(MapSOParserRGB<T> parser)
    {
        return parser.Value;
    }

    /// <summary>
    /// Convert Value to Parser
    /// </summary>
    public static implicit operator MapSOParserRGB<T>(T value)
    {
        return new MapSOParserRGB<T>(value);
    }
}

// Parser for a MapSO RGBA
[RequireConfigType(ConfigType.Value)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MapSOParserRGBA<T> : BaseLoader, IParsable, ITypeParser<T> where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public void SetFromString(String s)
    {
        // Should we use OnDemand?
        Boolean useOnDemand = OnDemandStorage.UseOnDemand;

        if (s.StartsWith("BUILTIN/"))
        {
            s = s.Substring(8);
            Value = Utility.FindMapSO<T>(s);
        }
        else
        {
            // check if OnDemand.
            if (useOnDemand && typeof(T) == typeof(MapSO))
            {
                s = Utility.ValidateOnDemandTexture(s);

                MapSODemand map = ScriptableObject.CreateInstance<MapSODemand>();
                map.Path = s;
                map.Depth = MapSO.MapDepth.RGBA;
                map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
                OnDemandStorage.AddMap(generatedBody.name, map);
                Value = map as T;
            }
            else
            {
                var options = new TextureLoadOptions
                {
                    Hint = TextureLoadHint.Synchronous,
                    Unreadable = false
                };
                var handle = TextureLoader.LoadTexture<Texture2D>(s, options);
                Texture2D map;
                try
                {
                    map = handle.TakeTexture();
                }
                catch (Exception e)
                {
                    Logger.Active.Log($"Failed to load texture {s}");
                    Logger.Active.LogException(e);
                    return;
                }

                // Create a new map script object
                Value = ScriptableObject.CreateInstance<T>();
                Value.CreateMap(MapSO.MapDepth.RGBA, map);
            }
        }

        if (Value != null)
        {
            Value.name = s;
        }
    }

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public String ValueToString()
    {
        if (Value == null)
        {
            return null;
        }

        if (GameDatabase.Instance.ExistsTexture(Value.name) || TextureLoader.TextureExists(Value.name))
        {
            return Value.name;
        }

        return "BUILTIN/" + Value.name;
    }

    /// <summary>
    /// Create a new MapSOParser_RGBA
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MapSOParserRGBA()
    {

    }

    /// <summary>
    /// Create a new MapSOParser_RGB from an already existing Texture
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public MapSOParserRGBA(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Convert Parser to Value
    /// </summary>
    public static implicit operator T(MapSOParserRGBA<T> parser)
    {
        return parser.Value;
    }

    /// <summary>
    /// Convert Value to Parser
    /// </summary>
    public static implicit operator MapSOParserRGBA<T>(T value)
    {
        return new MapSOParserRGBA<T>(value);
    }
}
