using System;
using System.Collections.Generic;
using AetherBags.Addons;
using AetherBags.Helpers;
using AetherBags.Inventory;
using AetherBags.Inventory.Items;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AetherBags.Commands;

public class CommandHandler : IDisposable
{
    private const string MainCommand = "/aetherbags";
    private const string ShortCommand = "/ab";
    private static readonly string HelpDescription = Loc.T("Opens your inventory. Use '/ab help' for more options.");
    private string? _debugVanillaBypassToken;

    public CommandHandler()
    {
        Services.CommandManager.AddHandler(MainCommand, new CommandInfo(OnCommand)
        {
            DisplayOrder = 1,
            ShowInHelp = true,
            HelpMessage = HelpDescription
        });

        Services.CommandManager.AddHandler(ShortCommand, new CommandInfo(OnCommand)
        {
            DisplayOrder = 2,
            ShowInHelp = true,
            HelpMessage = HelpDescription
        });
    }

    private void OnCommand(string command, string args)
    {
        var argsParts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var subCommand = argsParts.Length > 0 ? argsParts[0].ToLowerInvariant() : string.Empty;
        var subArgs = argsParts.Length > 1 ? argsParts[1] : string.Empty;

        switch (subCommand)
        {
            case "":
            case "toggle":
                System.AddonInventoryWindow.Toggle();
                break;

            case "config":
            case "settings":
                System.AddonConfigurationWindow.Toggle();
                break;

            case "show":
            case "open":
                System.AddonInventoryWindow.Open();
                break;

            case "hide":
            case "close":
                System.AddonInventoryWindow.Close();
                break;

            case "search":
                HandleSearch(subArgs);
                break;

            case "import-sk":
                ImportExportResetHelper.TryImportSortaKindaFromClipboard(true);
                InventoryOrchestrator.RefreshAll(updateMaps: true);
                break;

            case "export":
                HandleExport();
                break;

            case "export-categories":
                ImportExportResetHelper.TryExportCategoriesToClipboard();
                break;

            case "import-categories":
                ImportExportResetHelper.TryImportCategoriesFromClipboard(true);
                break;

            case "import":
                ImportExportResetHelper.TryImportConfigFromClipboard();
                InventoryOrchestrator.RefreshAll(updateMaps: true);
                break;

            case "reset":
                ImportExportResetHelper.TryResetConfig();
                InventoryOrchestrator.RefreshAll(updateMaps: true);
                break;

            case "count":
            case "stats":
                PrintInventoryStats();
                break;

            case "debug":
                HandleDebug(subArgs);
                break;

            case "help":
            case "?":
                PrintHelp();
                break;

            case "list":
            case "items":
                ListInventoryItems(subArgs);
                break;

            default:
                PrintChat(Loc.T("Unknown command: {0}. Use '/ab help' for available commands.", subCommand));
                break;
        }
    }

    private void HandleDebug(string args)
    {
        if (System.Config?.General?.DebugEnabled != true)
        {
            PrintChat(Loc.T("Debug commands are disabled. Enable 'General.DebugEnabled' in your config to use debug commands."));
            return;
        }

        var parts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts.Length > 0 ? parts[0].ToLowerInvariant() : string.Empty;
        var subArgs = parts.Length > 1 ? parts[1] : string.Empty;

        switch (cmd)
        {
            case "":
            case "help":
                PrintChat(Loc.T("Debug commands:\n  /ab debug saddle                  - Toggle saddlebag window\n  /ab debug retainer                - Toggle retainer window\n  /ab debug changelog               - Toggle changelog window\n  /ab debug test [arg]              - Test IPC source (toggle/on/off/refresh/status)\n  /ab debug batching [arg]          - Frame batching (toggle/on/off/status)\n  /ab debug vanilla-bypass [arg]    - Vanilla inventory bypass (status/acquire/release)\n  /ab debug help                    - Show this message"));
                break;

            case "saddle":
                System.AddonSaddleBagWindow.Toggle();
                break;

            case "retainer":
                System.AddonRetainerWindow.Toggle();
                break;

            case "changelog":
                System.AddonChangelogWindow.Toggle();
                break;

            case "test":
                HandleTestSource(subArgs);
                break;

            case "batching":
                HandleBatching(subArgs);
                break;

            case "vanilla-bypass":
            case "bypass":
                HandleVanillaInventoryBypass(subArgs);
                break;

            case "refresh":
                InventoryOrchestrator.RefreshAll(updateMaps: true);
                PrintChat(Loc.T("Inventory refreshed."));
                break;

            default:
                PrintChat(Loc.T("Unknown debug command: {0}. Use '/ab debug help' for debug commands.", cmd));
                break;
        }
    }

