using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerBossService
{
    /// <summary>
    /// Marks a dungeon as cleared and, if possible, tracks the boss reward.
    /// </summary>
    /// <param name="region">The dungeon that was cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was cleared by the auto tracker</param>
    public void MarkRegionBossAsDefeated(IHasBoss region, float? confidence = null, bool autoTracked = false);

    /// <summary>
    /// Marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that was defeated.</param>
    /// <param name="admittedGuilt">
    /// <see langword="true"/> if the command implies the boss was killed;
    /// <see langword="false"/> if the boss was simply "tracked".
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    public void MarkBossAsDefeated(Boss boss, bool admittedGuilt = true, float? confidence = null,
        bool autoTracked = false);

    /// <summary>
    /// Un-marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that should be 'revived'.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void MarkBossAsNotDefeated(Boss boss, float? confidence = null);

    /// <summary>
    /// Un-marks a dungeon as cleared and, if possible, untracks the boss
    /// reward.
    /// </summary>
    /// <param name="region">The dungeon that should be un-cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void MarkRegionBossAsNotDefeated(IHasBoss region, float? confidence = null);
}
