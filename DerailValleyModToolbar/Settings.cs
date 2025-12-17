using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace DerailValleyModToolbar;

[Serializable]
public class PanelState
{
    public PanelState()
    {
    }
    public string Id; // modname_type|title
    public bool Visible = false;
    public Rect? Rect = null;
}

public class Settings : UnityModManager.ModSettings, IDrawable
{
    private static UnityModManager.ModEntry.ModLogger Logger => Main.ModEntry.Logger;
    public List<PanelState> PanelStates = [];
    [Draw(Type = DrawType.Slider, Min = 0.5f, Max = 5f)] public float Scale = 1f;
    [Draw(Type = DrawType.Slider, Min = -200f, Max = 200f)] public float OffsetX = 0f;
    [Draw(Type = DrawType.Slider, Min = -200f, Max = 200f)] public float OffsetY = 0f;
    [Draw(Type = DrawType.Slider, Min = 100, Max = 2000)] public int DefaultWidth = 700;
    [Draw(Type = DrawType.Slider, Min = 100, Max = 2000)] public int DefaultHeight = 400;
    [Draw]
    public bool ShowDebugPanel = false;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public void OnChange()
    {
    }

    // TODO: probably let user specify ID here
    public string GetIdForPanel(PanelDefinition def) => $"{def.ModEntry.Info.Id}_{(def.WindowType != null ? def.WindowType : def.WindowTitle)}";

    public PanelState? GetPanelState(PanelDefinition def)
    {
        return PanelStates.FirstOrDefault(x => x.Id == GetIdForPanel(def));
    }

    public PanelState? GetPanelState(string id)
    {
        var panelState = PanelStates.FirstOrDefault(x => x.Id == id);
        Logger.Log($"GetPanelState id={id} result={panelState}");
        return panelState;
    }

    public void SavePanelState(string id, bool? visible = null, Rect? rect = null)
    {
        var panelState = GetPanelState(id);

        if (panelState == null)
        {
            Logger.Log($"SavePanelState id={id} does not exist, creating...");
            panelState = new PanelState()
            {
                Id = id
            };
            PanelStates.Add(panelState);
        }

        if (visible != null)
            panelState.Visible = visible.Value;

        if (rect != null)
            panelState.Rect = rect.Value;

        Logger.Log($"SavePanelState id={id} visible={visible} rect={rect}");

        Save(this, Main.ModEntry);
    }

    public void ResetPanelStates()
    {
        Logger.Log("Reset panel states");

        PanelStates.Clear();

        Save(this, Main.ModEntry);
    }
}
