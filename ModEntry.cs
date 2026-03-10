using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace EasySpire;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    public static Harmony? HarmonyInstance { get; private set; }

    public static void Initialize()
    {
        Logger.Initialize();
        Logger.Log("Initializing settings...");
        SettingsManager.Initialize();
        Logger.Log($"Settings loaded: {SettingsManager.Current}");

        Logger.Log("Applying Harmony patches...");
        HarmonyInstance = new Harmony("com.susu.easyspire");

        try
        {
            HarmonyInstance.PatchAll();
            var patchedMethods = HarmonyInstance.GetPatchedMethods().ToList();
            Logger.Log($"Harmony patched {patchedMethods.Count} methods:");
            foreach (var method in patchedMethods)
            {
                Logger.Log($"  - {method.DeclaringType?.FullName}.{method.Name}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("PatchAll", ex);
        }

        Logger.Log("EasySpire initialization complete!");
    }
}
