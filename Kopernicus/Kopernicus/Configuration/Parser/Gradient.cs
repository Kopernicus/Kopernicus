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
 * which is copyright 2011-2017 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kopernicus
{
    namespace Configuration
    {
        [RequireConfigType(ConfigType.Node)]
        public class Gradient : IParserEventSubscriber
        {
            // Points in the gradient we are generating
            SortedList<Single, Color> points = new SortedList<Single, Color>();
            
            // Build the gradient from data found in the node
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                // List of keyframes
                points = new SortedList<Single, Color>();
                
                // Iterate through all the values in the node (all are keyframes)
                foreach(ConfigNode.Value point in node.values)
                {
                    // Convert the "name" (left side) into a Single for sorting
                    Single p = Single.Parse(point.name);

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
            public void Add(Single p, Color c)
            {
                points.Add(p, c);
            }

            // Get a color from the gradient
            public Color ColorAt (Single p)
            {
                // Gradient points
                Color a = Color.black;
                Color b = Color.black;
                Single ap = Single.NaN;
                Single bp = Single.NaN;

                // Find the points along the gradient
                IEnumerator<KeyValuePair<Single, Color>> enumerator = points.GetEnumerator ();
                while (enumerator.MoveNext()) 
                {
                    KeyValuePair<Single, Color> point = enumerator.Current;
                    if(point.Key >= p)
                    {
                        bp = point.Key;
                        b = point.Value;

                        // If we never found a leading color
                        if(Single.IsNaN(ap))
                            return b;

                        // break out
                        break;
                    }

                    // Otherwise cache these colors
                    ap = point.Key;
                    a = point.Value;
                }

                // If we never found a tail color
                if(Single.IsNaN(bp))
                    return a;

                // Calculate the color
                Single k = (p - ap) / (bp - ap);
                return Color.Lerp(a, b, k);
            }
        }
    }
}

