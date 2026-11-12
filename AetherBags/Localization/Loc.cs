using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AetherBags.Localization;

/// <summary>
/// Which language the plugin UI should be rendered in.
/// </summary>
public enum PluginLanguage
{
    /// <summary>Follow the game client / Dalamud UI language.</summary>
    [global::System.ComponentModel.DescriptionAttribute("Auto (Follow Client)")]
    Auto = 0,

    /// <summary>Force Simplified Chinese.</summary>
    [global::System.ComponentModel.DescriptionAttribute("Simplified Chinese")]
    Chinese = 1,

    /// <summary>Force English.</summary>
    English = 2,
}

/// <summary>
/// Tiny runtime localization layer. English source text is used as the lookup key so that
/// every call site stays readable and untranslated strings gracefully fall back to English.
/// </summary>
public static class Loc
{
    private const string ResourceName = "AetherBags.Localization.zh-CN.json";

    private static Dictionary<string, string> _zh = new(StringComparer.Ordinal);
    private static bool _loaded;
    private static readonly HashSet<string> MissingKeys = new(StringComparer.Ordinal);

    /// <summary>Configured language mode.</summary>
    public static PluginLanguage Setting { get; private set; } = PluginLanguage.Auto;

    /// <summary>True when the plugin should currently render Simplified Chinese.</summary>
    public static bool IsChinese { get; private set; }

    /// <summary>Raised whenever the effective language may have changed.</summary>
    public static event global::System.Action? LanguageChanged;

    /// <summary>Number of distinct strings that were requested but have no zh-CN translation.</summary>
    public static int MissingKeyCount => MissingKeys.Count;

    public static void Initialize(PluginLanguage setting)
    {
        EnsureInitialized();
        Setting = setting;
        Apply();
    }

    /// <summary>
    /// Loads the dictionary and resolves the language on first use, so static initializers
    /// that run before plugin startup still receive translated text.
    /// </summary>
    private static void EnsureInitialized()
    {
        if (_loaded)
            return;

        LoadDictionary();
        IsChinese = Setting == PluginLanguage.Chinese ||
                    (Setting == PluginLanguage.Auto && DetectChineseClient());
    }

    public static void SetSetting(PluginLanguage setting)
    {
        Setting = setting;
        Apply();
    }

    private static void Apply()
    {
        var chinese = Setting switch
        {
            PluginLanguage.Chinese => true,
            PluginLanguage.English => false,
            _ => DetectChineseClient(),
        };

        if (chinese == IsChinese && _loaded)
        {
            // Still raise once during initial setup so windows built afterwards use the value.
            LanguageChanged?.Invoke();
            return;
        }

        IsChinese = chinese;
        Services.Logger?.Debug($"[AetherBags] UI language: {(IsChinese ? "Chinese (zh-CN)" : "English")}");
        LanguageChanged?.Invoke();
    }

    /// <summary>Translates a single string. Unknown strings are returned unchanged.</summary>
    public static string T(string english)
    {
        EnsureInitialized();

        if (!IsChinese || string.IsNullOrEmpty(english))
            return english;

        if (_zh.TryGetValue(english, out var translated))
            return translated;

        if (MissingKeys.Add(english))
            Services.Logger?.Debug($"[AetherBags] Missing zh-CN translation: {english}");

        return english;
    }

    /// <summary>Translates a composite format string and applies <paramref name="args"/>.</summary>
    public static string T(string english, params object?[] args)
    {
        var format = T(english);
        if (args.Length == 0)
            return format;

        try
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
        catch (FormatException)
        {
            return format;
        }
    }

    private static void LoadDictionary()
    {
        if (_loaded)
            return;

        _loaded = true;

        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
            if (stream is null)
            {
                Services.Logger?.Warning($"[AetherBags] Localization resource '{ResourceName}' not found.");
                return;
            }

            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
            if (parsed is null || parsed.Count == 0)
            {
                Services.Logger?.Warning("[AetherBags] Localization resource is empty.");
                return;
            }

            _zh = new Dictionary<string, string>(parsed, StringComparer.Ordinal);
            Services.Logger?.Debug($"[AetherBags] Loaded {_zh.Count} zh-CN strings.");
        }
        catch (Exception ex)
        {
            Services.Logger?.Error(ex, "[AetherBags] Failed to load zh-CN localization.");
        }
    }

    private static bool DetectChineseClient()
    {
        try
        {
            var uiLanguage = Services.PluginInterface.UiLanguage;
            if (!string.IsNullOrEmpty(uiLanguage) &&
                uiLanguage.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        catch
        {
            // ignored, fall through to the next signal
        }

        try
        {
            // Works for both global and CN Dalamud builds regardless of the enum's members.
            if (Services.ClientState.ClientLanguage.ToString()
                .Contains("Chinese", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        catch
        {
            // ignored
        }

        try
        {
            var sample = Services.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Item>().GetRow(1).Name.ToString();
            return ContainsCjk(sample);
        }
        catch
        {
            return false;
        }
    }

    private static bool ContainsCjk(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        foreach (var c in text)
        {
            if (c >= '\u4E00' && c <= '\u9FFF')
                return true;
        }

        return false;
    }
}


