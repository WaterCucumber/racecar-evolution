namespace Simulation.Scenes;


using Godot;

public partial class MapSegment : Node2D
{
    private Vector2 size;
    private float exitRotationDegree;

    [Export] public float ExitRotationDegree { get => exitRotationDegree * Scale.Y + RotationDegrees; private set => exitRotationDegree = value; }
    
    [Export] public Vector2 Size { get => size; private set => size = value; }
    [Export] public StaticBody2D StartBox { get; private set; }
    [Export] public Marker2D StartPoint { get; private set; }
    [Export] public Marker2D EndPoint { get; private set; }
    [Export] public Checkpoints Checkpoints { get; private set; }


    public Rect2 GetGlobalRect()
    {
        Vector2 topLeft = Vector2.Zero;
        Vector2 topRight = new(size.X, 0);
        Vector2 bottomLeft = new(0, size.Y);
        Vector2 bottomRight = size;

        Transform2D globalTransform = GlobalTransform;

        Vector2 gTopLeft = globalTransform * topLeft;
        Vector2 gTopRight = globalTransform * topRight;
        Vector2 gBottomLeft = globalTransform * bottomLeft;
        Vector2 gBottomRight = globalTransform * bottomRight;

        Vector2 min = new(
            Mathf.Min(Mathf.Min(gTopLeft.X, gTopRight.X), Mathf.Min(gBottomLeft.X, gBottomRight.X)),
            Mathf.Min(Mathf.Min(gTopLeft.Y, gTopRight.Y), Mathf.Min(gBottomLeft.Y, gBottomRight.Y))
        );

        Vector2 max = new(
            Mathf.Max(Mathf.Max(gTopLeft.X, gTopRight.X), Mathf.Max(gBottomLeft.X, gBottomRight.X)),
            Mathf.Max(Mathf.Max(gTopLeft.Y, gTopRight.Y), Mathf.Max(gBottomLeft.Y, gBottomRight.Y))
        );

        return new Rect2(min, max - min);
    }
}
