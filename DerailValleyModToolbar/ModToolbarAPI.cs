using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;

namespace DerailValleyModToolbar;

public static class ModToolbarAPI
{
    private static ModEntry.ModLogger Logger => Main.ModEntry.Logger;
    private static readonly Dictionary<ModEntry, List<RuntimeElement>> Active = new();

    public static ModRegistration Register(ModEntry mod)
    {
        Logger.Log($"Register mod {mod.Info.Id}");
        return new ModRegistration(mod);
    }

    public static void Commit(ModEntry mod, List<ElementDefinition> defs)
    {
        Logger.Log($"Commit");

        if (!Active.TryGetValue(mod, out var list))
        {
            list = new List<RuntimeElement>();
            Active[mod] = list;
        }

        foreach (var def in defs)
        {
            Logger.Log($"Add def '{def.Label}'");
            var runtime = RuntimeFactory.Create(mod, def);
            list.Add(runtime);
        }
    }

    public static void Unregister(ModEntry mod)
    {
        Logger.Log($"Unregister mod '{mod.Info.Id}'");

        if (!Active.TryGetValue(mod, out var list))
            return;

        foreach (var element in list)
        {
            Logger.Log($"Destroy element={element}");

            RuntimeFactory.Destroy(element);

            Main.toolbar!.RemoveElement(element);
        }

        Logger.Log($"Remove mod '{mod}'");
        Active.Remove(mod);

        Logger.Log($"Unregistered mod '{mod.Info.Id}' successfully");
    }
}

public sealed class ModRegistration
{
    private static ModEntry.ModLogger Logger => Main.ModEntry.Logger;
    private readonly ModEntry _mod;
    private readonly List<ElementDefinition> _definitions = new();

    public ModRegistration(ModEntry mod)
    {
        _mod = mod;
    }

    private ModRegistration AddControlInternal(string label, Texture2D? icon, string tooltip, Action onClick)
    {
        Logger.Log($"ModRegistration.AddControl label={label} icon={icon} tooltip={tooltip} onClick={onClick}");

        _definitions.Add(new ControlDefinition
        {
            Label = label,
            Icon = icon,
            Tooltip = tooltip,
            OnClick = onClick
        });

        return this;
    }

    public ModRegistration AddControl(string label, string icon, string tooltip, Action onClick)
    {
        return AddControlInternal(label, icon: icon != null ? LoadIconByPath(icon) : null, tooltip, onClick);
    }

    private ModRegistration AddPanelControlInternal(
        string label,
        Texture2D? icon,
        string tooltip,
        Action<Rect>? onGUIContent,
        Type? windowType,
        string title,
        int? width,
        int? height)
    {
        Logger.Log($"ModRegistration.AddPanelControl label={label} icon={icon} tooltip={tooltip} onGUIContent={onGUIContent} windowType={windowType} title={title} width={width} height={height}");

        _definitions.Add(new PanelDefinition
        {
            Label = label,
            Icon = icon,
            Tooltip = tooltip,
            OnGUIContent = onGUIContent,
            WindowType = windowType,
            WindowTitle = title,
            Width = width,
            Height = height
        });

        return this;
    }

    // string + GUI

    public ModRegistration AddPanelControl(
        string label,
        string? icon,
        string tooltip,
        Action<Rect> onGUIContent,
        string title,
        int? width = null,
        int? height = null)
    {
        return AddPanelControlInternal(
            label, icon: icon != null ? LoadIconByPath(icon) : null, tooltip,
            onGUIContent,
            windowType: null,
            title,
            width, height);
    }

    // string + type

    public ModRegistration AddPanelControl(
        string label,
        string? icon,
        string tooltip,
        Type type,
        string title,
        int? width = null,
        int? height = null)
    {
        return AddPanelControlInternal(
            label, icon: icon != null ? LoadIconByPath(icon) : null, tooltip,
            onGUIContent: null,
            windowType: type,
            title,
            width, height);
    }

    public void Finish()
    {
        Logger.Log($"ModRegistration.Finish");

        ModToolbarAPI.Commit(_mod, _definitions);
    }

    // TODO: probably move
    private Texture2D LoadIconByPath(string iconPath)
    {
        var absoluteFilePath = Path.Combine(_mod.Path, iconPath);

        if (!File.Exists(absoluteFilePath))
            throw new Exception($"Icon does not exist: {absoluteFilePath} (provided {iconPath})");

        byte[] data = File.ReadAllBytes(absoluteFilePath);

        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(data);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        return tex;
    }
}

