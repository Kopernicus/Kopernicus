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
            Files:
            {CollectFileMTimes(pqsNode, templateNode)}
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

    private static string CollectFileMTimes(params ConfigNode[] nodes)
    {
        SortedDictionary<string, long> seen = new SortedDictionary<string, long>(StringComparer.Ordinal);
        foreach (ConfigNode node in nodes)
        {
            if (node == null)
                continue;
            CollectFileMTimes(node, seen);
        }

        StringBuilder sb = new StringBuilder();
        foreach (var entry in seen)
            sb.AppendLine($"{entry.Value.ToString(CultureInfo.InvariantCulture)} {entry.Key}");
        return sb.ToString();
    }

    private static void CollectFileMTimes(ConfigNode node, SortedDictionary<string, long> seen)
    {
        foreach (ConfigNode.Value value in node.values)
        {
            string url = value.value;
            if (string.IsNullOrEmpty(url))
                continue;

            foreach (string path in ResolveReferencedFiles(url))
            {
                if (seen.ContainsKey(path))
                    continue;
                try
                {
                    seen[path] = File.GetLastWriteTimeUtc(path).Ticks;
                }
                catch
                {
                }
            }
        }

        foreach (ConfigNode child in node.nodes)
            CollectFileMTimes(child, seen);
    }

    private static IEnumerable<string> ResolveReferencedFiles(string url)
    {
        if (url.StartsWith("BUILTIN/", StringComparison.Ordinal))
            yield break;
        if (url.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            yield break;

        string combined;
        try
        {
            combined = Path.Combine(KSPUtil.ApplicationRootPath, "GameData", url);
        }
        catch
        {
            yield break;
        }

        if (File.Exists(combined))
        {
            yield return combined;
            yield break;
        }

        // Extensionless KSP convention: value names the file without its
        // extension. Match anything in the parent directory with that base.
        string dir = Path.GetDirectoryName(combined);
        string baseName = Path.GetFileName(combined);
        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(baseName))
            yield break;
        if (!Directory.Exists(dir))
            yield break;

        string[] matches;
        try
        {
            matches = Directory.GetFiles(dir, baseName + ".*");
        }
        catch
        {
            yield break;
        }
        foreach (string match in matches)
            yield return match;
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
