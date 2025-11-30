using System.Collections.Generic;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;

namespace DerailValleyModToolbar;

public class Toolbar : MonoBehaviour
{
    private static ModEntry.ModLogger Logger => Main.ModEntry.Logger;
    private readonly List<RuntimeElement> elements = new();

    private const float ButtonSize = 40f;
    private const float Margin = 5f;

    void Awake()
    {
        Logger.Log("Toolbar.Awake");
        DontDestroyOnLoad(gameObject);
    }

    public void AddElement(RuntimeElement runtime)
    {
        Logger.Log($"Toolbar.AddElement runtime={runtime}");
        elements.Add(runtime);
    }

    void OnGUI()
    {
        // only show if are truly loaded in
        if (PlayerManager.PlayerTransform == null)
            return;

        // only show if in UI mode
        if (!VRManager.IsVREnabled() && ScreenspaceMouse.Instance && !ScreenspaceMouse.Instance.on) return;

        Matrix4x4 prev = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Main.settings.Scale, Main.settings.Scale, 1));

        float scaledMargin = Margin;
        float scaledButton = ButtonSize;

        float x = scaledMargin + Main.settings.OffsetX;
        float y = scaledMargin + Main.settings.OffsetY;

        foreach (var element in elements)
        {
            var def = element.Definition;

            GUIContent content = new GUIContent(def.Icon != null ? null : def.Label, def.Icon, def.Tooltip);

            Rect rect = new Rect(x, y, scaledButton, scaledButton);

            if (GUI.Button(rect, content))
            {
                if (def is ControlDefinition control)
                    control.OnClick();

                element.ModToolbarWindow?.Toggle();
            }

            x += scaledButton + scaledMargin;
        }

        GUI.matrix = prev;
    }
}
