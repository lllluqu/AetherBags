using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Premade.Addon;

namespace AetherBags.Addons;

/// <summary>
/// Color picker addon that translates the KamiToolKit-provided buttons
/// (Confirm / Default / Cancel) after setup.
/// </summary>
public class LocalizedColorPickerAddon : ColorPickerAddon
{
    protected override unsafe void OnSetup(AtkUnitBase* addon, Span<AtkValue> atkValueSpan)
    {
        base.OnSetup(addon, atkValueSpan);
        KtkLocalizer.Apply(this);
    }
}
