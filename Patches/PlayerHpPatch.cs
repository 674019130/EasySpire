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

            // HP properties are on Creature base class, need to search up the hierarchy
            var maxHpProp = type.GetProperty("MaxHp",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
            var currentHpProp = type.GetProperty("CurrentHp",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);

            // If not found on Player, walk up to Creature base class explicitly
            if (maxHpProp == null || currentHpProp == null)
            {
                var creatureType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Creatures.Creature");
                if (creatureType != null)
                {
                    maxHpProp ??= creatureType.GetProperty("MaxHp");
                    currentHpProp ??= creatureType.GetProperty("CurrentHp");
                }
            }

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
