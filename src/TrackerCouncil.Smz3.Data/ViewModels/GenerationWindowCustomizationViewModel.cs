using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Interface")]
[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Interface Top", parentGroup: "Interface")]
[DynamicFormGroupBasic(DynamicFormLayout.TwoColumns, "Interface Bottom", parentGroup: "Interface")]
[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Metroid Control Settings")]
[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Metroid Control Settings Top", parentGroup: "Metroid Control Settings")]
[DynamicFormGroupBasic(DynamicFormLayout.TwoColumns, "Metroid Control Settings Bottom", parentGroup: "Metroid Control Settings")]
[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Metroid Button Mappings")]
[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "Metroid Button Mappings Top", parentGroup: "Metroid Button Mappings")]
[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Metroid Button Mappings Bottom", parentGroup: "Metroid Button Mappings")]
public class GenerationWindowCustomizationViewModel : ViewModelBase
{
    private RunButtonBehavior _runButtonBehavior;
    private ItemCancelBehavior _itemCancelBehavior;
    private AimButtonBehavior _aimButtonBehavior;
    private MetroidButton _shootButton = MetroidButton.A;
    private MetroidButton _jumpButton = MetroidButton.B;
    private MetroidButton _dashButton = MetroidButton.L;
    private MetroidButton _itemSelectButton = MetroidButton.R;
    private MetroidButton _itemCancelButton = MetroidButton.Select;
    private MetroidButton _aimUpButton = MetroidButton.X;
    private MetroidButton _aimDownButton = MetroidButton.Y;

    [DynamicFormFieldComboBox(label: "Heart color:", groupName: "Interface Top")]
    public HeartColor HeartColor { get; set; }

    [DynamicFormFieldComboBox(label: "ALttP menu speed:", groupName: "Interface Top")]
    public MenuSpeed MenuSpeed { get; set; }

    [DynamicFormFieldComboBox(label: "Low health beep speed:", groupName: "Interface Top")]
    public LowHealthBeepSpeed LowHealthBeepSpeed { get; set; }

    [DynamicFormFieldCheckBox("Disable low energy beep in Super Metroid", groupName: "Interface Bottom")]
    public bool DisableLowEnergyBeep { get; set; }

    [DynamicFormFieldComboBox(label: "Run button behavior:", groupName: "Metroid Control Settings Top")]
    public RunButtonBehavior RunButtonBehavior
    {
        get => _runButtonBehavior;
        set
        {
            SetField(ref _runButtonBehavior, value);
            OnPropertyChanged(nameof(RunButtonLabel));
        }
    }

    [DynamicFormFieldComboBox(label: "Item cancel behavior:", groupName: "Metroid Control Settings Top")]
    public ItemCancelBehavior ItemCancelBehavior
    {
        get => _itemCancelBehavior;
        set
        {
            SetField(ref _itemCancelBehavior, value);
            OnPropertyChanged(nameof(ItemCancelLabel));
        }
    }

    [DynamicFormFieldComboBox(label: "Aim button behavior:", groupName: "Metroid Control Settings Top")]
    public AimButtonBehavior AimButtonBehavior
    {
        get => _aimButtonBehavior;
        set
        {
            SetField(ref _aimButtonBehavior, value);
            OnPropertyChanged(nameof(AimUpLabel));
            OnPropertyChanged(nameof(AimDownLabel));
        }
    }

    [DynamicFormFieldCheckBox("Moon walk", groupName: "Metroid Control Settings Bottom")]
    public bool MoonWalk { get; set; }

    [DynamicFormFieldText(groupName: "Metroid Button Mappings Top")]
    public string ButtonMappingsDescription =>
        "Button mappings are based on the default SNES controls and do not account for different emulator button mappings.";

    [DynamicFormFieldComboBox(label:"Shoot:", groupName: "Metroid Button Mappings Bottom")]
    public MetroidButton ShootButton
    {
        get => _shootButton;
        set
        {
            ReplaceButton(value, _shootButton);
            SetField(ref _shootButton, value);
        }
    }

    [DynamicFormFieldComboBox(label:"Jump:", groupName: "Metroid Button Mappings Bottom")]
    public MetroidButton JumpButton
    {
        get => _jumpButton;
        set
        {
            ReplaceButton(value, _jumpButton);
            SetField(ref _jumpButton, value);
        }
    }

    [DynamicFormFieldComboBox(label: nameof(RunButtonLabel), labelIsProperty: true, groupName: "Metroid Button Mappings Bottom")]
    public MetroidButton DashButton
    {
        get => _dashButton;
        set
        {
            ReplaceButton(value, _dashButton);
            SetField(ref _dashButton, value);
        }
    }

    [DynamicFormFieldComboBox(label: "Item select:", groupName: "Metroid Button Mappings Bottom")]
    public MetroidButton ItemSelectButton
    {
        get => _itemSelectButton;
        set
        {
            ReplaceButton(value, _itemSelectButton);
            SetField(ref _itemSelectButton, value);
        }
    }

    [DynamicFormFieldComboBox(label: nameof(ItemCancelLabel), labelIsProperty: true, groupName: "Metroid Button Mappings Bottom")]
    public MetroidButton ItemCancelButton
    {
        get => _itemCancelButton;
        set
        {
            ReplaceButton(value, _itemCancelButton);
            SetField(ref _itemCancelButton, value);
        }
    }

    [DynamicFormFieldComboBox(label: nameof(AimUpLabel), labelIsProperty: true, groupName: "Metroid Button Mappings Bottom")]
    public MetroidButton AimUpButton
    {
        get => _aimUpButton;
        set
        {
            ReplaceButton(value, _aimUpButton);
            SetField(ref _aimUpButton, value);
        }
    }

    [DynamicFormFieldComboBox(label: nameof(AimDownLabel), labelIsProperty: true, groupName: "Metroid Button Mappings Bottom")]
    public MetroidButton AimDownButton
    {
        get => _aimDownButton;
        set
        {
            ReplaceButton(value, _aimDownButton);
            SetField(ref _aimDownButton, value);
        }
    }

    public string RunButtonLabel => RunButtonBehavior == RunButtonBehavior.Vanilla ? "Run:" : "Walk:";

    public string ItemCancelLabel => ItemCancelBehavior switch
    {
        ItemCancelBehavior.Vanilla => "Item cancel:",
        ItemCancelBehavior.HoldSupersOnly => "Hold to equip:",
        ItemCancelBehavior.Hold => "Hold to equip:",
        ItemCancelBehavior.Toggle => "Item toggle:",
        _ => "Item cancel:"
    };

    public string AimUpLabel => AimButtonBehavior switch
    {
        AimButtonBehavior.Vanilla => "Aim up:",
        AimButtonBehavior.UnifiedAim => "Aim:",
        _ => "Aim up:"
    };

    public string AimDownLabel => AimButtonBehavior switch
    {
        AimButtonBehavior.Vanilla => "Aim down:",
        AimButtonBehavior.UnifiedAim => "Quick morph:",
        _ => "Aim up:"
    };

    private void ReplaceButton(MetroidButton oldButton, MetroidButton newButton)
    {
        if (_shootButton == oldButton)
        {
            _shootButton = newButton;
            OnPropertyChanged(nameof(ShootButton));
        }
        else if (_jumpButton == oldButton)
        {
            _jumpButton = newButton;
            OnPropertyChanged(nameof(JumpButton));
        }
        else if (_dashButton == oldButton)
        {
            _dashButton = newButton;
            OnPropertyChanged(nameof(DashButton));
        }
        else if (_itemSelectButton == oldButton)
        {
            _itemSelectButton = newButton;
            OnPropertyChanged(nameof(ItemSelectButton));
        }
        else if (_itemCancelButton == oldButton)
        {
            _itemCancelButton = newButton;
            OnPropertyChanged(nameof(ItemCancelButton));
        }
        else if (_aimUpButton == oldButton)
        {
            _aimUpButton = newButton;
            OnPropertyChanged(nameof(AimUpButton));
        }
        else if (_aimDownButton == oldButton)
        {
            _aimDownButton = newButton;
            OnPropertyChanged(nameof(AimDownButton));
        }
    }
}
