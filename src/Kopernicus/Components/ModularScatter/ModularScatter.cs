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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kopernicus.Components.ModularComponentSystem;
using Kopernicus.Components.Serialization;
using UnityEngine;

namespace Kopernicus.Components.ModularScatter
{
    /// <summary>
    /// Component to add other Components to Scatter objects easily
    /// </summary>
    public class ModularScatter : SerializableMonoBehaviour, IComponentSystem<ModularScatter>
    {
        /// <summary>
        /// Components that can be added to the Ring
        /// </summary>
        public List<IComponent<ModularScatter>> Components
        {
            get { return components; }
            set { components = value; }
        }

        /// <summary>
        /// The mod we are attached to
        /// </summary>
        public PQSLandControl landControl;

        /// <summary>
        /// The scatter instance we are attached to
        /// </summary>
        public PQSLandControl.LandClassScatter scatter;

        [SerializeField]
        private List<IComponent<ModularScatter>> components;

        /// <summary>
        /// Create a new ScatterExtension
        /// </summary>
        public ModularScatter()
        {
            Components = new List<IComponent<ModularScatter>>();
        }

        /// <summary>
        /// Create colliders for the scatter
        /// </summary>
        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        private void Start()
        {
            // Register us as the parental object for the scatter
            landControl = transform.parent.GetComponent<PQSLandControl>();
            transform.parent = landControl.sphere.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            scatter = landControl.scatters.FirstOrDefault(s => s.scatterName == scatter.scatterName); // I hate Unity
            typeof(PQSLandControl.LandClassScatter).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.FieldType == typeof(GameObject))?.SetValue(scatter, gameObject);

            // Call the modules
            Components.ForEach(c => c.Apply(this));
            Components.ForEach(c => c.PostApply(this));
        }

        private void Update()
        {
            Components.ForEach(c => c.Update(this));
        }
    }
}