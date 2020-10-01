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

namespace Kopernicus.Configuration.Enumerations
{
	// PQS Material Type Enum
	public enum SurfaceMaterialType
	{
		// pre-1.8 Shaders
		Vacuum = 0,
		Basic = 1,
		Main = 2,
		Optimized = 4,
		Extra = 8,

		// 1.8 Shaders (Kerbin, Terrain Quality Low - High)
		MainFastBlend = 16,
		OptimizedFastBlend = 32,
		Triplanar = 64,

		// 1.9 Atlas Shader (Kerbin, Terrain Quality Ultra)
		TriplanarAtlas = 128,

		// Old names, kept around for compatibility
		AtmosphericBasic = 1,
		AtmosphericMain = 2,
		AtmosphericOptimized = 4,
		AtmosphericExtra = 8,
		AtmosphericMainFastBlend = 16,
		AtmosphericOptimizedFastBlend = 32,
		AtmosphericTriplanarZoomRotation = 64,
		AtmosphericTriplanarZoomRotationTextureArray = 128,
	}
}
