using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Increases the amount of healing received at rest sites.
/// Hooks into HealRestSiteOption.ExecuteRestSiteHeal to boost healing.
/// </summary>
[HarmonyPatch]
internal static class RestSiteBoostPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        var healOptionType = AccessTools.TypeByName(
            "MegaCrit.Sts2.Core.Entities.RestSite.HealRestSiteOption");
        if (healOptionType == null) yield break;

        foreach (var method in healOptionType.GetMethods())
        {
            if (method.Name == "ExecuteRestSiteHeal")
                yield return method;
        }
    }

    /// <summary>
    /// After the rest site heal executes, apply additional healing
    /// based on the boost multiplier.
    /// </summary>
    [HarmonyPostfix]
    static void Postfix()
    {
        var settings = SettingsManager.Current;
        if (!settings.RestSiteBoost.Enabled) return;
        if (settings.RestSiteBoost.Value <= 1.0) return;

        try
        {
            // Access the current player to apply bonus healing
            // The rest site already healed the player, we add extra on top
            var runManagerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.RunManager");
            if (runManagerType == null) return;

            var instanceProp = runManagerType.GetProperty("Instance") ??
                               runManagerType.GetProperty("Current");
            if (instanceProp == null) return;

            var runManager = instanceProp.GetValue(null);
            if (runManager == null) return;

            var runStateProp = runManagerType.GetProperty("RunState") ??
                               runManagerType.GetProperty("State");
            var runState = runStateProp?.GetValue(runManager);
            if (runState == null) return;

            var playerProp = runState.GetType().GetProperty("Player") ??
                             runState.GetType().GetProperty("Players");
            var player = playerProp?.GetValue(runState);
            if (player == null) return;

            var maxHpProp = player.GetType().GetProperty("MaxHp");
            var currentHpProp = player.GetType().GetProperty("CurrentHp");
            if (maxHpProp == null || currentHpProp == null) return;

            var maxHp = (int)maxHpProp.GetValue(player)!;
            var currentHp = (int)currentHpProp.GetValue(player)!;

            // Default rest site heals ~30% of max HP
            // We add bonus healing: (multiplier - 1.0) * 30% * maxHp
            var bonusRatio = (settings.RestSiteBoost.Value - 1.0) * 0.3;
            var bonusHeal = (int)(maxHp * bonusRatio);
            var newHp = Math.Min(maxHp, currentHp + bonusHeal);
            currentHpProp.SetValue(player, newHp);
        }
        catch
        {
            // Don't crash
        }
    }
}
