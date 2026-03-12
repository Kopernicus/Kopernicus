using System.Collections.Generic;

namespace Kopernicus.UI.MissingTexturesPopup;

internal static class MissingTextureLog
{
    internal struct Entry
    {
        public PSystemBody Body;
        public string TexturePath;
    }

    private static readonly List<Entry> _entries = new();

    internal static IReadOnlyList<Entry> Entries => _entries;

    internal static void Log(PSystemBody body, string texturePath)
    {
        _entries.Add(new Entry
        {
            Body = body,
            TexturePath = texturePath ?? "<empty>"
        });
    }

    internal static void Clear()
    {
        _entries.Clear();
    }

    internal static bool HasEntries => _entries.Count > 0;
}