    private void PrintInventoryStats()
    {
        var openWindows = new List<(string Name, IInventoryWindow Window)>();

        if (System.AddonInventoryWindow.IsOpen)
            openWindows.Add((Loc.T("Main"), System.AddonInventoryWindow));
        if (System.AddonSaddleBagWindow.IsOpen)
            openWindows.Add((Loc.T("Saddle"), System.AddonSaddleBagWindow));
        if (System.AddonRetainerWindow.IsOpen)
            openWindows.Add((Loc.T("Retainer"), System.AddonRetainerWindow));

        if (openWindows.Count == 0)
        {
            PrintChat(Loc.T("No inventory windows are open. Open an inventory to see stats."));
            return;
        }

        foreach (var (name, window) in openWindows)
        {
            var stats = window.GetStats();
            PrintChat(Loc.T("[{0}] {1}/{2} slots ({3:F0}%) | {4} items | {5} categories", name, stats.UsedSlots, stats.TotalSlots, stats.UsagePercent, stats.TotalItems, stats.CategoryCount));
        }

        if (openWindows.Count > 1)
        {
            var combined = new InventoryStats();
            foreach (var (_, window) in openWindows)
            {
                combined += window.GetStats();
            }
            PrintChat(Loc.T("[Total] {0}/{1} slots ({2:F0}%) | {3} items | {4} categories", combined.UsedSlots, combined.TotalSlots, combined.UsagePercent, combined.TotalItems, combined.CategoryCount));
        }
    }

    private void HandleSearch(string searchTerm)
    {
        if (!System.AddonInventoryWindow.IsOpen)
        {
            System.AddonInventoryWindow.Open();
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            System.AddonInventoryWindow.SetSearchText(searchTerm);
        }

        PrintChat(Loc.T("Searching for: {0}", searchTerm));
    }

    private void HandleExport()
    {
        ImportExportResetHelper.TryExportConfigToClipboard(System.Config);
    }

    private void ListInventoryItems(string args)
    {
        if (!System.AddonInventoryWindow.IsOpen)
        {
            PrintChat(Loc.T("Open your inventory first."));
            return;
        }

        var categories = System.AddonInventoryWindow.GetVisibleCategories();
        if (categories == null || categories.Count == 0)
        {
            PrintChat(Loc.T("No items found."));
            return;
        }

        var subCmd = args.Trim().ToLowerInvariant();

        if (subCmd is "ids" or "copy")
        {
            var ids = new List<uint>();
            foreach (var category in categories)
            {
                foreach (var item in category.Items)
                {
                    if (!ids.Contains(item.Item.ItemId))
                        ids.Add(item.Item.ItemId);
                }
            }
            var idsString = string.Join(", ", ids);
            ImGui.SetClipboardText(idsString);
            PrintChat(Loc.T("Copied {0} unique item IDs to clipboard.", ids.Count));
            return;
        }

        if (subCmd is "full" or "all")
        {
            var lines = new List<string>();
            foreach (var category in categories)
            {
                foreach (var item in category.Items)
                {
                    lines.Add($"{item.Item.ItemId}: {item.Name} x{item.ItemCount}");
                }
            }
            var fullText = string.Join("\n", lines);
            ImGui.SetClipboardText(fullText);
            PrintChat(Loc.T("Copied {0} items to clipboard.", lines.Count));
            return;
        }

        int limit = 20;
        if (!string.IsNullOrWhiteSpace(args) && int.TryParse(args.Trim(), out int parsed))
        {
            limit = parsed;
        }

        PrintChat(Loc.T("Listing up to {0} items (use '/ab list ids' to copy all IDs):", limit));

        int count = 0;
        foreach (var category in categories)
        {
            foreach (var item in category.Items)
            {
                if (count >= limit) break;
                PrintChat($"  {item.Item.ItemId}: {item.Name} x{item.ItemCount}");
                count++;
            }
            if (count >= limit) break;
        }

        PrintChat(Loc.T("Shown {0} items. Total categories: {1}", count, categories.Count));
    }

