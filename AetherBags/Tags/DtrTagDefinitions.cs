using System.Collections.Generic;
using System.Linq;

namespace AetherBags.Tags;

public static class DtrTagDefinitions
{
    public const string DefaultDropdownLabel = "Insert Tag...";

    public static string DefaultLabel => Loc.T(DefaultDropdownLabel);

    private static readonly (string Label, string Tag)[] Definitions =
    [
        (DefaultDropdownLabel, string.Empty),
        ("Main: Used Slots", "[used]"),
        ("Main: Total Slots", "[total]"),
        ("Main: Free Slots", "[free]"),
        ("Main: Usage Percent", "[percent:0]%"),
        ("Main: Item Stacks", "[items]"),
        ("Main: Item Quantity", "[quantity]"),
        ("Main: Categories", "[categories]"),
        ("Saddlebag: Used Slots", "[saddle.used]"),
        ("Saddlebag: Total Slots", "[saddle.total]"),
        ("Saddlebag: Free Slots", "[saddle.free]"),
        ("Saddlebag: Usage Percent", "[saddle.percent:0]%"),
    ];

    private static Dictionary<string, string>? _templates;
    private static bool _templatesChinese;

    public static IReadOnlyDictionary<string, string> Templates => GetTemplates();

    public static List<string> GetLabels() => Templates.Keys.ToList();

    private static Dictionary<string, string> GetTemplates()
    {
        if (_templates is not null && _templatesChinese == Loc.IsChinese)
            return _templates;

        var templates = new Dictionary<string, string>(Definitions.Length);
        foreach (var (label, tag) in Definitions)
            templates[Loc.T(label)] = tag;

        _templates = templates;
        _templatesChinese = Loc.IsChinese;
        return templates;
    }
}
