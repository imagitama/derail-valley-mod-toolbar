using HarmonyLib;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;

namespace DerailValleyModToolbar;

public static class UtilitiesMod_Patcher
{
    private static ModEntry.ModLogger Logger => Main.ModEntry.Logger;
    public static void RegisterWithToolbar(Harmony harmony)
    {
        var utilType = AccessTools.TypeByName("UtilitiesMod.UtilitiesMod");
        if (utilType == null)
        {
            Logger.Log($"UtilitiesMod not found, skipping");
            return;
        }

        PatchOnGUI(harmony, utilType);
        RegisterUtilitiesPanel(utilType);

        Logger.Log($"Succesfully registered UtilitiesMod");
    }

    private static void PatchOnGUI(Harmony harmony, System.Type utilType)
    {
        var onGuiMethod = AccessTools.Method(utilType, "OnGUI");
        if (onGuiMethod == null)
        {
            Logger.Log($"UtilitiesMod.OnGUI not found, skipping");
            return;
        }

        var prefixMethod = typeof(UtilitiesMod_Patch).GetMethod(nameof(UtilitiesMod_Patch.OnGUI_Prefix));

        harmony.Patch(onGuiMethod, prefix: new HarmonyMethod(prefixMethod));

        Logger.Log($"Successfully patched UtilitiesMod.OnGUI");
    }

    private static void RegisterUtilitiesPanel(System.Type utilType)
    {
        var utilComponent = Object.FindObjectOfType(utilType) as MonoBehaviour;
        if (utilComponent == null)
        {
            Logger.Log($"Could not find UtilitiesMod instance, skipping");
            return;
        }

        var windowMethod = utilType.GetMethod("Window",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        var weatherMethod = utilType.GetMethod("WeatherPresetWindow",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        ModToolbarAPI.Register(Main.ModEntry)
            .AddPanelControl(
                label: "Utilities",
                icon: null,
                tooltip: "Utilities",
                title: "Utilities",
                onGUIContent: rect =>
                {
                    try
                    {
                        windowMethod?.Invoke(utilComponent, new object[] { 555 });
                        weatherMethod?.Invoke(utilComponent, new object[] { 556 });
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Log($"Failed rendering: {ex}");
                    }
                },
                width: 300)
            .Finish();

        Logger.Log($"Utilities control added to toolbar");
    }
}

public static class UtilitiesMod_Patch
{
    public static bool OnGUI_Prefix()
    {
        return false;
    }
}
