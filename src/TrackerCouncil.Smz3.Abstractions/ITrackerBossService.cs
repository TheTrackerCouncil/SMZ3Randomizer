using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerBossService
{
    public event EventHandler<BossTrackedEventArgs>? BossUpdated;

    /// <summary>
    /// Marks a dungeon as cleared and, if possible, tracks the boss reward.
    /// </summary>
    /// <param name="region">The dungeon that was cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was cleared by the auto tracker</param>
    /// <param name="admittedGuilt">
    /// <see langword="true"/> if the command implies the boss was killed;
    /// <see langword="false"/> if the boss was simply "tracked".
    /// <param name="force">If the boss should be forced to be tracked while auto tracking</param>
    /// </param>
    public void MarkBossAsDefeated(IHasBoss region, float? confidence = null, bool autoTracked = false, bool admittedGuilt = false, bool force = false);

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
    /// <param name="force">If the boss should be forced to be tracked while auto tracking</param>
    public void MarkBossAsDefeated(Boss boss, bool admittedGuilt = true, float? confidence = null,
        bool autoTracked = false, bool force = false);

    /// <summary>
    /// Un-marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that should be 'revived'.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="force">If the boss should be forced to be untracked while auto tracking</param>
    public void MarkBossAsNotDefeated(Boss boss, float? confidence = null, bool force = false);

    /// <summary>
    /// Un-marks a dungeon as cleared and, if possible, untracks the boss
    /// reward.
    /// </summary>
    /// <param name="region">The dungeon that should be un-cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="force">If the boss should be forced to be untracked while auto tracking</param>
    public void MarkBossAsNotDefeated(IHasBoss region, float? confidence = null, bool force = false);

    public void UpdateAccessibility(Progression? actualProgression = null, Progression? withKeysProgression = null);

    public void UpdateAccessibility(Boss boss, Progression? actualProgression = null, Progression? withKeysProgression = null);

    public void UpdateAccessibility(IHasBoss region, Progression? actualProgression = null, Progression? withKeysProgression = null);


}
