using System;
using System.Numerics;
using AetherBags.Configuration;
using AetherBags.Nodes.Color;
using Dalamud.Utility;
using KamiToolKit.Nodes;

namespace AetherBags.Nodes.Configuration.Category;

public sealed class BasicSettingsSection(Func<UserCategoryDefinition> getCategoryDefinition) : ConfigurationSection(getCategoryDefinition)
{
    public Action? OnPropertyChanged { get; init; }

    private CheckboxNode? _enabledCheckbox;
    private CheckboxNode? _pinnedCheckbox;
    private TextInputNode? _nameInput;
    private TextInputNode? _descriptionInput;
    private ColorInputRow? _colorInput;
    private NumericInputNode? _priorityInput;
    private NumericInputNode? _orderInput;

    private bool _initialized;
    private bool _isRefreshing;

    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        _enabledCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = Loc.T("Enabled"),
            OnClick = isChecked =>
            {
                if (_isRefreshing) return;
                CategoryDefinition.Enabled = isChecked;
                OnPropertyChanged?.Invoke();
            },
        };
        AddNode(_enabledCheckbox);

        _pinnedCheckbox = new CheckboxNode
        {
            Size = new Vector2(Width, 20),
            String = Loc.T("Pinned"),
            OnClick = isChecked =>
            {
                if (_isRefreshing) return;
                CategoryDefinition.Pinned = isChecked;
                OnPropertyChanged?.Invoke();
            },
        };
        AddNode(_pinnedCheckbox);

        AddNode(CreateLabel(Loc.T("Name: ")));
        _nameInput = new TextInputNode
        {
            Size = new Vector2(250, 28),
            PlaceholderString = Loc.T("Category Name"),
            OnInputReceived = input =>
            {
                if (_isRefreshing) return;
                CategoryDefinition.Name = input.ExtractText();
                OnPropertyChanged?.Invoke();
            },
        };
        AddNode(_nameInput);

        AddNode(CreateLabel(Loc.T("Description:")));
        _descriptionInput = new TextInputNode
        {
            Size = new Vector2(250, 28),
            PlaceholderString = Loc.T("Optional description"),
            OnInputReceived = input =>
            {
                if (_isRefreshing) return;
                CategoryDefinition.Description = input.ExtractText();
                OnValueChanged?.Invoke();
            },
        };
        AddNode(_descriptionInput);

        _colorInput = new ColorInputRow
        {
            Label = Loc.T("Color"),
            Size = new Vector2(300, 28),
            CurrentColor = new UserCategoryDefinition().Color,
            DefaultColor = new UserCategoryDefinition().Color,
            OnColorConfirmed = color => { if (_isRefreshing) return; CategoryDefinition.Color = color; OnValueChanged?.Invoke(); },
            OnColorCanceled = color => { if (_isRefreshing) return; CategoryDefinition.Color = color; OnValueChanged?.Invoke(); },
            OnColorPreviewed = color => { if (_isRefreshing) return; CategoryDefinition.Color = color; OnValueChanged?.Invoke(); },
            OnColorChange = color => { if (_isRefreshing) return; CategoryDefinition.Color = color; OnValueChanged?.Invoke(); },
        };
        AddNode(_colorInput);

        AddNode(CreateLabel(Loc.T("Priority:")));
        _priorityInput = new NumericInputNode
        {
            Size = new Vector2(120, 28),
            Min = 0,
            Max = 1000,
            Step = 1,
            OnValueUpdate = value =>
            {
                if (_isRefreshing) return;
                CategoryDefinition.Priority = value;
                OnValueChanged?.Invoke();
            },
        };
        AddNode(_priorityInput);

        AddNode(CreateLabel(Loc.T("Order: ")));
        _orderInput = new NumericInputNode
        {
            Size = new Vector2(120, 28),
            Min = 0,
            Max = 9999,
            Step = 1,
            OnValueUpdate = val =>
            {
                if (_isRefreshing) return;
                CategoryDefinition.Order = val;
                OnPropertyChanged?.Invoke();
            },
        };
        AddNode(_orderInput);

        RecalculateLayout();
    }

    public override void Refresh()
    {
        EnsureInitialized();

        _isRefreshing = true;
        try
        {
            _enabledCheckbox!.IsChecked = CategoryDefinition.Enabled;
            _pinnedCheckbox!.IsChecked = CategoryDefinition.Pinned;
            _nameInput!.String = CategoryDefinition.Name;
            _nameInput.PlaceholderString = CategoryDefinition.Name.IsNullOrWhitespace() ? Loc.T("Category Name") : "";
            _descriptionInput!.String = CategoryDefinition.Description;
            _descriptionInput.PlaceholderString = CategoryDefinition.Description.IsNullOrWhitespace() ? Loc.T("Optional description") : "";
            _colorInput!.CurrentColor = CategoryDefinition.Color;
            _priorityInput!.Value = CategoryDefinition.Priority;
            _orderInput!.Value = CategoryDefinition.Order;
        }
        finally
        {
            _isRefreshing = false;
        }

        RecalculateLayout();
    }
}