using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using UnityEngine;

namespace DerailValleyModToolbar;

public static class Main
{
    public static Harmony harmony;
    public static UnityModManager.ModEntry ModEntry;
    public static Settings settings;
    public static GameObject? toolbarGO;
    public static Toolbar? toolbar;

    static bool Load(UnityModManager.ModEntry modEntry)
    {
        ModEntry = modEntry;

        try
        {
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;

            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            toolbarGO = new GameObject("DerailValleyModToolbar_Toolbar");
            toolbar = toolbarGO.AddComponent<Toolbar>();

            WorldStreamingInit.LoadingFinished += PatchOtherMods;

            modEntry.Logger.Log("DerailValleyModToolbar started");
        }
        catch (Exception ex)
        {
            modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
            harmony?.UnpatchAll(modEntry.Info.Id);
            return false;
        }

        return true;
    }

    static void PatchOtherMods()
    {
        ModEntry.Logger.Log("World has finished loading, patching other mods...");

        UtilitiesMod_Patcher.RegisterWithToolbar(harmony);

        // gets called multiple times
        WorldStreamingInit.LoadingFinished -= PatchOtherMods;
    }

    static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        settings.Draw(modEntry);
    }

    static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        settings.Save(modEntry);
    }

    static bool OnUnload(UnityModManager.ModEntry modEntry)
    {
        modEntry.Logger.Log("DerailValleyModToolbar stopping...");

        WorldStreamingInit.LoadingFinished -= PatchOtherMods;

        if (toolbarGO != null)
            GameObject.Destroy(toolbarGO);

        toolbarGO = null;
        toolbar = null;

        harmony?.UnpatchAll(modEntry.Info.Id);

        modEntry.Logger.Log("DerailValleyModToolbar stopped");
        return true;
    }
}
