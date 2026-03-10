using HarmonyLib;

namespace EasySpire;

/// <summary>
/// Injects a settings UI into the game's Mod info panel.
/// When the player selects EasySpire in Settings -> Mods, toggle buttons appear.
/// </summary>
[HarmonyPatch]
internal static class ModInfoUiPatch
{
    private static readonly string TargetPckName = "EasySpire";

    // Feature display names (Chinese)
    private static readonly (string PropName, string Label, bool HasValue, string ValueLabel)[] Features =
    [
        ("PlayerHpBoost", "玩家HP加成", true, "倍率"),
        ("EnemyDamageReduce", "敌人伤害降低", true, "倍率"),
        ("EnemyHpReduce", "敌人血量降低", true, "倍率"),
        ("PostCombatHeal", "战斗后回血", true, "比例"),
        ("NullifyCurses", "诅咒卡无效化", false, ""),
        ("ExtraEnergy", "额外能量", true, "点数"),
        ("GoldBoost", "金币加成", true, "倍率"),
        ("ShopDiscount", "商店折扣", true, "倍率"),
    ];

    [HarmonyTargetMethods]
    private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        var type = AccessTools.TypeByName(
            "MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModInfoContainer");
        if (type == null) yield break;

        var modType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Modding.Mod");
        if (modType == null) yield break;

        var method = AccessTools.Method(type, "Fill", [modType]);
        if (method != null)
            yield return method;
    }

    [HarmonyPostfix]
    private static void Postfix(object __instance, object mod)
    {
        try
        {
            RefreshForSelection(__instance, mod);
        }
        catch
        {
            // Don't crash the game if UI injection fails
        }
    }

    private static void RefreshForSelection(object infoContainer, object mod)
    {
        // Check if this is our mod
        var pckName = GetField(mod, "pckName") as string;
        if (pckName == null) return;

        if (!System.IO.Path.GetFileNameWithoutExtension(pckName)
                .Equals(TargetPckName, StringComparison.OrdinalIgnoreCase))
            return;

        // Create UI container
        var vboxType = AccessTools.TypeByName("Godot.VBoxContainer");
        if (vboxType == null) return;

        var root = Activator.CreateInstance(vboxType)!;

        // Add a separator label
        var separatorLabel = CreateLabel("─── Easy Spire 设置 ───");
        AddChild(root, separatorLabel);

        var settings = SettingsManager.Current;

        foreach (var (propName, label, hasValue, valueLabel) in Features)
        {
            var prop = typeof(EasySpireSettings).GetProperty(propName);
            if (prop == null) continue;

            var toggle = (FeatureToggle?)prop.GetValue(settings);
            if (toggle == null) continue;

            // Create row: [Label] [ON/OFF button] [value if applicable]
            var hboxType = AccessTools.TypeByName("Godot.HBoxContainer");
            if (hboxType == null) continue;

            var row = Activator.CreateInstance(hboxType)!;

            // Feature label
            var featureLabel = CreateLabel($"  {label}: ");
            AddChild(row, featureLabel);

            // Toggle button
            var toggleBtn = CreateButton(toggle.Enabled ? "ON" : "OFF");
            var capturedPropName = propName;
            BindPressed(toggleBtn, () =>
            {
                var current = SettingsManager.Current;
                var currentProp = typeof(EasySpireSettings).GetProperty(capturedPropName);
                var currentToggle = (FeatureToggle?)currentProp?.GetValue(current);
                if (currentToggle == null) return;

                SettingsManager.UpdateFeature(capturedPropName, enabled: !currentToggle.Enabled);
                SetButtonText(toggleBtn, !currentToggle.Enabled ? "ON" : "OFF");
            });
            AddChild(row, toggleBtn);

            // Value adjust buttons
            if (hasValue)
            {
                var minusBtn = CreateButton("-");
                BindPressed(minusBtn, () =>
                {
                    var current = SettingsManager.Current;
                    var currentProp = typeof(EasySpireSettings).GetProperty(capturedPropName);
                    var currentToggle = (FeatureToggle?)currentProp?.GetValue(current);
                    if (currentToggle == null) return;

                    var newVal = Math.Max(0.1, currentToggle.Value - 0.1);
                    SettingsManager.UpdateFeature(capturedPropName, value: Math.Round(newVal, 1));
                });
                AddChild(row, minusBtn);

                var valLabel = CreateLabel($" {toggle.Value:F1} ");
                AddChild(row, valLabel);

                var plusBtn = CreateButton("+");
                BindPressed(plusBtn, () =>
                {
                    var current = SettingsManager.Current;
                    var currentProp = typeof(EasySpireSettings).GetProperty(capturedPropName);
                    var currentToggle = (FeatureToggle?)currentProp?.GetValue(current);
                    if (currentToggle == null) return;

                    var newVal = Math.Min(10.0, currentToggle.Value + 0.1);
                    SettingsManager.UpdateFeature(capturedPropName, value: Math.Round(newVal, 1));
                });
                AddChild(row, plusBtn);
            }

            AddChild(root, row);
        }

        AddChild(infoContainer, root);
    }

    // Godot node helpers via reflection
    private static object CreateLabel(string text)
    {
        var labelType = AccessTools.TypeByName("Godot.Label")!;
        var label = Activator.CreateInstance(labelType)!;
        var textProp = labelType.GetProperty("Text");
        textProp?.SetValue(label, text);
        return label;
    }

    private static object CreateButton(string text)
    {
        var btnType = AccessTools.TypeByName("Godot.Button")!;
        var btn = Activator.CreateInstance(btnType)!;
        var textProp = btnType.GetProperty("Text");
        textProp?.SetValue(btn, text);
        return btn;
    }

    private static void SetButtonText(object button, string text)
    {
        var textProp = button.GetType().GetProperty("Text");
        textProp?.SetValue(button, text);
    }

    private static void AddChild(object parent, object child)
    {
        var method = parent.GetType().GetMethod("AddChild",
            [AccessTools.TypeByName("Godot.Node")!, typeof(bool), typeof(int)]);
        if (method != null)
        {
            method.Invoke(parent, [child, false, 0]);
            return;
        }

        // Try simpler overload
        var simpleMethod = parent.GetType().GetMethod("AddChild",
            [AccessTools.TypeByName("Godot.Node")!]);
        simpleMethod?.Invoke(parent, [child]);
    }

    private static void BindPressed(object button, Action callback)
    {
        // Godot signals: button.Pressed += callback
        var pressedEvent = button.GetType().GetEvent("Pressed");
        if (pressedEvent == null) return;

        var handler = Delegate.CreateDelegate(pressedEvent.EventHandlerType!, callback.Target,
            callback.Method);
        pressedEvent.AddEventHandler(button, handler);
    }

    private static object? GetField(object obj, string name)
    {
        var field = obj.GetType().GetField(name,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        return field?.GetValue(obj);
    }
}
