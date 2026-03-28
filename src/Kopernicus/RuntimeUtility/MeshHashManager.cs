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
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Kopernicus.Constants;
using UnityEngine;

namespace Kopernicus.RuntimeUtility;

internal static class MeshHashManager
{
    private static readonly Dictionary<string, string> Hashes = new Dictionary<string, string>();

    private static string HashFilePath =>
        Path.Combine(KSPUtil.ApplicationRootPath, "GameData/Kopernicus/Cache/mesh.hash");

    public static void LoadHashes()
    {
        Hashes.Clear();

        if (!File.Exists(HashFilePath))
            return;

        try
        {
            foreach (var line in File.ReadAllLines(HashFilePath))
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                int spaceIndex = line.IndexOf(' ');
                if (spaceIndex <= 0 || spaceIndex >= line.Length - 1)
                    continue;

                string hash = line.Substring(0, spaceIndex);
                string path = line.Substring(spaceIndex + 1);
                Hashes[path] = hash;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Kopernicus] Failed to load mesh hashes: " + e.Message);
        }
    }

    public static void SaveHashes()
    {
        try
        {
            string directory = Path.GetDirectoryName(HashFilePath);
            if (directory != null)
                Directory.CreateDirectory(directory);

            StringBuilder sb = new StringBuilder();
            foreach (var (path, hash) in Hashes)
                sb.AppendLine(hash + " " + path);

            File.WriteAllText(HashFilePath, sb.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError("[Kopernicus] Failed to save mesh hashes: " + e.Message);
        }
    }

    public static string ComputeHash(ConfigNode pqsNode, ConfigNode templateNode, string bodyName, double radius, bool spherical)
    {
        string input =
            $"""
            Kopernicus: {Constants.Version.VersionNumber}
            Name: {bodyName}
            Radius: {radius.ToString(CultureInfo.InvariantCulture)}
            Spherical: {spherical}
            {pqsNode?.ToString() ?? ""}
            {templateNode?.ToString() ?? ""}
            """;

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string checksum = BitConverter.ToString(sha256.ComputeHash(bytes));
            checksum = checksum.Replace("-", "");
            checksum = checksum.ToLower();
            return checksum;
        }
    }

    public static bool IsValid(string relativeCachePath, string computedHash)
    {
        return Hashes.TryGetValue(relativeCachePath, out string storedHash) && storedHash == computedHash;
    }

    public static void SetHash(string relativeCachePath, string hash)
    {
        Hashes[relativeCachePath] = hash;
    }
}