    private void HandleBatching(string args)
    {
        var subCmd = args.Trim().ToLowerInvariant();
        var config = System.Config.General;

        switch (subCmd)
        {
            case "":
            case "toggle":
                config.FrameBatchingEnabled = !config.FrameBatchingEnabled;
                break;

            case "on":
            case "enable":
                config.FrameBatchingEnabled = true;
                break;

            case "off":
            case "disable":
                config.FrameBatchingEnabled = false;
                break;

            case "status":
                PrintChat(Loc.T("Frame batching is {0}.", config.FrameBatchingEnabled ? Loc.T("enabled") : Loc.T("DISABLED")));
                return;

            default:
                PrintChat(Loc.T("Usage: /ab debug batching [toggle|on|off|status]\n  on  = batching enabled (default, items spread across frames)\n  off = batching disabled (all items populated in one frame, for profiling)"));
                return;
        }

        Util.SaveConfig(System.Config);
        PrintChat(Loc.T("Frame batching is now {0}. Refreshing inventory…", config.FrameBatchingEnabled ? Loc.T("enabled") : Loc.T("DISABLED")));
        InventoryOrchestrator.RefreshAll(updateMaps: true);
    }

    private void HandleVanillaInventoryBypass(string args)
    {
        var api = System.AetherBagsAPI?.API;
        if (api == null)
        {
            PrintChat(Loc.T("AetherBags API is not available."));
            return;
        }

        var parts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var subCmd = parts.Length > 0 ? parts[0].ToLowerInvariant() : "status";
        var subArgs = parts.Length > 1 ? parts[1] : string.Empty;

        switch (subCmd)
        {
            case "":
            case "status":
                PrintChat(api.GetVanillaInventoryBypassStatus());
                break;

            case "on":
            case "acquire":
            case "enable":
                AcquireDebugVanillaBypass(ParseBypassTimeout(subArgs));
                break;

            case "off":
            case "release":
            case "disable":
                ReleaseDebugVanillaBypass();
                break;

            default:
                if (int.TryParse(subCmd, out int timeoutMs))
                {
                    AcquireDebugVanillaBypass(timeoutMs);
                    break;
                }

                PrintChat(Loc.T("Usage: /ab debug vanilla-bypass [status|acquire [ms]|release|<ms>]\n  Example: /ab debug vanilla-bypass 30000"));
                break;
        }
    }

    private void AcquireDebugVanillaBypass(int timeoutMs)
    {
        var api = System.AetherBagsAPI?.API;
        if (api == null)
        {
            PrintChat(Loc.T("AetherBags API is not available."));
            return;
        }

        if (!string.IsNullOrWhiteSpace(_debugVanillaBypassToken))
            api.ReleaseVanillaInventoryBypass(_debugVanillaBypassToken);

        _debugVanillaBypassToken = api.AcquireVanillaInventoryBypass("AetherBags Debug Command", timeoutMs);
        PrintChat(Loc.T("Vanilla inventory bypass acquired for {0}ms. Token: {1}", timeoutMs, _debugVanillaBypassToken));
    }

    private void ReleaseDebugVanillaBypass()
    {
        if (string.IsNullOrWhiteSpace(_debugVanillaBypassToken))
        {
            PrintChat(Loc.T("No debug vanilla inventory bypass token is active in this session."));
            return;
        }

        bool released = System.AetherBagsAPI?.API.ReleaseVanillaInventoryBypass(_debugVanillaBypassToken) ?? false;
        PrintChat(released ? Loc.T("Debug vanilla inventory bypass released.") : Loc.T("Debug vanilla inventory bypass token was already expired or released."));
        _debugVanillaBypassToken = null;
    }

