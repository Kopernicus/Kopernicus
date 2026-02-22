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
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.Enumerations;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.OnDemand;
using KSPTextureLoader;
using UnityEngine;

namespace Kopernicus.Configuration.Parsing;

/// <summary>
/// An internal base class used to implement various MapSOParser types.
/// You should never need to use this directly.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MapSOParserBase<T> : BaseLoader, IParsable, ITypeParser<T>
    where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public T Value { get; set; }

    private protected abstract MapSO.MapDepth Depth { get; }

    private protected MapSOParserBase() { }

    private protected MapSOParserBase(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public void SetFromString(string s)
    {
        using var guard = new SetNameGuard(this, s);

        if (s.StartsWith("BUILTIN/"))
        {
            s = s.Substring("BUILTIN/".Length);
            Value = Utility.FindMapSO<T>(s);
            return;
        }

        if (OnDemandStorage.UseOnDemand &&
            (typeof(MapSODemand).IsAssignableFrom(typeof(T)) || typeof(T) == typeof(MapSO))
        )
        {
            s = Utility.ValidateOnDemandTexture(s);

            MapSODemand map;
            // If we want to load the texture on demand 
            if (typeof(T) == typeof(MapSO))
                map = ScriptableObject.CreateInstance<MapSODemand>();
            else
                map = (MapSODemand)(MapSO)ScriptableObject.CreateInstance<T>();

            map.Path = s;
            map.Depth = Depth;
            map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;
            OnDemandStorage.AddMap(generatedBody.name, map);
            Value = (T)(MapSO)map;
            return;
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
            Value.CreateMap(Depth, map);
        }
    }

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public string ValueToString()
    {
        if (Value.IsNullOrDestroyed())
            return "";

        var name = Value.name;
        if (GameDatabase.Instance.ExistsTexture(name))
            return name;
        if (TextureLoader.TextureExists(name))
            return name;

        return $"BUILTIN/{name}";
    }

    struct SetNameGuard(MapSOParserBase<T> parser, string name) : IDisposable
    {
        public void Dispose()
        {
            var value = parser.Value;
            if (value.IsNullOrDestroyed())
                return;

            value.name = name;
        }
    }
}

// Note that we need to define forwarding methods for all the types here so
// that the parser types stay binary-compatible with previous versions.

// Parser for a MapSO
[RequireConfigType(ConfigType.Value)]
public class MapSOParserGreyScale<T> : MapSOParserBase<T>, IParsable, ITypeParser<T>
    where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public new T Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    private protected override MapSO.MapDepth Depth => MapSO.MapDepth.Greyscale;

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public new void SetFromString(String s) => base.SetFromString(s);

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public new string ValueToString() => base.ValueToString();

    /// <summary>
    /// Create a new MapSOParser_GreyScale
    /// </summary>
    public MapSOParserGreyScale() { }

    /// <summary>
    /// Create a new MapSOParser_GreyScale from an already existing Texture
    /// </summary>
    public MapSOParserGreyScale(T value) : base(value) { }

    /// <summary>
    /// Convert Parser to Value
    /// </summary>
    public static implicit operator T(MapSOParserGreyScale<T> parser) => parser.Value;

    /// <summary>
    /// Convert Value to Parser
    /// </summary>
    public static implicit operator MapSOParserGreyScale<T>(T value) => new(value);
}

// Parser for a MapSO
[RequireConfigType(ConfigType.Value)]
public class MapSOParserHeightAlpha<T> : MapSOParserBase<T>, IParsable, ITypeParser<T>
    where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public new T Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    private protected override MapSO.MapDepth Depth => MapSO.MapDepth.Greyscale;

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public new void SetFromString(String s) => base.SetFromString(s);

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public new string ValueToString() => base.ValueToString();

    /// <summary>
    /// Create a new MapSOParser_GreyScale
    /// </summary>
    public MapSOParserHeightAlpha() { }

    /// <summary>
    /// Create a new MapSOParser_GreyScale from an already existing Texture
    /// </summary>
    public MapSOParserHeightAlpha(T value) : base(value) { }

    /// <summary>
    /// Convert Parser to Value
    /// </summary>
    public static implicit operator T(MapSOParserHeightAlpha<T> parser) => parser.Value;

    /// <summary>
    /// Convert Value to Parser
    /// </summary>
    public static implicit operator MapSOParserHeightAlpha<T>(T value) => new(value);
}

// Parser for a MapSO RGB
[RequireConfigType(ConfigType.Value)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MapSOParserRGB<T> : MapSOParserBase<T>, IParsable, ITypeParser<T>
    where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public new T Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    private protected override MapSO.MapDepth Depth => MapSO.MapDepth.Greyscale;

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public new void SetFromString(String s) => base.SetFromString(s);

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public new string ValueToString() => base.ValueToString();

    /// <summary>
    /// Create a new MapSOParser_GreyScale
    /// </summary>
    public MapSOParserRGB() { }

    /// <summary>
    /// Create a new MapSOParser_GreyScale from an already existing Texture
    /// </summary>
    public MapSOParserRGB(T value) : base(value) { }

    /// <summary>
    /// Convert Parser to Value
    /// </summary>
    public static implicit operator T(MapSOParserRGB<T> parser) => parser.Value;

    /// <summary>
    /// Convert Value to Parser
    /// </summary>
    public static implicit operator MapSOParserRGB<T>(T value) => new(value);
}

// Parser for a MapSO RGBA
[RequireConfigType(ConfigType.Value)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MapSOParserRGBA<T> : MapSOParserBase<T>, IParsable, ITypeParser<T>
    where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public new T Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    private protected override MapSO.MapDepth Depth => MapSO.MapDepth.Greyscale;

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public new void SetFromString(String s) => base.SetFromString(s);

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public new string ValueToString() => base.ValueToString();

    /// <summary>
    /// Create a new MapSOParser_GreyScale
    /// </summary>
    public MapSOParserRGBA() { }

    /// <summary>
    /// Create a new MapSOParser_GreyScale from an already existing Texture
    /// </summary>
    public MapSOParserRGBA(T value) : base(value) { }

    /// <summary>
    /// Convert Parser to Value
    /// </summary>
    public static implicit operator T(MapSOParserRGBA<T> parser) => parser.Value;

    /// <summary>
    /// Convert Value to Parser
    /// </summary>
    public static implicit operator MapSOParserRGBA<T>(T value) => new(value);
}
