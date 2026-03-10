using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Increases player's max HP at the start of a new run.
/// Uses reflection to access HP properties on the Creature base class.
/// </summary>
[HarmonyPatch]
internal static class PlayerHpPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        var playerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Players.Player");
        if (playerType == null) yield break;

        var method = AccessTools.Method(playerType, "CreateForNewRun");
        if (method != null)
            yield return method;
    }

    [HarmonyPostfix]
    static void Postfix(object __result)
    {
        var settings = SettingsManager.Current;
        if (!settings.PlayerHpBoost.Enabled) return;

        try
        {
            var type = __result.GetType();

            // Try property names on Creature/Player hierarchy
            var maxHpProp = type.GetProperty("MaxHp") ??
                            type.GetProperty("Hp");
            var currentHpProp = type.GetProperty("CurrentHp") ??
                                type.GetProperty("Hp");

            if (maxHpProp == null || currentHpProp == null) return;

            var maxHp = (int)maxHpProp.GetValue(__result)!;
            var multiplier = settings.PlayerHpBoost.Value;
            var newMaxHp = (int)(maxHp * multiplier);

            maxHpProp.SetValue(__result, newMaxHp);
            currentHpProp.SetValue(__result, newMaxHp);
        }
        catch
        {
            // Silently fail - don't crash
        }
    }
}
