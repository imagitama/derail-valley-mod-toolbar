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
    public static GameObject toolbarGO;
    public static Toolbar toolbar;

    private static bool Load(UnityModManager.ModEntry modEntry)
    {
        ModEntry = modEntry;

        try
        {
            settings = Settings.Load<Settings>(modEntry);
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            toolbarGO = new GameObject("DerailValleyModToolbar_Toolbar");
            toolbar = toolbarGO.AddComponent<Toolbar>();

            WorldStreamingInit.LoadingFinished += OnLoadingFinished;

            modEntry.Logger.Log("DerailValleyModToolbar started");
        }
        catch (Exception ex)
        {
            modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
            harmony?.UnpatchAll(modEntry.Info.Id);
            return false;
        }

        modEntry.OnUnload = Unload;
        return true;
    }

    public static void OnLoadingFinished()
    {
        ModEntry.Logger.Log("World has finished loading, patching other mods...");

        UtilitiesMod_Patcher.RegisterWithToolbar(harmony);
    }

    static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        settings.Draw(modEntry);
    }

    static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        settings.Save(modEntry);
    }

    private static bool Unload(UnityModManager.ModEntry modEntry)
    {
        modEntry.Logger.Log("DerailValleyModToolbar stopping...");

        WorldStreamingInit.LoadingFinished -= OnLoadingFinished;

        if (toolbar != null)
            GameObject.Destroy(toolbar);
        if (toolbarGO != null)
            GameObject.Destroy(toolbarGO);

        harmony?.UnpatchAll(modEntry.Info.Id);

        modEntry.Logger.Log("DerailValleyModToolbar stopped");
        return true;
    }
}
