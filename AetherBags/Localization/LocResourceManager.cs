using System.Globalization;
using System.Resources;

namespace AetherBags.Localization;

/// <summary>
/// Bridges our zh-CN dictionary into KamiToolKit's built-in enum description
/// localization hook, so KTK-rendered dropdown options ("Quantity", "Name",
/// "Filter (Hide non-matches)", …) follow the selected plugin language.
/// </summary>
internal sealed class LocResourceManager : ResourceManager
{
    public static readonly LocResourceManager Instance = new();

    private LocResourceManager()
    {
    }

    public override string? GetString(string name)
        => Loc.TryGet(name, out var translated) ? translated : null;

    public override string? GetString(string name, CultureInfo? culture)
        => GetString(name);
}
