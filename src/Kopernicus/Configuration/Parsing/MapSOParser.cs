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

public abstract class MapSOParserCommon<T> : BaseLoader, IParsable, ITypeParser<T>
    where T : MapSO
{
    /// <summary>
    /// The value that is being parsed
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// The <see cref="MapSO.MapDepth"/> that should be used for this map.
    /// </summary>
    private protected abstract MapSO.MapDepth Depth { get; }

    // This is an internal-only helper type for now.
    private protected MapSOParserCommon() { }

    private protected MapSOParserCommon(T value) { }

    public void SetFromString(string s)
    {
        // Should we use OnDemand?
        bool useOnDemand = OnDemandStorage.UseOnDemand;

        T mapSO;
        if (s.StartsWith("BUILTIN/"))
        {
            s = s.Substring("BUILTIN/".Length);
            mapSO = Utility.FindMapSO<T>(s);
        }
        else if (
            useOnDemand
            && (typeof(MapSODemand).IsAssignableFrom(typeof(T)) || typeof(T) == typeof(MapSO))
        )
        {
            s = Utility.ValidateOnDemandTexture(s);

            if (typeof(T) == typeof(MapSO))
                mapSO = (T)(MapSO)ScriptableObject.CreateInstance<MapSODemand>();
            else
                mapSO = ScriptableObject.CreateInstance<T>();
            MapSODemand map = (MapSODemand)(MapSO)mapSO;

            map.Path = s;
            map.Depth = Depth;
            map.AutoLoad = OnDemandStorage.OnDemandLoadOnMissing;

            OnDemandStorage.AddMap(generatedBody.name, map);
        }
        else
        {
            var options = new TextureLoadOptions
            {
                Hint = TextureLoadHint.Synchronous,
                Unreadable = false
            };
            using var handle = TextureLoader.LoadTexture<Texture2D>(s, options);
            try
            {
                _ = handle.GetTexture();
            }
            catch (Exception e)
            {
                Logger.Active.Log($"Failed to load texture {s}");
                Logger.Active.LogException(e);
                return;
            }

            if (typeof(T) == typeof(MapSO))
                mapSO = (T)(MapSO)ScriptableObject.CreateInstance<KopernicusMapSO>();
            else
                mapSO = ScriptableObject.CreateInstance<T>();

            // Keep the handle alive after it is disposed of, since methods
            // below need to take ownership of it.
            handle.Acquire();

            if (mapSO is KopernicusMapSO kmapSO)
            {
                kmapSO.CreateMap(Depth, handle);
                if (!kmapSO.IsLoaded)
                    return;
            }
            else
            {
                mapSO.CreateMap(Depth, handle.GetTexture());
            }
        }

        Value = mapSO;
        if (Value != null)
            Value.name = s;
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
}

// Parser for a MapSO
[RequireConfigType(ConfigType.Value)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MapSOParserGreyScale<T> : MapSOParserCommon<T>
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
    public new void SetFromString(string s) => base.SetFromString(s);

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public new String ValueToString() => base.ValueToString();

    /// <summary>
    /// Create a new MapSOParser_GreyScale
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MapSOParserGreyScale() { }

    /// <summary>
    /// Create a new MapSOParser_GreyScale from an already existing Texture
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MapSOParserHeightAlpha<T> : MapSOParserCommon<T>
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

    private protected override MapSO.MapDepth Depth => MapSO.MapDepth.HeightAlpha;

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public new void SetFromString(string s) => base.SetFromString(s);

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public new String ValueToString() => base.ValueToString();

    /// <summary>
    /// Create a new MapSOParser_GreyScale
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MapSOParserHeightAlpha() { }

    /// <summary>
    /// Create a new MapSOParser_GreyScale from an already existing Texture
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
public class MapSOParserRGB<T> : MapSOParserCommon<T>
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

    private protected override MapSO.MapDepth Depth => MapSO.MapDepth.RGB;

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public new void SetFromString(string s) => base.SetFromString(s);

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public new String ValueToString() => base.ValueToString();

    /// <summary>
    /// Create a new MapSOParser_RGB
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MapSOParserRGB() { }

    /// <summary>
    /// Create a new MapSOParser_RGB from an already existing Texture
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
public class MapSOParserRGBA<T> : MapSOParserCommon<T>
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

    private protected override MapSO.MapDepth Depth => MapSO.MapDepth.RGBA;

    /// <summary>
    /// Parse the Value from a string
    /// </summary>
    public new void SetFromString(string s) => base.SetFromString(s);

    /// <summary>
    /// Convert the value to a parsable String
    /// </summary>
    public new String ValueToString() => base.ValueToString();

    /// <summary>
    /// Create a new MapSOParser_RGBA
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public MapSOParserRGBA() { }

    /// <summary>
    /// Create a new MapSOParser_RGB from an already existing Texture
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
