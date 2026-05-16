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
using UnityEngine;

namespace Kopernicus.OnDemand;

// These classes are responsible for routing material loads from PQSMod_OnDemandHandler
// to the actual material they are supposed to apply to.
//
// They don't have any behaviour (beyond the callback) but make sure that the correct
// material or object is being modified once the planetary system is instantiated.

internal class PQSSurfaceMaterialTextureListener : MonoBehaviour, IOnDemandTextureListener
{
    [SerializeField]
    private PQS pqs;

    public Shader Shader => pqs?.surfaceMaterial?.shader;
    public bool Immediate => false;

    void Start()
    {
        pqs ??= GetComponent<PQS>();
    }

    public void OnTextureLoaded(string property, Texture texture) =>
        pqs?.surfaceMaterial?.SetTexture(property, texture);
}

internal class PQSFallbackMaterialTextureListener : MonoBehaviour, IOnDemandTextureListener
{
    [SerializeField]
    private PQS pqs;

    public Shader Shader => pqs?.fallbackMaterial?.shader;
    public bool Immediate => false;

    void Start()
    {
        pqs ??= GetComponent<PQS>();
    }

    public void OnTextureLoaded(string property, Texture texture) =>
        pqs?.fallbackMaterial?.SetTexture(property, texture);
}

internal class MaterialTextureListener : MonoBehaviour, IOnDemandTextureListener
{
    [SerializeField]
    private Material material;

    public Shader Shader => material?.shader;
    public bool Immediate => false;

    public void Setup(Material material) => this.material = material;

    public void OnTextureLoaded(string property, Texture texture) =>
        material?.SetTexture(property, texture);
}