    private static int ParseBypassTimeout(string args)
    {
        return int.TryParse(args.Trim(), out int timeoutMs) ? timeoutMs : 30_000;
    }

    private void HandleTestSource(string args)
    {
        var testSource = System.IPC?.TestSource;
        if (testSource == null)
        {
            PrintChat(Loc.T("Test source not available."));
            return;
        }

        var subCmd = args.Trim().ToLowerInvariant();

        switch (subCmd)
        {
            case "":
            case "toggle":
                if (testSource.IsReady)
                {
                    testSource.Disable();
                    PrintChat(Loc.T("Test source disabled."));
                }
                else
                {
                    testSource.Enable();
                    PrintChat(Loc.T("Test source enabled. Search 'test' to find items."));
                }
                InventoryOrchestrator.RefreshAll(updateMaps: true);
                break;

            case "on":
            case "enable":
                testSource.Enable();
                PrintChat(Loc.T("Test source enabled. Search 'test' to find items."));
                InventoryOrchestrator.RefreshAll(updateMaps: true);
                break;

            case "off":
            case "disable":
                testSource.Disable();
                PrintChat(Loc.T("Test source disabled."));
                InventoryOrchestrator.RefreshAll(updateMaps: true);
                break;

            case "refresh":
                testSource.Refresh();
                PrintChat(Loc.T("Test source refreshed."));
                InventoryOrchestrator.RefreshAll(updateMaps: true);
                break;

            case "status":
                PrintChat(Loc.T("Test source is {0}.", testSource.IsReady ? Loc.T("enabled") : Loc.T("disabled")));
                break;

            default:
                PrintChat(Loc.T("Usage: /ab test [toggle|on|off|refresh|status]"));
                break;
        }
    }

    private void PrintHelp()
    {
        var commands = new (string Command, string Description)[]
        {
            ("/ab", Loc.T("Toggle inventory window")),
            ("/ab config", Loc.T("Open configuration window")),
            ("/ab show", Loc.T("Open inventory window")),
            ("/ab hide", Loc.T("Close inventory window")),

            ("/ab search <term>", Loc.T("Open inventory and search for items")),

            ("/ab list", Loc.T("List items currently visible in inventory")),
            ("/ab list <n>", Loc.T("List first N items")),
            ("/ab list ids", Loc.T("Copy unique item IDs to clipboard")),
            ("/ab list full", Loc.T("Copy full item list to clipboard")),

            ("/ab stats", Loc.T("Show inventory statistics")),

            ("/ab import", Loc.T("Import config from clipboard (hold Shift)")),
            ("/ab import-sk", Loc.T("Import SortaKinda clipboard config")),
            ("/ab export", Loc.T("Export config to clipboard")),
            ("/ab export-categories", Loc.T("Export all categories to clipboard")),
            ("/ab import-categories", Loc.T("Import categories from clipboard (replaces existing)")),
            ("/ab reset", Loc.T("Reset config to default")),

            ("/ab help", Loc.T("Show this help message")),
        };

        var lines = new List<string> { Loc.T("AetherBags Commands:") };

        foreach (var (cmd, desc) in commands)
            lines.Add($"  {cmd}  - {desc}");

        lines.Add(System.Config?.General?.DebugEnabled == true
            ? Loc.T("  /ab debug [saddle|retainer|test|refresh]  - Debug commands")
            : Loc.T("  /ab debug  - Debug-only commands (enable in config)"));

        PrintChat(string.Join("\n", lines));
    }

    private static void PrintChat(string message)
    {
        Services.ChatGui.Print(message, "AetherBags");
    }

    public void Dispose()
    {
        Services.CommandManager.RemoveHandler(MainCommand);
        Services.CommandManager.RemoveHandler(ShortCommand);
    }
}