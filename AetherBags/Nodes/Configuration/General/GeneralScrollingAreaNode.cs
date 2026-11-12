using System;
using System.Linq;
using System.Numerics;
using AetherBags.Configuration;
using AetherBags.Helpers;
using AetherBags.Nodes.Configuration.Layout;
using AetherBags.Nodes.Input;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace AetherBags.Nodes.Configuration.General;

public sealed class GeneralScrollingAreaNode : ScrollingListNode
{
    public GeneralScrollingAreaNode()
    {
        GeneralSettings config = System.Config.General;

        new ImportExportResetNode().AttachNode(this);

        ItemSpacing = 10;

        AddNode(new LabeledEnumDropdownNode<PluginLanguage>
        {
            Size = new Vector2(500, 20),
            LabelText = Loc.T("Language"),
            LabelTextFlags = TextFlags.AutoAdjustNodeSize,
            Options = Enum.GetValues<PluginLanguage>().ToList(),
            SelectedOption = config.Language,
            OnOptionSelected = selected =>
            {
                config.Language = selected;
                Loc.SetSetting(selected);
                Util.SaveConfig(System.Config);
                Services.ChatGui.Print(
                    Loc.T("UI language changed. Reopen the windows to apply."),
                    "AetherBags");
            }
        });

        AddNode(new FunctionalConfigurationNode());

        AddNode(new LayoutConfigurationNode(RecalculateLayout));

        AddNode(new DtrConfigurationNode());

        AddNode(new CheckboxNode
        {
            Size = new Vector2(300, 20),
            IsVisible = true,
            String = Loc.T("Debug Mode"),
            IsChecked = config.DebugEnabled,
            OnClick = isChecked =>
            {
                config.DebugEnabled = isChecked;
            }
        });
    }
}
