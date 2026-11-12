namespace AetherBags.Configuration;

public class DtrSettings
{
    public static readonly string DefaultFormatString = Loc.T("Bags [used]/[total] ([percent:0]%)");

    public bool Enabled { get; set; } = true;
    public string FormatString { get; set; } = DefaultFormatString;
}
