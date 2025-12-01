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
    public Action<Rect>? Content;
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
        Position();
    }

    void Position()
    {
        windowRect = new Rect(Toolbar.Margin, Toolbar.Margin + Toolbar.ButtonSize + Toolbar.Margin, Width, Height);
    }

    void Start()
    {
        Logger.Log($"Create window title={Title} content={Content} id={WindowID} width={Width} height={Height}");
        Position();
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

        if (Event.current.type == EventType.Repaint)
        {
            GUILayout.BeginVertical();
            Content?.Invoke(windowRect);
            GUILayout.EndVertical();

            var r = GUILayoutUtility.GetLastRect();
            float targetWidth = Mathf.Min(r.width + 20f, maxWidth);
            float targetHeight = Mathf.Min(r.height + 20f, maxHeight);

            Width = (int)targetWidth;
            Height = (int)targetHeight;

            windowRect.width = Width;
            windowRect.height = Height;
        }

        bool needsScroll = Height >= maxHeight;

        if (needsScroll)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();
            Content?.Invoke(windowRect);
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.BeginVertical();
            Content?.Invoke(windowRect);
            GUILayout.EndVertical();
        }

        GUI.DragWindow();
    }

}