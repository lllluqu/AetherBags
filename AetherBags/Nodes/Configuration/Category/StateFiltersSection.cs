using System;
using System.Collections.Generic;
using AetherBags.Configuration;

namespace AetherBags.Nodes.Configuration.Category;

public sealed class StateFiltersSection(Func<UserCategoryDefinition> getCategoryDefinition)
    : ConfigurationSection(getCategoryDefinition)
{
    private readonly List<(StateFilterRowNode Node, Func<UserCategoryDefinition, StateFilter> GetFilter)> _filters = [];
    private bool _initialized;

    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        AddFilter(Loc.T("Untradable"), def => def.Rules.Untradable);
        AddFilter(Loc.T("Unique"), def => def.Rules.Unique);
        AddFilter(Loc.T("Collectable"), def => def.Rules.Collectable);
        AddFilter(Loc.T("Dyeable"), def => def.Rules.Dyeable);
        AddFilter(Loc.T("Repairable"), def => def.Rules.Repairable);
        AddFilter(Loc.T("High Quality"), def => def.Rules.HighQuality);
        AddFilter(Loc.T("Desynthesizable"), def => def.Rules.Desynthesizable);
        AddFilter(Loc.T("Glamourable"), def => def.Rules.Glamourable);
        AddFilter(Loc.T("Spiritbonded"), def => def.Rules.FullySpiritbonded);

        RecalculateLayout();
    }

    private void AddFilter(string label, Func<UserCategoryDefinition, StateFilter> getFilter)
    {
        var node = new StateFilterRowNode(label, new StateFilter(), () => OnValueChanged?.Invoke());
        _filters.Add((node, getFilter));
        AddNode(node);
    }

    public override void Refresh()
    {
        EnsureInitialized();

        foreach (var (node, getFilter) in _filters)
        {
            node.SetState(getFilter(CategoryDefinition));
        }

        RecalculateLayout();
    }
}