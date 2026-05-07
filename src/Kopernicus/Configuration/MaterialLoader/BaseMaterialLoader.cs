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
using System.Collections.Generic;
using System.Linq;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.MaterialLoader.Parsing;
using Kopernicus.Configuration.Parsing;
using Kopernicus.OnDemand;
using KSPTextureLoader;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kopernicus.Configuration.MaterialLoader;

/// <summary>
/// The base class for all material loaders. It owns the live <see cref="Material"/>,
/// the on-demand texture entry list, and a set of validated property accessors that
/// both the raw <c>_X</c> key path (handled by <see cref="PostApply"/>) and the
/// per-shader friendly aliases route through.
/// </summary>
public abstract class BaseMaterialLoader : BaseLoader, IParserEventSubscriber
{
    /// <summary>
    /// The material actually loaded by this loader.
    /// </summary>
    public Material Value { get; protected set; }

    /// <summary>
    /// A list of <see cref="OnDemandTextureEntry"/>s that need to be loaded
    /// when this material is in use.
    /// </summary>
    public Dictionary<string, string> Entries { get; private set; } = [];

    /// <summary>
    /// The shader that will actually be loaded.
    /// </summary>
    ///
    /// <remarks>
    /// If made into a <see cref="ParserTarget" /> then this should be marked
    /// with <see cref="PreApply" /> because the material is created in
    /// <see cref="Apply" />.
    /// </remarks>
    public abstract ShaderParser ShaderParser { get; set; }

    /// <summary>
    /// Whether this specific material should use on-demand textures. Defaults
    /// to the global config but can be overridden if needed.
    /// </summary>
    public virtual NumericParser<bool> OnDemand { get; set; } = OnDemandStorage.UseOnDemand;

    public virtual void Apply(ConfigNode node)
    {
        var shader = ShaderParser.Value;

        if (shader is null)
        {
            Logger.Active.LogWarning("No shader specified for material. An error shader will be used instead.");
            shader = UnityEngine.Shader.Find("Hidden/InternalErrorShader");
        }

        if (Value is null)
            Value = new Material(shader);
        else
            Value.shader = shader;
    }

    public virtual void PostApply(ConfigNode node)
    {
        if (Value is null)
            return;

        var shader = Value.shader;
        var values = node.values;
        for (int i = 0; i < values.Count; ++i)
        {
            var entry = values[i];
            if (!entry.name.StartsWith("_"))
                continue;

            LoadEntry(shader, entry.name, entry.value);
        }
    }

    void LoadEntry(Shader shader, string name, string value)
    {
        var index = shader.FindPropertyIndex(name);
        if (index == -1)
        {
            // *Scale and *Offset don't exist as standalone shader properties;
            // they live alongside the underlying texture property. If the
            // base name resolves to a texture, route to the appropriate
            // setter instead.
            if (name.EndsWith("Scale"))
            {
                var subname = name.Substring(0, name.Length - "Scale".Length);
                if (shader.FindPropertyIndex(subname) != -1)
                {
                    var p = new Vector2Parser();
                    p.SetFromString(value);
                    SetTextureScale(subname, p.Value);
                    return;
                }
            }

            if (name.EndsWith("Offset"))
            {
                var subname = name.Substring(0, name.Length - "Offset".Length);
                if (shader.FindPropertyIndex(subname) != -1)
                {
                    var p = new Vector2Parser();
                    p.SetFromString(value);
                    SetTextureOffset(subname, p.Value);
                    return;
                }
            }

            Logger.Active.LogWarning($"Property name `{name}` is not a property on the shader `{shader.name}` and will be ignored.");
            return;
        }

        switch (shader.GetPropertyType(index))
        {
            case ShaderPropertyType.Color:
                {
                    var p = new ColorParser();
                    p.SetFromString(value);
                    SetColor(name, p.Value);
                    break;
                }
            case ShaderPropertyType.Vector:
                {
                    var p = new Vector4Parser();
                    p.SetFromString(value);
                    SetVector(name, p.Value);
                    break;
                }
            case ShaderPropertyType.Float:
            case ShaderPropertyType.Range:
                {
                    var p = new NumericParser<float>();
                    p.SetFromString(value);
                    SetFloat(name, p.Value);
                    break;
                }
            case ShaderPropertyType.Texture:
                SetTexture(name, value);
                break;
        }
    }

