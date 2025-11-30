using UnityModManagerNet;

namespace DerailValleyModToolbar;

public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Draw(Type = DrawType.Slider, Min = 0.5f, Max = 5f)] public float Scale = 1f;
    [Draw(Type = DrawType.Slider, Min = -200f, Max = 200f)] public float OffsetX = 0f;
    [Draw(Type = DrawType.Slider, Min = -200f, Max = 200f)] public float OffsetY = 0f;
    [Draw(Type = DrawType.Slider, Min = 100, Max = 2000)] public int DefaultWidth = 700;
    [Draw(Type = DrawType.Slider, Min = 100, Max = 2000)] public int DefaultHeight = 400;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public void OnChange()
    {
    }
}
