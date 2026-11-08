using Godot;
using Simulation.Util;

namespace Simulation.Scenes;


public partial class AutoCamera : Camera2D
{
    public override void _EnterTree()
    {
        TrackGenerator.TrackGenerated += OnMapGenerated;
    }

    private void OnMapGenerated(Rect2 rect)
    {
        Zoom = Vector2.One;
        GlobalPosition = rect.GetCenter();
        var size = GetViewportRect().Size;
        var ratio = size / rect.Size;
        Zoom *= Mathf.Min(ratio.X, ratio.Y);
    }

}
