using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Reduces enemy max HP when they are initialized for combat.
/// </summary>
[HarmonyPatch]
internal static class EnemyHpPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        // Target the SetMaxHp method on creatures
        var creatureCmdType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Commands.CreatureCmd");
        if (creatureCmdType == null) yield break;

        foreach (var method in creatureCmdType.GetMethods())
        {
            if (method.Name == "SetMaxHp")
                yield return method;
        }
    }

    [HarmonyPrefix]
    static void Prefix(object __instance, ref int __0)
    {
        var settings = SettingsManager.Current;
        if (!settings.EnemyHpReduce.Enabled) return;

        // Only reduce HP for non-player creatures
        // Check if the creature is a monster (not a player)
        var creatureType = __instance?.GetType();
        var playerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Players.Player");

        if (creatureType != null && playerType != null && !playerType.IsAssignableFrom(creatureType))
        {
            var original = __0;
            __0 = Math.Max(1, (int)(__0 * settings.EnemyHpReduce.Value));
            Logger.LogPatchResult("EnemyHpReduce", true, $"{creatureType.Name} MaxHp {original} -> {__0}");
        }
    }
}
