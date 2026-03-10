using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Heals the player for a percentage of max HP after combat victory.
/// </summary>
[HarmonyPatch]
internal static class PostCombatHealPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        // Hook into combat victory
        var combatManagerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Combat.CombatManager");
        if (combatManagerType == null) yield break;

        // Try to find a victory/end combat method
        foreach (var method in combatManagerType.GetMethods())
        {
            if (method.Name is "EndCombatVictory" or "OnCombatVictory" or "Victory")
                yield return method;
        }
    }

    [HarmonyPostfix]
    static void Postfix()
    {
        var settings = SettingsManager.Current;
        if (!settings.PostCombatHeal.Enabled) return;

        try
        {
            // Access the current player through the run state
            var runManagerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Runs.RunManager");
            if (runManagerType == null) return;

            var instanceProp = runManagerType.GetProperty("Instance") ??
                               runManagerType.GetProperty("Current");
            if (instanceProp == null) return;

            var runManager = instanceProp.GetValue(null);
            if (runManager == null) return;

            // Try to get player from run state
            var runStateProp = runManagerType.GetProperty("RunState") ??
                               runManagerType.GetProperty("State");
            var runState = runStateProp?.GetValue(runManager);
            if (runState == null) return;

            var playerProp = runState.GetType().GetProperty("Player") ??
                             runState.GetType().GetProperty("Players");
            var player = playerProp?.GetValue(runState);
            if (player == null) return;

            // Heal the player
            var maxHpProp = player.GetType().GetProperty("MaxHp");
            var currentHpProp = player.GetType().GetProperty("CurrentHp");
            if (maxHpProp == null || currentHpProp == null) return;

            var maxHp = (int)maxHpProp.GetValue(player)!;
            var currentHp = (int)currentHpProp.GetValue(player)!;
            var healAmount = (int)(maxHp * settings.PostCombatHeal.Value);
            var newHp = Math.Min(maxHp, currentHp + healAmount);
            currentHpProp.SetValue(player, newHp);
        }
        catch
        {
            // Don't crash if we can't find the player
        }
    }
}
