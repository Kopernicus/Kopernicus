/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: - Bryce C Schroeder (bryce.schroeder@gmail.com)
 * 			   - Nathaniel R. Lewis (linux.robotdude@gmail.com)
 * 
 * Maintained by: - Thomas P.
 * 				  - NathanKell
 * 
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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
 * which is copyright 2011-2014 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class TGAUtils
        {
            public static Color32[] ReadImage(TGAHeader header, byte[] data)
            {
                TGAImage image = new TGAImage();
                TGAImageType imageType = header.imageType;
                if (imageType == TGAImageType.Uncompressed_TrueColor)
                {
                    return ReadTrueColorImage(header, data);
                }
                if (imageType != TGAImageType.RTE_TrueColor)
                {
                    Debug.Log("Image type of " + header.imageType.ToString() + " is not supported.");
                    return null;
                }
                return ReadRTETrueColorImage(header, data);
            }

            private static Color32[] ReadTrueColorImage(TGAHeader header, byte[] data)
            {
                MethodInfo read = typeof(TGAImage).GetMethod("ReadTrueColorImage", (BindingFlags.Instance | BindingFlags.NonPublic));
                Color32[] colors = read.Invoke(new TGAImage(), new object[] { header, data }) as Color32[];
                return colors;
            }

            private static Color32[] ReadRTETrueColorImage(TGAHeader header, byte[] data)
            {
                MethodInfo read = typeof(TGAImage).GetMethod("ReadRTETrueColorImage", (BindingFlags.Instance | BindingFlags.NonPublic));
                Color32[] colors = read.Invoke(new TGAImage(), new object[] { header, data }) as Color32[];
                return colors;
            }
        }
    }
}
