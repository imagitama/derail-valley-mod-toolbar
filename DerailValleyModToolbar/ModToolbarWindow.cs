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
    public int DefaultWidth = Main.settings.DefaultWidth;
    public int DefaultHeight = Main.settings.DefaultHeight;
    private bool _visible;
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value) return;
            _visible = value;
            Main.settings.SavePanelState(Id, visible: _visible);
        }
    }
    public Rect? WindowRect;
    public static int LastWindowId = 800;
    public int WindowID = LastWindowId++;
    public string Id;
    private Vector2 _scrollPosition;
    private Rect _lastRect;
    float _nextCheckTime;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Position(Rect rect)
    {
        WindowRect = rect;
    }

    void Start()
    {
        Logger.Log($"[ModToolbarWindow] Start title={Title} content={DrawContent} id={WindowID} width={DefaultWidth} height={DefaultHeight} visible={Visible} rect={WindowRect}");
    }

    void OnGUI()
    {
        if (!VRManager.IsVREnabled() && ScreenspaceMouse.Instance && !ScreenspaceMouse.Instance.on)
            return;

        if (!Visible)
            return;

        if (WindowRect == null)
            WindowRect = new Rect(Toolbar.Margin, Toolbar.Margin + Toolbar.ButtonSize + Toolbar.Margin, DefaultWidth, DefaultHeight);

        float scale = Main.settings.Scale;
        Vector2 pivot = Vector2.zero; // top-left corner

        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        var newRect = GUILayout.Window(WindowID, WindowRect.Value, Window, Title);

        if (Time.realtimeSinceStartup >= _nextCheckTime && newRect.size != Vector2.zero)
        {
            if (newRect.position != _lastRect.position || newRect.size != _lastRect.size)
                OnRectChanged();

            _lastRect = newRect;
            _nextCheckTime = Time.realtimeSinceStartup + 1f;
        }

        WindowRect = newRect;

        GUI.matrix = oldMatrix;
    }

    Vector2 scroll;
    bool resizing;
    int resizeControlId;
    Vector2 resizeStartMouse;
    Vector2 resizeStartSize;

    void Window(int id)
    {
        float pad = 30f;
        float maxW = Screen.width - pad * 2f;
        float maxH = Screen.height - pad * 2f;

        var r = WindowRect.Value;

        if (r.width <= 1 || r.height <= 1)
        {
            r.width = DefaultWidth;
            r.height = DefaultHeight;
        }

        var resizeRect = new Rect(r.width - 20, r.height - 20, 20, 20);
        resizeControlId = GUIUtility.GetControlID(FocusType.Passive);

        switch (Event.current.type)
        {
            case EventType.MouseDown:
                if (resizeRect.Contains(Event.current.mousePosition))
                {
                    GUIUtility.hotControl = resizeControlId;
                    resizing = true;
                    resizeStartMouse = Event.current.mousePosition;
                    resizeStartSize = new Vector2(r.width, r.height);
                    Event.current.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == resizeControlId)
                {
                    var delta = Event.current.mousePosition - resizeStartMouse;
                    r.width = Mathf.Clamp(resizeStartSize.x + delta.x, DefaultWidth, maxW);
                    r.height = Mathf.Clamp(resizeStartSize.y + delta.y, DefaultHeight, maxH);
                    Event.current.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == resizeControlId)
                {
                    resizing = false;
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
                break;
        }

        r.x = Mathf.Clamp(r.x, pad, Screen.width - r.width - pad);
        r.y = Mathf.Clamp(r.y, pad, Screen.height - r.height - pad);

        WindowRect = r;

        scroll = GUILayout.BeginScrollView(scroll, false, false);
        DrawContent?.Invoke(WindowRect.Value);
        GUILayout.EndScrollView();

        GUI.DrawTexture(resizeRect, Texture2D.blackTexture);

        GUI.DragWindow();
    }

    public void Show()
    {
        Visible = true;
    }

    public void Hide()
    {
        Visible = false;
    }

    public void Toggle()
    {
        Visible = !Visible;
    }

    void OnRectChanged()
    {
        Main.settings.SavePanelState(Id, rect: WindowRect!.Value);
    }
}