    bool TryFindProperty(string key, ShaderPropertyType expected, string typeName, out int index)
    {
        index = -1;
        if (Value is null)
            return false;

        var shader = Value.shader;
        index = shader.FindPropertyIndex(key);
        if (index == -1)
        {
            Logger.Active.LogWarning($"Shader property `{key}` does not exist on shader `{shader.name}`");
            return false;
        }

        if (shader.GetPropertyType(index) != expected)
        {
            Logger.Active.LogWarning($"Shader property `{key}` on shader `{shader.name}` is not a {typeName} property");
            return false;
        }

        return true;
    }

    // === Color ==============================================================

    public Color GetColor(string key)
    {
        if (Value is null)
            return default;
        return Value.GetColor(key);
    }

    public void SetColor(string key, Color value)
    {
        if (!TryFindProperty(key, ShaderPropertyType.Color, "color", out _))
            return;
        Value.SetColor(key, value);
    }

    // === Float / Range ======================================================

    public float GetFloat(string key)
    {
        if (Value is null)
            return default;
        return Value.GetFloat(key);
    }

    public void SetFloat(string key, float value)
    {
        if (Value is null)
            return;

        var shader = Value.shader;
        var index = shader.FindPropertyIndex(key);
        if (index == -1)
        {
            Logger.Active.LogWarning($"Shader property `{key}` does not exist on shader `{shader.name}`");
            return;
        }

        var type = shader.GetPropertyType(index);
        if (type != ShaderPropertyType.Float && type != ShaderPropertyType.Range)
        {
            Logger.Active.LogWarning($"Shader property `{key}` on shader `{shader.name}` is not a float or range property");
            return;
        }

        if (type == ShaderPropertyType.Range)
        {
            var range = shader.GetPropertyRangeLimits(index);
            value = Mathf.Clamp(value, range.x, range.y);
        }

        Value.SetFloat(key, value);
    }

    public int GetInt(string key)
    {
        if (Value is null)
            return default;
        return Value.GetInt(key);
    }

    public void SetInt(string key, int value)
    {
        if (Value is null)
            return;

        var shader = Value.shader;
        var index = shader.FindPropertyIndex(key);
        if (index == -1)
        {
            Logger.Active.LogWarning($"Shader property `{key}` does not exist on shader `{shader.name}`");
            return;
        }

        var type = shader.GetPropertyType(index);
        if (type != ShaderPropertyType.Float && type != ShaderPropertyType.Range)
        {
            Logger.Active.LogWarning($"Shader property `{key}` on shader `{shader.name}` is not a float property");
            return;
        }

        Value.SetInt(key, value);
    }

    // === Vector =============================================================

    public Vector4 GetVector(string key)
    {
        if (Value is null)
            return default;
        return Value.GetVector(key);
    }

    public void SetVector(string key, Vector4 value)
    {
        if (!TryFindProperty(key, ShaderPropertyType.Vector, "vector", out _))
            return;
        Value.SetVector(key, value);
    }

    // === Texture ============================================================

    public Texture GetTexture(string key)
    {
        if (Value is null)
            return null;
        return Value.GetTexture(key);
    }

    public void SetTexture(string key, MaterialTextureParser path)
    {
        if (path is null)
            return;
        SetTexture(key, path.Path);
    }

    public void SetTexture(string key, string path)
    {
        if (!TryFindProperty(key, ShaderPropertyType.Texture, "texture", out var index))
            return;

        var dim = Value.shader.GetPropertyTextureDimension(index);
        using var handle = LoadTextureForDim(dim, key, path);
        if (handle is null)
            return;

        try
        {
            Value.SetTexture(key, handle.GetTexture());
        }
        catch (Exception e)
        {
            Debug.LogError($"[Kopernicus] Failed to load texture {handle.Path}");
            Logger.Active.LogWarning($"Failed to load texture {handle.Path}");
            Logger.Active.LogException(e);
            Utility.LogMissingTexture(CurrentBody, handle.Path);
        }
    }

    public Vector2 GetTextureScale(string key)
    {
        if (Value is null)
            return default;
        return Value.GetTextureScale(key);
    }

    public void SetTextureScale(string key, Vector2 value)
    {
        if (!TryFindProperty(key, ShaderPropertyType.Texture, "texture", out _))
            return;
        Value.SetTextureScale(key, value);
    }

    public Vector2 GetTextureOffset(string key)
    {
        if (Value is null)
            return default;
        return Value.GetTextureOffset(key);
    }

