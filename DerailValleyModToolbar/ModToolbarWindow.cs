using System;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;

namespace DerailValleyModToolbar;

public interface IModToolbarPanel
{
    public void Window(Rect rect);
}

public class ModToolbarWindow : MonoBehaviour
{
    private static ModEntry.ModLogger Logger => Main.ModEntry.Logger;
    public string Title;
    public Action<Rect>? Content;
    public int Width = Main.settings.DefaultWidth;
    public int Height = Main.settings.DefaultHeight;
    private bool showGui = false;
    private Rect windowRect = new Rect(20, 30, 0, 0);
    private Vector2 scrollPosition;
    public static int LastWindowId = 800;
    public int WindowID = LastWindowId++;

    public void Show() { showGui = true; }
    public void Hide() { showGui = false; }
    public void Toggle() { showGui = !showGui; }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        Logger.Log($"Create window title={Title} content={Content} id={WindowID} width={Width} height={Height}");
    }

    void OnGUI()
    {
        if (!VRManager.IsVREnabled() && ScreenspaceMouse.Instance && !ScreenspaceMouse.Instance.on) return;

        if (!showGui)
            return;

        float scale = Main.settings.Scale;
        Vector2 pivot = Vector2.zero; // top-left corner

        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        windowRect = GUILayout.Window(WindowID, windowRect, Window, Title);

        GUI.matrix = oldMatrix;
    }

    void Window(int windowId)
    {
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition,
            GUILayout.Width(Width + GUI.skin.verticalScrollbar.fixedWidth),
            GUILayout.Height(Height + GUI.skin.horizontalScrollbar.fixedHeight)
        );

        GUILayout.BeginVertical();

        Content?.Invoke(windowRect);

        GUILayout.EndVertical();

        GUILayout.EndScrollView();

        GUI.DragWindow();
    }
}