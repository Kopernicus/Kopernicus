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

#if FALSE
using Kopernicus.Components;
using System;
using System.Collections.Generic;
using Kopernicus.Configuration.NoiseLoader;
using UniLinq;

namespace Kopernicus
{
    namespace Configuration
    {
        public class DebrisFieldLoader : ComponentLoader<Ring, DebrisField>
        {
            [ParserTarget("seed")]
            public NumericParser<Int32> seed
            {
                get { return Value.seed; }
                set { Value.seed = value; }
            }

            [ParserTargetCollection("Meshes")]
            public List<MuParser> meshes
            {
                get { return Value.meshes.Select(m => new MuParser(m)).ToList(); }
                set { Value.meshes = value.Select(m => m.Value).ToArray(); }
            }

            [ParserTarget("XNoise", allowMerge = true, nameSignificance = NameSignificance.Type)]
            public INoiseLoader xNoise
            {
                get
                {
                    if (Value.xNoise != null)
                    {
                        Type noiseType = Value.xNoise.GetType();
                        foreach (Type loaderType in Parser.ModTypes)
                        {
                            if (loaderType.BaseType == null)
                                continue;
                            if (loaderType.BaseType.Namespace != "Kopernicus.Configuration.NoiseLoader")
                                continue;
                            if (!loaderType.BaseType.Name.StartsWith("NoiseLoader"))
                                continue;
                            if (loaderType.BaseType.GetGenericArguments()[0] != noiseType)
                                continue;

                            // We found our loader type
                            INoiseLoader loader = (INoiseLoader) Activator.CreateInstance(loaderType);
                            loader.Create(Value.xNoise);
                            return loader;
                        }
                    }
                    return null;
                }
                set { Value.xNoise = value.Noise; }
            }

            [ParserTarget("YNoise", allowMerge = true, nameSignificance = NameSignificance.Type)]
            public INoiseLoader yNoise
            {
                get
                {
                    if (Value.yNoise != null)
                    {
                        Type noiseType = Value.yNoise.GetType();
                        foreach (Type loaderType in Parser.ModTypes)
                        {
                            if (loaderType.BaseType == null)
                                continue;
                            if (loaderType.BaseType.Namespace != "Kopernicus.Configuration.NoiseLoader")
                                continue;
                            if (!loaderType.BaseType.Name.StartsWith("NoiseLoader"))
                                continue;
                            if (loaderType.BaseType.GetGenericArguments()[0] != noiseType)
                                continue;

                            // We found our loader type
                            INoiseLoader loader = (INoiseLoader) Activator.CreateInstance(loaderType);
                            loader.Create(Value.yNoise);
                            return loader;
                        }
                    }
                    return null;
                }
                set { Value.yNoise = value.Noise; }
            }

            [ParserTarget("ZNoise", allowMerge = true, nameSignificance = NameSignificance.Type)]
            public INoiseLoader zNoise
            {
                get
                {
                    if (Value.zNoise != null)
                    {
                        Type noiseType = Value.zNoise.GetType();
                        foreach (Type loaderType in Parser.ModTypes)
                        {
                            if (loaderType.BaseType == null)
                                continue;
                            if (loaderType.BaseType.Namespace != "Kopernicus.Configuration.NoiseLoader")
                                continue;
                            if (!loaderType.BaseType.Name.StartsWith("NoiseLoader"))
                                continue;
                            if (loaderType.BaseType.GetGenericArguments()[0] != noiseType)
                                continue;

                            // We found our loader type
                            INoiseLoader loader = (INoiseLoader) Activator.CreateInstance(loaderType);
                            loader.Create(Value.zNoise);
                            return loader;
                        }
                    }
                    return null;
                }
                set { Value.zNoise = value.Noise; }
            }

            [ParserTarget("XRotNoise", allowMerge = true, nameSignificance = NameSignificance.Type)]
            public INoiseLoader xRotNoise
            {
                get
                {
                    if (Value.xRotNoise != null)
                    {
                        Type noiseType = Value.xRotNoise.GetType();
                        foreach (Type loaderType in Parser.ModTypes)
                        {
                            if (loaderType.BaseType == null)
                                continue;
                            if (loaderType.BaseType.Namespace != "Kopernicus.Configuration.NoiseLoader")
                                continue;
                            if (!loaderType.BaseType.Name.StartsWith("NoiseLoader"))
                                continue;
                            if (loaderType.BaseType.GetGenericArguments()[0] != noiseType)
                                continue;

                            // We found our loader type
                            INoiseLoader loader = (INoiseLoader) Activator.CreateInstance(loaderType);
                            loader.Create(Value.xRotNoise);
                            return loader;
                        }
                    }
                    return null;
                }
                set { Value.xRotNoise = value.Noise; }
            }

            [ParserTarget("YRotNoise", allowMerge = true, nameSignificance = NameSignificance.Type)]
            public INoiseLoader yRotNoise
            {
                get
                {
                    if (Value.yRotNoise != null)
                    {
                        Type noiseType = Value.yRotNoise.GetType();
                        foreach (Type loaderType in Parser.ModTypes)
                        {
                            if (loaderType.BaseType == null)
                                continue;
                            if (loaderType.BaseType.Namespace != "Kopernicus.Configuration.NoiseLoader")
                                continue;
                            if (!loaderType.BaseType.Name.StartsWith("NoiseLoader"))
                                continue;
                            if (loaderType.BaseType.GetGenericArguments()[0] != noiseType)
                                continue;

                            // We found our loader type
                            INoiseLoader loader = (INoiseLoader) Activator.CreateInstance(loaderType);
                            loader.Create(Value.yRotNoise);
                            return loader;
                        }
                    }
                    return null;
                }
                set { Value.yRotNoise = value.Noise; }
            }

            [ParserTarget("ZRotNoise", allowMerge = true, nameSignificance = NameSignificance.Type)]
            public INoiseLoader zRotNoise
            {
                get
                {
                    if (Value.zRotNoise != null)
                    {
                        Type noiseType = Value.zRotNoise.GetType();
                        foreach (Type loaderType in Parser.ModTypes)
                        {
                            if (loaderType.BaseType == null)
                                continue;
                            if (loaderType.BaseType.Namespace != "Kopernicus.Configuration.NoiseLoader")
                                continue;
                            if (!loaderType.BaseType.Name.StartsWith("NoiseLoader"))
                                continue;
                            if (loaderType.BaseType.GetGenericArguments()[0] != noiseType)
                                continue;

                            // We found our loader type
                            INoiseLoader loader = (INoiseLoader) Activator.CreateInstance(loaderType);
                            loader.Create(Value.zRotNoise);
                            return loader;
                        }
                    }
                    return null;
                }
                set { Value.zRotNoise = value.Noise; }
            }

            [ParserTarget("offset")]
            public Vector3Parser offset
            {
                get { return Value.offset; }
                set { Value.offset = value; }
            }

            [ParserTarget("densityMap")]
            public Texture2DParser densityMap
            {
                get { return Value.densityMap; }
                set { Value.densityMap = value; }
            }

            [ParserTarget("outwardSteps")]
            public NumericParser<Int32> outwardSteps
            {
                get { return Value.outwardSteps; }
                set { Value.outwardSteps = value; }
            }
        }
    }
}
#endif