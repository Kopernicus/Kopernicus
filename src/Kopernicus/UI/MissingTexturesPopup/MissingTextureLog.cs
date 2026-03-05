using System.Collections.Generic;

namespace Kopernicus.UI.MissingTexturesPopup;

internal static class MissingTextureLog
{
    internal struct Entry
    {
        public string BodyName;
        public string TexturePath;
    }

    private static readonly List<Entry> _entries = new();

    internal static IReadOnlyList<Entry> Entries => _entries;

    internal static void Log(string bodyName, string texturePath)
    {
        _entries.Add(new Entry
        {
            BodyName = bodyName ?? "<unknown>",
            TexturePath = texturePath ?? "<empty>"
        });
    }

    internal static void Clear()
    {
        _entries.Clear();
    }

    internal static bool HasEntries => _entries.Count > 0;
}
