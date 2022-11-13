namespace Randomizer.Data.Multiworld;

public class SubmitConfigResponse : MultiworldResponse
{
    public bool IsValid => IsSuccessful;
}
