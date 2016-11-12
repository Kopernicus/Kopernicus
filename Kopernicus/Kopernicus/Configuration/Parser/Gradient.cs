/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class Gradient : IParserEventSubscriber
        {
            // Points in the gradient we are generating
            SortedList<float, Color> points = new SortedList<float, Color>();
            
            // Build the gradient from data found in the node
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // List of keyframes
                points = new SortedList<float, Color>();
                
                // Iterate through all the values in the node (all are keyframes)
                foreach(ConfigNode.Value point in node.values)
                {
                    // Convert the "name" (left side) into a float for sorting
                    float p = float.Parse(point.name);

                    // Get the color at this point
                    ColorParser cp = new ColorParser();
                    cp.SetFromString(point.value);
                    
                    // Add the keyframe to the list
                    points.Add(p, cp.value);
                }
            }
            
            // We don't use this
            void IParserEventSubscriber.PostApply(ConfigNode node) { }

            // Add a point to the gradient
            public void Add(float p, Color c)
            {
                points.Add(p, c);
            }

            // Get a color from the gradient
            public Color ColorAt (float p)
            {
                // Gradient points
                Color a = Color.black;
                Color b = Color.black;
                float ap = float.NaN;
                float bp = float.NaN;

                // Find the points along the gradient
                IEnumerator<KeyValuePair<float, Color>> enumerator = points.GetEnumerator ();
                while (enumerator.MoveNext()) 
                {
                    KeyValuePair<float, Color> point = enumerator.Current;
                    if(point.Key >= p)
                    {
                        bp = point.Key;
                        b = point.Value;

                        // If we never found a leading color
                        if(float.IsNaN(ap))
                            return b;

                        // break out
                        break;
                    }

                    // Otherwise cache these colors
                    ap = point.Key;
                    a = point.Value;
                }

                // If we never found a tail color
                if(float.IsNaN(bp))
                    return a;

                // Calculate the color
                float k = (p - ap) / (bp - ap);
                return Color.Lerp(a, b, k);
            }
        }
    }
}

