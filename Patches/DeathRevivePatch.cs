using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Prevents player death by intercepting the BeforeDeath hook.
/// When the player would die, restore them to a percentage of max HP instead.
/// Each run gets one free revive (resets per combat or per run depending on game state).
/// </summary>
[HarmonyPatch]
internal static class DeathRevivePatch
{
    private static bool _reviveUsed = false;

    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        // Hook into BeforeDeath on the Hook system
        var hookType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Hooks.Hook");
        if (hookType != null)
        {
            foreach (var method in hookType.GetMethods())
            {
                if (method.Name == "BeforeDeath")
                    yield return method;
            }
        }

        // Also reset revive flag when combat starts
        var combatManagerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Combat.CombatManager");
        if (combatManagerType != null)
        {
            foreach (var method in combatManagerType.GetMethods())
            {
                if (method.Name == "StartCombat")
                    yield return method;
            }
        }
    }

    [HarmonyPrefix]
    static bool Prefix(System.Reflection.MethodBase __originalMethod, object[] __args)
    {
        var settings = SettingsManager.Current;
        if (!settings.DeathRevive.Enabled) return true;

        // Reset revive on combat start
        if (__originalMethod.Name == "StartCombat")
        {
            _reviveUsed = false;
            return true;
        }

        // BeforeDeath - check if this is a player dying
        if (__originalMethod.Name == "BeforeDeath" && !_reviveUsed)
        {
            try
            {
                // The first argument should be the creature that's dying
                if (__args.Length == 0) return true;

                var creature = __args[0];
                var creatureType = creature.GetType();
                var playerType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Players.Player");

                if (playerType != null && playerType.IsAssignableFrom(creatureType))
                {
                    // This is a player dying - revive them!
                    var maxHpProp = creatureType.GetProperty("MaxHp");
                    var currentHpProp = creatureType.GetProperty("CurrentHp");

                    if (maxHpProp != null && currentHpProp != null)
                    {
                        var maxHp = (int)maxHpProp.GetValue(creature)!;
                        var reviveHp = Math.Max(1, (int)(maxHp * settings.DeathRevive.Value));
                        currentHpProp.SetValue(creature, reviveHp);
                        _reviveUsed = true;
                        return false; // Cancel the death
                    }
                }
            }
            catch
            {
                // Don't crash - let death proceed normally
            }
        }

        return true; // Let original method run
    }
}
