using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Utility;
using KamiToolKit;
using KamiToolKit.Nodes;

namespace AetherBags.Localization;

/// <summary>
/// KamiToolKit ships a few hard-coded English labels (picker confirm/cancel buttons,
/// search placeholders, "Edit"/"Remove" list buttons…). Those widgets are built inside
/// KTK and expose no localization hook, so after an addon has been set up we walk its
/// node tree and translate any text node whose contents are a known dictionary key.
/// </summary>
internal static class KtkLocalizer
{
    private const int MaxDepth = 64;

    private static readonly FieldInfo? ChildNodesField =
        typeof(NodeBase).GetField("ChildNodes", BindingFlags.Instance | BindingFlags.NonPublic);

    private static bool _reportedFailure;

    public static void Apply(NativeAddon? addon)
    {
        if (addon is null || !Loc.IsChinese)
            return;

        var root = addon.RootNode;
        if (root is null)
            return;

        try
        {
            Walk(root, 0);
        }
        catch (Exception ex)
        {
            if (_reportedFailure)
                return;

            _reportedFailure = true;
            Services.Logger?.Debug(ex, "[AetherBags] KamiToolKit localization pass failed.");
        }
    }

    private static void Walk(NodeBase node, int depth)
    {
        if (depth > MaxDepth)
            return;

        if (node is TextNode textNode)
        {
            var current = textNode.String.ExtractText();
            if (!string.IsNullOrEmpty(current) && Loc.TryGet(current, out var translated))
                textNode.String = translated;
        }

        if (ChildNodesField?.GetValue(node) is List<NodeBase> children)
        {
            foreach (var child in children.ToArray())
                Walk(child, depth + 1);
        }
    }
}
