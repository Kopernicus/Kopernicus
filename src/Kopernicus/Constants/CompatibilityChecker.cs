/**
 * Copyright (c) 2014, Majiir
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification, are permitted
 * provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this list of
 * conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of
 * conditions and the following disclaimer in the documentation and/or other materials provided
 * with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Linq;
using System.Reflection;
using Kopernicus.ConfigParser;
using UnityEngine;
using Object = UnityEngine.Object;

/*-----------------------------------------*\
|   SUBSTITUTE YOUR MOD'S NAMESPACE HERE.   |
\*-----------------------------------------*/
namespace Kopernicus.Constants
{

    /**
     * This utility displays a warning with a list of mods that determine themselves
     * to be incompatible with the current running version of Kerbal Space Program.
     *
     * See this forum thread for details:
     * http://forum.kerbalspaceprogram.com/threads/65395-Voluntarily-Locking-Plugins-to-a-Particular-KSP-Version
     */

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class CompatibilityChecker : MonoBehaviour
    {
        // Compatible version
        internal const Int32 VERSION_MAJOR = 1;
        internal const Int32 VERSION_MINOR = 9;
        internal const Int32 REVISION = 1;
        internal const Int32 KOPERNICUS = 2;

        public static Boolean IsCompatible()
        {
            /*-----------------------------------------------*\
            |    BEGIN IMPLEMENTATION-SPECIFIC EDITS HERE.    |
            \*-----------------------------------------------*/

            #if !DEBUG
            return
                Versioning.version_major == VERSION_MAJOR &&
                Versioning.version_minor == VERSION_MINOR &&
                Versioning.Revision == REVISION;
            #else
            return true;
            #endif

            /*-----------------------------------------------*\
            | IMPLEMENTERS SHOULD NOT EDIT BEYOND THIS POINT! |
            \*-----------------------------------------------*/
        }

        // Version of the compatibility checker itself.
        private static Int32 _version = 4;

        private void Awake()
        {
            // If Kopernicus isn't compatible, activate the cats
            if (IsCompatible())
            {
                return;
            }

            Type moduleManager =
                Parser.ModTypes.FirstOrDefault(t => t.Name == "ModuleManager" && t.Namespace == "ModuleManager");
            if (moduleManager == null)
            {
                return; // no cat :(
            }

            FieldInfo nyan = moduleManager.GetField("nyan", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo ncats = moduleManager.GetField("nCats", BindingFlags.Instance | BindingFlags.NonPublic);
            Object mm = FindObjectOfType(moduleManager);
            nyan?.SetValue(mm, true);
            ncats?.SetValue(mm, true);

            // Nobody can read that popup
            ScreenMessages.PostScreenMessage(
                "Kopernicus will not work on this version of KSP!\nPlease don't try to open your saved games!",
                Single.MaxValue,
                ScreenMessageStyle.UPPER_LEFT, true);
        }

        public void Start()
        {
            // Checkers are identified by the type name and version field name.
            FieldInfo[] fields =
                Parser.ModTypes
                    .Where(t => t.Name == "CompatibilityChecker")
                    .Select(t => t.GetField("_version", BindingFlags.Static | BindingFlags.NonPublic))
                    .Where(f => f != null)
                    .Where(f => f.FieldType == typeof(Int32))
                    .ToArray();

            // Let the latest version of the checker execute.
            if (_version != fields.Max(f => (Int32) f.GetValue(null)))
            {
                return;
            }

            Debug.Log(
                $"[CompatibilityChecker] Running checker version {_version} from '{Assembly.GetExecutingAssembly().GetName().Name}'");

            // Other checkers will see this version and not run.
            // This accomplishes the same as an explicit "ran" flag with fewer moving parts.
            _version = Int32.MaxValue;

            // A mod is incompatible if its compatibility checker has an IsCompatible method which returns false.
            String[] incompatible =
                fields
                    .Select(f => f.DeclaringType?.GetMethod("IsCompatible", Type.EmptyTypes))
                    .Where(m => m != null && m.IsStatic)
                    .Where(m => m.ReturnType == typeof(Boolean))
                    .Where(m =>
                    {
                        try
                        {
                            return !(Boolean) m.Invoke(null, new System.Object[0]);
                        }
                        catch (Exception e)
                        {
                            // If a mod throws an exception from IsCompatible, it's not compatible.
                            Debug.LogWarning(
                                $"[CompatibilityChecker] Exception while invoking IsCompatible() from '{m.DeclaringType?.Assembly.GetName().Name}':\n\n{e}");
                            return true;
                        }
                    })
                    .Select(m => m.DeclaringType?.Assembly.GetName().Name)
                    .ToArray();

            // A mod is incompatible with Unity if its compatibility checker has an IsUnityCompatible method which returns false.
            String[] incompatibleUnity =
                fields
                    .Select(f => f.DeclaringType?.GetMethod("IsUnityCompatible", Type.EmptyTypes))
                    .Where(m => m != null) // Mods without IsUnityCompatible() are assumed to be compatible.
                    .Where(m => m.IsStatic)
                    .Where(m => m.ReturnType == typeof(Boolean))
                    .Where(m =>
                    {
                        try
                        {
                            return !(Boolean) m.Invoke(null, new System.Object[0]);
                        }
                        catch (Exception e)
                        {
                            // If a mod throws an exception from IsUnityCompatible, it's not compatible.
                            Debug.LogWarning(
                                $"[CompatibilityChecker] Exception while invoking IsUnityCompatible() from '{m.DeclaringType?.Assembly.GetName().Name}':\n\n{e}");
                            return true;
                        }
                    })
                    .Select(m => m.DeclaringType?.Assembly.GetName().Name)
                    .ToArray();

            Array.Sort(incompatible);
            Array.Sort(incompatibleUnity);

            String message = String.Empty;

            if (incompatible.Length > 0 || incompatibleUnity.Length > 0)
            {
                message += (message == String.Empty ? "Some" : "\n\nAdditionally, some") +
                           " installed mods may be incompatible with this version of Kerbal Space Program. Features may be broken or disabled. Please check for updates to the listed mods.";

                if (incompatible.Length > 0)
                {
                    Debug.LogWarning("[CompatibilityChecker] Incompatible mods detected: " +
                                     String.Join(", ", incompatible));
                    message +=
                        $"\n\nThese mods are incompatible with KSP {Versioning.version_major}.{Versioning.version_minor}.{Versioning.Revision}:\n\n";
                    message += String.Join("\n", incompatible);
                }

                if (incompatibleUnity.Length > 0)
                {
                    Debug.LogWarning("[CompatibilityChecker] Incompatible mods (Unity) detected: " +
                                     String.Join(", ", incompatibleUnity));
                    message += $"\n\nThese mods are incompatible with Unity {Application.unityVersion}:\n\n";
                    message += String.Join("\n", incompatibleUnity);
                }
            }

            if (incompatible.Length > 0 || incompatibleUnity.Length > 0 /*|| IsWin64()*/)
            {
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "CompatibilityChecker",
                    "Incompatible Mods Detected", message, "OK", true, UISkinManager.defaultSkin);
            }
        }
    }
}
