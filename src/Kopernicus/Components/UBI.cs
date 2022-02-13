/*
 * UBI - Unique Body Identifier
 * Copyright (c) 2018 Interstellar Consortium
 * This file is licensed under the MIT license
 *
 * UBI is a system for referencing bodies in Kerbal Space Program without
 * using the internal name. It does not impose the same restrictions, like
 * having fixed names for homeworld ("Kerbin") and center of the universe ("Sun").
 * It uses a different structure than normal names, it has to be a "system name"
 * and a "body name", combined with a forward slash "/". * 
 * 
 * This file is a reference implementation for UBI. Feel free to include and use
 * this file in your mods to make them compatible with UBI references.
 * It includes backwards compatibility with the internal names to maintain
 * config compatibility.
 *
 * In case a internal name gets queried the functions can log a warning that they
 * encountered a non-UBI name. If you want to enable that warning,
 * uncomment the line after this block
 */

// #define LOG_FALLBACK_NAME

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "CheckNamespace")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class UBI
{
    private static readonly Regex Parser = new Regex(@"^.+/.+$");

    /// <summary>
    /// Returns the name of the first found body implementing a UBI
    /// </summary>
    public static String GetName(String ubi, CelestialBody[] localBodies = null)
    {
        return GetNames(ubi, localBodies).FirstOrDefault();
    }

    /// <summary>
    /// Returns the first found body implementing a UBI
    /// </summary>
    public static CelestialBody GetBody(String ubi, CelestialBody[] localBodies = null)
    {
        return GetBodies(ubi, localBodies).FirstOrDefault();
    }

    /// <summary>
    /// Returns the names of all found bodies implementing a UBI
    /// </summary>
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public static String[] GetNames(String ubi, CelestialBody[] localBodies = null)
    {
        return GetBodies(ubi, localBodies).Select(b => b.transform.name).ToArray();
    }

    /// <summary>
    /// Returns all found bodies implementing a UBI
    /// </summary>
    public static CelestialBody[] GetBodies(String ubi, CelestialBody[] localBodies = null)
    {
        if (localBodies == null)
        {
            localBodies = PSystemManager.Instance.localBodies.ToArray();
        }

        List<CelestialBody> bodies = new List<CelestialBody>();

        // Check if the UBI is valid
#if LOG_FALLBACK_NAME
        if (String.IsNullOrEmpty(ubi) || !Parser.IsMatch(ubi))
        {
            Debug.Log("[UBI] Encountered invalid UBI \"" + ubi + "\". Falling back to internal name.");
        }
#endif

        for (Int32 i = 0; i < localBodies.Length; i++)
        {
            CelestialBody body = localBodies[i];
            UBIIdent[] idents = FetchUBIs(body);
            for (Int32 j = 0; j < idents.Length; j++)
            {
                if (idents[j].UBI != ubi)
                {
                    continue;
                }

                if (!idents[j].IsAbstract)
                {
                    bodies.Insert(0, body);
                }
                else
                {
                    bodies.Add(body);
                }
            }
        }

        // Fallback to the internal name for compatibility reasons
        for (Int32 i = 0; i < localBodies.Length; i++)
        {
            CelestialBody body = localBodies[i];
            if (body.transform.name == ubi)
            {
                bodies.Add(body);
            }
        }

        return bodies.ToArray();
    }

    /// <summary>
    /// Connects a UBI to a body. 
    /// </summary>
    public static void RegisterUBI(CelestialBody body, String ubi, Boolean isAbstract = false, CelestialBody[] localBodies = null)
    {
        if (String.IsNullOrEmpty(ubi) || !Parser.IsMatch(ubi))
        {
            throw new InvalidOperationException("The specified UBI is invalid! It needs to be SystemName/PlanetName.");
        }

        if (!isAbstract)
        {
            // The UBI isn't abstract, so it has to be unique. We don't have some kind of
            // web API to check it for the entire world, but we can check the local solar system
            CelestialBody[] bodies = GetBodies(ubi, localBodies);
            for (Int32 i = 0; i < bodies.Length; i++)
            {
                UBIIdent[] idents = FetchUBIs(bodies[i]);
                for (Int32 j = 0; j < idents.Length; j++)
                {
                    if (idents[j].UBI == ubi && !idents[j].IsAbstract)
                    {
                        throw new InvalidOperationException("The specified UBI already exists!");
                    }
                }
            }
        }

        // Check if the UBI is already assigned
        if (GetBodies(ubi, localBodies).Contains(body))
        {
            throw new InvalidOperationException("The specified UBI was already assigned to this body!");
        }

        // Check if the body already has a primary UBI
        if (FetchUBIs(body).All(u => u.IsAbstract) && isAbstract)
        {
            throw new InvalidOperationException("The body has no primary UBI assigned to it!");
        }

        // This is probably the most hacky way to do this, but still easier than to reflect and combine multiple 
        // UBIIdent Components from multiple assemblies
        String[] split = ubi.Split('/');
        GameObject ubiParent = body.gameObject.GetChild("UBI");
        if (!ubiParent)
        {
            ubiParent = new GameObject("UBI");
            ubiParent.transform.parent = body.transform;
        }
        GameObject ident = new GameObject(split[0] + ";" + split[1] + ";" + isAbstract);
        ident.transform.parent = ubiParent.transform;
    }

    /// <summary>
    /// Removes the connection between a UBI and a body
    /// </summary>
    public static void UnregisterUBI(CelestialBody body, String ubi)
    {
        if (String.IsNullOrEmpty(ubi) || !Parser.IsMatch(ubi))
        {
            throw new InvalidOperationException("The specified UBI is invalid! It needs to be SystemName/PlanetName.");
        }

        UBIIdent[] idents = FetchUBIs(body);
        for (Int32 i = 0; i < idents.Length; i++)
        {
            if (idents[i].UBI == ubi)
            {
                UnityEngine.Object.Destroy(idents[i].Object);
            }
        }
    }

    /// <summary>
    /// Gets the first UBI assigned to a body
    /// </summary>
    public static String GetUBI(CelestialBody body)
    {
        return GetUBIs(body).FirstOrDefault();
    }

    /// <summary>
    /// Gets all UBIs assigned to a body
    /// </summary>
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public static String[] GetUBIs(CelestialBody body)
    {
        List<String> idents = FetchUBIs(body).OrderBy(u => u.IsAbstract ? 1 : 0).Select(u => u.UBI).ToList();

        // Add the internal name at the end for compatibility reasons
        idents.Add(body.transform.name);

        return idents.ToArray();
    }

    /// <summary>
    /// Grabs a list of UBIIdents from a given body
    /// </summary>
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    private static UBIIdent[] FetchUBIs(CelestialBody body)
    {
        GameObject ubiParent = body.gameObject.GetChild("UBI");
        List<UBIIdent> idents = new List<UBIIdent>();
        if (!ubiParent)
        {
            return idents.ToArray();
        }

        foreach (Transform ident in ubiParent.transform)
        {
            String[] split = ident.name.Split(';');
            idents.Add(new UBIIdent
            {
                System = split[0],
                Body = split[1],
                IsAbstract = Boolean.Parse(split[2]),
                Object = ident.gameObject
            });
        }

        return idents.ToArray();
    }

    private struct UBIIdent
    {
        public String UBI
        {
            get { return System + "/" + Body; }
        }

        /// <summary>
        /// The name of the system the body is assigned to
        /// </summary>
        public String System;

        /// <summary>
        /// The name of the body
        /// </summary>
        public String Body;

        /// <summary>
        /// Whether this UBI is an abstract one. When this is set to false, the UBI must be unique in the loaded
        /// solar system, preferably unique in all planets ever made. If it is set to true the body implements the
        /// specified UBI and can take over configuration that was made for it.
        /// </summary>
        public Boolean IsAbstract;

        /// <summary>
        /// The game object that contains this UBI definition
        /// </summary>
        public GameObject Object;
    }

}
