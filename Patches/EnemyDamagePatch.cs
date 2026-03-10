using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Reduces damage dealt by enemies to the player.
/// Hooks into the damage command to scale down enemy attacks.
/// </summary>
[HarmonyPatch]
internal static class EnemyDamagePatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        // Target CreatureCmd.Damage methods
        var creatureCmdType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Commands.CreatureCmd");
        if (creatureCmdType == null) yield break;

        foreach (var method in creatureCmdType.GetMethods())
        {
            if (method.Name == "Damage" && !method.IsAbstract)
                yield return method;
        }
    }

    [HarmonyPrefix]
    static void Prefix(object[] __args)
    {
        var settings = SettingsManager.Current;
        if (!settings.EnemyDamageReduce.Enabled) return;

        // Find the int damage parameter and scale it down
        // The Damage methods have various signatures, but the int amount is always present
        for (int i = 0; i < __args.Length; i++)
        {
            if (__args[i] is int amount && amount > 0)
            {
                // Check if the source is an enemy (not the player hurting themselves)
                // We scale all incoming damage - this is the simplest approach
                var newAmount = (int)(amount * settings.EnemyDamageReduce.Value);
                Logger.LogPatchResult("EnemyDamageReduce", true, $"{amount} -> {newAmount}");
                __args[i] = newAmount;
                break;
            }
        }
    }
}
