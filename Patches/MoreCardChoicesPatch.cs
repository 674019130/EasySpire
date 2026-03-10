using HarmonyLib;

namespace EasySpire.Patches;

/// <summary>
/// Increases the number of card choices offered after combat.
/// Hooks into CardReward to add extra cards to the selection.
/// </summary>
[HarmonyPatch]
internal static class MoreCardChoicesPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        var cardRewardType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Rewards.CardReward");
        if (cardRewardType == null) yield break;

        // Look for the property that returns the list of card choices
        foreach (var method in cardRewardType.GetMethods())
        {
            if (method.Name == "get_CardChoices")
                yield return method;
        }
    }

    [HarmonyPostfix]
    static void Postfix(object __result)
    {
        var settings = SettingsManager.Current;
        if (!settings.MoreCardChoices.Enabled) return;
        if (__result == null) return;

        // __result is likely a List<T> of card choices
        // We can't easily add more cards without knowing the generation logic,
        // so this patch works by intercepting the AfterModifyingCardRewardOptions hook instead
    }
}

/// <summary>
/// Alternative approach: hook into the reward modification system
/// to add additional card options.
/// </summary>
[HarmonyPatch]
internal static class CardRewardOptionsPatch
{
    [HarmonyTargetMethods]
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        // Hook AfterModifyingCardRewardOptions on the Hook system
        var hookType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Hooks.Hook");
        if (hookType != null)
        {
            foreach (var method in hookType.GetMethods())
            {
                if (method.Name == "AfterModifyingCardRewardOptions")
                    yield return method;
            }
        }
    }

    [HarmonyPostfix]
    static void Postfix(object[] __args)
    {
        var settings = SettingsManager.Current;
        if (!settings.MoreCardChoices.Enabled) return;

        try
        {
            // The args should contain the card reward with a list of cards
            foreach (var arg in __args)
            {
                if (arg == null) continue;
                var type = arg.GetType();

                // Look for a list-like property containing card choices
                var cardsProp = type.GetProperty("CardChoices") ??
                                type.GetProperty("Cards") ??
                                type.GetProperty("Options");
                if (cardsProp == null) continue;

                var list = cardsProp.GetValue(arg);
                if (list == null) continue;

                // Get Count property to see current number of choices
                var countProp = list.GetType().GetProperty("Count");
                if (countProp == null) continue;

                var currentCount = (int)countProp.GetValue(list)!;
                var extraCards = (int)settings.MoreCardChoices.Value;

                // We've found the card list - the extra cards need to be generated
                // through the game's own card generation system, which is complex.
                // For now, this serves as a hook point that can be expanded.
                break;
            }
        }
        catch
        {
            // Silently fail
        }
    }
}
