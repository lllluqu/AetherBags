using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Premade.Addon.Search;
using KamiToolKit.Nodes;

namespace AetherBags.Addons;

/// <summary>
/// Search picker addon that translates the KamiToolKit-provided chrome
/// (search placeholder, Cancel/Confirm buttons, sort tooltip) after setup.
/// </summary>
public abstract class LocalizedSearchAddon<T, TU> : BaseSearchAddon<T, TU>
    where TU : ListItemNode<T>, IListItemNode, new()
{
    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);
        KtkLocalizer.Apply(this);
    }
}
