namespace Randomizer.Data.Options;

/// <summary>
/// Class for different settings over how Super Metroid controls
/// </summary>
public class MetroidControlOptions
{
    public MetroidButton Shoot { get; set; } = MetroidButton.X;
    public MetroidButton Jump { get; set; } = MetroidButton.A;
    public MetroidButton Dash { get; set; } = MetroidButton.B;
    public MetroidButton ItemSelect { get; set; } = MetroidButton.Select;
    public MetroidButton ItemCancel { get; set; } = MetroidButton.Y;
    public MetroidButton AimUp { get; set; } = MetroidButton.R;
    public MetroidButton AimDown { get; set; } = MetroidButton.L;
    public bool MoonWalk { get; set; }
    public ItemCancelBehavior ItemCancelBehavior { get; set; } = ItemCancelBehavior.Vanilla;
    public RunButtonBehavior RunButtonBehavior { get; set; } = RunButtonBehavior.Vanilla;
    public AimButtonBehavior AimButtonBehavior { get; set; } = AimButtonBehavior.Vanilla;

    public MetroidControlOptions Clone()
    {
        return (MetroidControlOptions)MemberwiseClone();
    }
}