    public void SetTextureOffset(string key, Vector2 value)
    {
        if (!TryFindProperty(key, ShaderPropertyType.Texture, "texture", out _))
            return;
        Value.SetTextureOffset(key, value);
    }

    // === Keyword ============================================================

    public bool GetKeyword(string keyword)
    {
        if (Value is null)
            return false;
        return Value.IsKeywordEnabled(keyword);
    }

    public void SetKeyword(string keyword, bool enabled)
    {
        if (Value is null)
            return;

        if (enabled)
            Value.EnableKeyword(keyword);
        else
            Value.DisableKeyword(keyword);
    }

    public string GetMultiKeyword(string[] keywords)
    {
        if (Value is null)
            return null;

        foreach (var k in keywords)
        {
            if (Value.IsKeywordEnabled(k))
                return k;
        }

        return null;
    }

    public void SetMultiKeyword(string[] keywords, string selected)
    {
        if (Value is null || selected is null)
            return;

        if (!keywords.Contains(selected))
        {
            Logger.Active.LogWarning($"Shader keyword `{selected}` is not valid for this property. Expected one of {string.Join(", ", keywords)}");
            return;
        }

        foreach (var k in keywords)
        {
            if (k != selected)
                Value.DisableKeyword(k);
        }

        Value.EnableKeyword(selected);
    }

    // === Gradient (baked into a 1D ramp texture) ============================

    public void SetGradient(string key, Configuration.Parsing.Gradient gradient)
    {
        if (gradient is null)
            return;
        if (!TryFindProperty(key, ShaderPropertyType.Texture, "texture", out _))
            return;
        Value.SetTexture(key, BakeRamp(gradient));
    }

    const int RAMP_WIDTH = 512;

    static Texture2D BakeRamp(Configuration.Parsing.Gradient gradient)
    {
        var ramp = new Texture2D(RAMP_WIDTH, 1)
        {
            wrapMode = TextureWrapMode.Clamp,
            mipMapBias = 0.0f,
        };

        var colors = ramp.GetPixels32(0);
        for (var i = 0; i < colors.Length; i++)
            colors[i] = gradient.ColorAt((float)i / colors.Length);

        ramp.SetPixels32(colors, 0);
        ramp.Apply(true, false);
        return ramp;
    }

    // === Texture loading ====================================================

    static PSystemBody CurrentBody =>
        Parser.GetState<Body>("Kopernicus:currentBody")?.GeneratedBody;

    TextureHandle<T> LoadTexture<T>(string key, string path)
        where T : Texture
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (path.StartsWith("BUILTIN/"))
        {
            path = path.Substring("BUILTIN/".Length);
            var texture = Resources
                .FindObjectsOfTypeAll<T>()
                .FirstOrDefault(tex => tex.name == path);
            if (texture == null)
            {
                Debug.LogError($"[Kopernicus] Could not find built-in texture {path} of type {typeof(T).Name}");
                Logger.Active.LogWarning($"Could not find built-in texture {path} of type {typeof(T).Name}");
                Utility.LogMissingTexture(CurrentBody, path);
                return null;
            }

            return TextureHandle.CreateExternalHandle<T>(texture);
        }

        if (typeof(T).IsAssignableFrom(typeof(Texture2D)))
        {
            if (GameDatabase.Instance.ExistsTexture(path))
            {
                return TextureHandle.CreateExternalHandle<T>(
                    (T)(Texture)GameDatabase.Instance.GetTexture(path, false)
                );
            }
        }

        if (OnDemand.Value)
        {
            path = Utility.ValidateOnDemandTexture(path);
            Entries[key] = path;
            return null;
        }
        else
        {
            var options = new TextureLoadOptions
            {
                Hint = TextureLoadHint.BatchSynchronous,
                Unreadable = true
            };

            Entries.Remove(key);
            var handle = TextureLoader.LoadTexture<T>(path, options);
            TextureHandleStorage.Instance.Store(handle.Acquire());
            return handle;
        }
    }

    TextureHandle LoadTextureForDim(TextureDimension dim, string key, string path)
    {
        switch (dim)
        {
            case TextureDimension.Tex2D:
                return LoadTexture<Texture2D>(key, path);

            case TextureDimension.Tex3D:
                return LoadTexture<Texture3D>(key, path);

            case TextureDimension.Tex2DArray:
                return LoadTexture<Texture2DArray>(key, path);

            case TextureDimension.CubeArray:
                return LoadTexture<CubemapArray>(key, path);

            default:
                return LoadTexture<Texture>(key, path);
        }
    }
}