public abstract class ElementDefinition
{
    private string _label = string.Empty;
    private string _actualLabel = string.Empty;

    public string Label
    {
        get => _label;
        set
        {
            _actualLabel = value;
            _label = TransformLabel(value);
        }
    }

    public Texture2D? Icon;
    public string Tooltip;

    private static string TransformLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return string.Empty;

        var parts = label.Split(' ');

        if (parts.Length > 1)
        {
            return string.Concat(
                parts.Take(2).Select(p => char.ToUpperInvariant(p[0]))
            );
        }

        var word = parts[0];
        return word.Length >= 2
            ? word.Substring(0, Math.Min(2, word.Length)).ToUpperInvariant()
            : word.ToUpperInvariant();
    }

    public override string ToString()
    {
        return $"ActualLabel={_actualLabel},Label={Label},Icon={Icon},Tooltip={Tooltip}";
    }
}

public sealed class ControlDefinition : ElementDefinition
{
    public Action OnClick;
    public override string ToString()
    {
        return $"ControlDefinition(OnClick={OnClick},{base.ToString()})";
    }
}

public sealed class PanelDefinition : ElementDefinition
{
    public Action<Rect>? OnGUIContent;
    public Type? WindowType;
    public string WindowTitle;
    public int? Width;
    public int? Height;
    public override string ToString()
    {
        return $"PanelDefinition(WindowType={WindowType},Title={WindowTitle},Width={Width},Height={Height},{base.ToString()})";
    }
}

public sealed class RuntimeElement
{
    public ElementDefinition Definition;
    public Action OnClick;
    public GameObject WindowGO;
    public ModToolbarWindow? ModToolbarWindow;
    public override string ToString()
    {
        return $"RuntimeElement(Def={Definition},OnClick={OnClick},Window={WindowGO},ModToolbarWindow={ModToolbarWindow})";
    }
}

public static class RuntimeFactory
{
    private static ModEntry.ModLogger Logger => Main.ModEntry.Logger;

    public static RuntimeElement Create(ModEntry mod, ElementDefinition def)
    {
        Logger.Log($"RuntimeFactory.Create mod={mod.Info.Id} def={def}");

        if (def is ControlDefinition c)
            return CreateControl(c);

        if (def is PanelDefinition p)
            return CreatePanel(p);

        throw new ArgumentOutOfRangeException();
    }

    private static RuntimeElement CreateControl(ControlDefinition def)
    {
        Logger.Log($"RuntimeFactory.CreateControl def={def}");

        var runtime = new RuntimeElement
        {
            Definition = def,
            OnClick = def.OnClick
        };

        Main.toolbar!.AddElement(runtime);
        return runtime;
    }

    private static RuntimeElement CreatePanel(PanelDefinition def)
    {
        Logger.Log($"RuntimeFactory.CreatePanel def={def}");

        var go = new GameObject($"DerailValleyModToolbar_Panel_{def.Label}");
        var win = go.AddComponent<ModToolbarWindow>();

        Logger.Log($"Created gameobject={go}");

        // TODO: default to hidden / make on demand to save resources

        if (def.Width != null)
            win.Width = (int)def.Width;
        if (def.Height != null)
            win.Height = (int)def.Height;

        win.Title = def.WindowTitle;

        if (def.OnGUIContent != null)
        {
            win.DrawContent = def.OnGUIContent;
        }
        else if (def.WindowType != null)
        {
            Logger.Log($"Adding component={def.WindowType}");

            IModToolbarPanel theirWindow = (IModToolbarPanel)go.AddComponent(def.WindowType);

            win.DrawContent = (rect) => theirWindow.Window(rect);
        }
        else
        {
            Logger.Log("Need OnGUIContent or WindowType");
        }

        Logger.Log($"Creating runtime...");

        var runtime = new RuntimeElement
        {
            Definition = def,
            WindowGO = go,
            ModToolbarWindow = win
        };

        Main.toolbar!.AddElement(runtime);

        Logger.Log($"RuntimeFactory.CreatePanel done go={go}");

        return runtime;
    }

    public static void Destroy(RuntimeElement runtime)
    {
        Logger.Log($"RuntimeFactory.Destroy runtime={runtime} window={runtime.WindowGO}");

        if (runtime.WindowGO != null)
            UnityEngine.Object.Destroy(runtime.WindowGO);
    }
}
