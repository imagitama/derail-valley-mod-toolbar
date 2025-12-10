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
    public string Title = "";
    public Action<Rect>? DrawContent;
    public int Width = Main.settings.DefaultWidth;
    public int Height = Main.settings.DefaultHeight;
    private bool showGui = false;
    private Rect windowRect = new Rect(0, 0, 0, 0);
    private Vector2 scrollPosition;
    public static int LastWindowId = 800;
    public int WindowID = LastWindowId++;

    public void Show() { showGui = true; }
    public void Hide() { showGui = false; }
    public void Toggle() { showGui = !showGui; }

    void Awake()
    {
        DontDestroyOnLoad(this);
        Reposition();
    }

    void Reposition()
    {
        windowRect = new Rect(Toolbar.Margin, Toolbar.Margin + Toolbar.ButtonSize + Toolbar.Margin, Width, Height);
    }

    void Start()
    {
        Logger.Log($"Create window title={Title} content={DrawContent} id={WindowID} width={Width} height={Height}");
        Reposition();
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
        float maxHeight = Screen.height * 0.9f;
        float maxWidth = Screen.width * 0.9f;

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.BeginVertical();

        var contentRect = GUILayoutUtility.GetRect(
            0, 99999,
            0, 99999,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(false)
        );

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (Event.current.type == EventType.Repaint)
        {
            float tw = Mathf.Min(contentRect.width + 20f, maxWidth);
            float th = Mathf.Min(contentRect.height + 20f, maxHeight);

            Width = (int)tw;
            Height = (int)th;

            windowRect.width = Width;
            windowRect.height = Height;
        }

        bool needsScroll = Height >= maxHeight;

        if (needsScroll)
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.BeginVertical();
        DrawContent?.Invoke(windowRect);
        GUILayout.EndVertical();

        if (needsScroll)
            GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}