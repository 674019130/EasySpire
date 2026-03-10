using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Grants extra energy at the start of each player turn.
/// </summary>
[HarmonyPatch]
internal static class EnergyPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        // Hook into the player's energy setup at turn start
        var playerCmdType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Commands.PlayerCmd");
        if (playerCmdType == null) yield break;

        foreach (var method in playerCmdType.GetMethods())
        {
            if (method.Name == "SetEnergy")
                yield return method;
        }
    }

    [HarmonyPrefix]
    static void Prefix(ref int __0)
    {
        var settings = SettingsManager.Current;
        if (!settings.ExtraEnergy.Enabled) return;

        __0 += (int)settings.ExtraEnergy.Value;
    }
}
