using System.Numerics;
using AetherBags.Addons;
using AetherBags.Commands;
using AetherBags.Helpers;
using AetherBags.Hooks;
using AetherBags.Inventory.Context;
using AetherBags.IPC;
using AetherBags.IPC.AetherBagsAPI;
using AetherBags.Monitoring;
using Dalamud.Plugin;
using KamiToolKit;

namespace AetherBags;

public class Plugin : IDalamudPlugin
{
    private readonly CommandHandler _commandHandler;
    private readonly InventoryHooks _inventoryHooks;
    private readonly GlobalKeybindHandler _globalKeybindHandler;
    private readonly InventoryMonitor inventoryMonitor;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Services>();

        System.Config = Util.LoadConfigOrDefault();

        Loc.Initialize(System.Config.General.Language);

        BackupHelper.DoConfigBackup(pluginInterface);

        KamiToolKitLibrary.Initialize(pluginInterface);
        KamiToolKitLibrary.SetResourceManager(LocResourceManager.Instance);
        KamiToolKitLibrary.SetCurrentCulture(global::System.Globalization.CultureInfo.GetCultureInfo("zh-CN"));

        System.IPC = new IPCService();
        System.IPC.RefreshExternalSources();
        ItemContextMenuHandler.Initialize();
        System.LootedItemsTracker = new LootedItemsTracker();

        System.AddonInventoryWindow = new AddonInventoryWindow
        {
            InternalName = "AetherBagsCN_MainBags",
            Title = "AetherBags",
            Size = new Vector2(750, 750),
        };

        System.AddonSaddleBagWindow = new AddonSaddleBagWindow
        {
            InternalName = "AetherBagsCN_SaddleBag",
            Title = Loc.T("AetherSaddlebag"),
            Size = new Vector2(750, 750),
        };

        System.AddonRetainerWindow = new AddonRetainerWindow
        {
            InternalName = "AetherBagsCN_Retainer",
            Title = Loc.T("AetherRetainerbag"),
            Size = new Vector2(750, 750),
        };

        System.AddonConfigurationWindow = new AddonConfigurationWindow
        {
            InternalName = "AetherBagsCN_Config",
            Title = Loc.T("AetherBags Config"),
            Size = new Vector2(640, 512),
        };

        System.AddonChangelogWindow = new AddonChangelogWindow
        {
            InternalName = "AetherBagsCN_Changelog",
            Title = Loc.T("AetherBags Changelog"),
            Size = new Vector2(640, 512),
        };

        Services.PluginInterface.UiBuilder.OpenMainUi += System.AddonInventoryWindow.Toggle;
        Services.PluginInterface.UiBuilder.OpenConfigUi += System.AddonConfigurationWindow.Toggle;

        System.AetherBagsAPI = new AetherBagsIPCProvider();

        _commandHandler = new CommandHandler();

        Services.ClientState.Login += OnLogin;
        Services.ClientState.Logout += OnLogout;

        if (Services.ClientState.IsLoggedIn) {
            Services.Framework.RunOnFrameworkThread(OnLogin);
        }

        _inventoryHooks = new InventoryHooks();
        _globalKeybindHandler = new GlobalKeybindHandler();
        System.DtrService = new DtrService();
        inventoryMonitor = new InventoryMonitor();
    }

    public void Dispose()
    {
        RegexCache.Clear();

        InventoryAddonContextMenu.Close();
        ItemContextMenuHandler.Dispose();
        _inventoryHooks.Dispose();
        _globalKeybindHandler.Dispose();
        System.DtrService.Dispose();
        inventoryMonitor.Dispose();

        System.LootedItemsTracker.Dispose();
        System.AetherBagsAPI?.Dispose();
        System.IPC.Dispose();
        HighlightState.ClearAll();

        System.AddonInventoryWindow.Dispose();
        System.AddonSaddleBagWindow.Dispose();
        System.AddonRetainerWindow.Dispose();
        System.AddonConfigurationWindow.Dispose();
        System.AddonChangelogWindow.Dispose();

        Util.SaveConfig(System.Config);
        KamiToolKitLibrary.Dispose();
    }

    private void OnLogin()
    {
        System.Config = Util.LoadConfigOrDefault();
        Loc.SetSetting(System.Config.General.Language);
        System.IPC.RefreshExternalSources();
        System.LootedItemsTracker.Enable();

        System.AddonInventoryWindow.DebugOpen();
        System.AddonConfigurationWindow.DebugOpen();
    }

    private void OnLogout(int type, int code)
    {
        Util.SaveConfig(System.Config);
        System.LootedItemsTracker.Disable();
        System.AddonInventoryWindow.Close();
        System.AddonSaddleBagWindow.Close();
        System.AddonRetainerWindow.Close();
        System.AddonConfigurationWindow.Close();
        System.AddonChangelogWindow.Close();
    }
}