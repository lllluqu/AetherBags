using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Premade.Addon.Search;
using KamiToolKit.Nodes;

namespace AetherBags.Addons;

/// <summary>
/// Item search picker that keeps KamiToolKit's item-search behaviour but
/// translates the generated chrome (search box, Cancel/Confirm, sort tooltip).
/// </summary>
public class LocalizedItemSearchAddon<T> : ItemSearchAddonBase<T>
    where T : ListItemNode<Lumina.Excel.Sheets.Item>, IListItemNode, new()
{
    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);
        KtkLocalizer.Apply(this);
    }
}
