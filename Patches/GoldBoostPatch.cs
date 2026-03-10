using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Multiplies gold gained from all sources.
/// </summary>
[HarmonyPatch]
internal static class GoldBoostPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        var playerCmdType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Commands.PlayerCmd");
        if (playerCmdType == null) yield break;

        foreach (var method in playerCmdType.GetMethods())
        {
            if (method.Name == "GainGold")
                yield return method;
        }
    }

    [HarmonyPrefix]
    static void Prefix(ref int __0)
    {
        var settings = SettingsManager.Current;
        if (!settings.GoldBoost.Enabled) return;

        if (__0 > 0) // Only boost positive gold gains, not costs
        {
            var original = __0;
            __0 = (int)(__0 * settings.GoldBoost.Value);
            Logger.LogPatchResult("GoldBoost", true, $"{original} -> {__0} (x{settings.GoldBoost.Value})");
        }
    }
}
