using System.Threading.Tasks;
using Godot;
using Simulation.Util;

namespace Simulation.Components;


public class TrackCreator(Node2D parent)
{
    private readonly CollisionPolygon2D _outWall = CreatePolygon(parent);
    private readonly CollisionPolygon2D _inWall = CreatePolygon(parent);
    private readonly SceneTree _tree = parent.GetTree();
    private Checkpoints _lastCheckpoints;

    public async Task<Checkpoints> CreateTrackAsync(int numPoints, float minSize, float maxSize)
    {
        Clear();
        await _tree.ToSignal(_tree, SceneTree.SignalName.ProcessFrame);

        Checkpoints checkpoints = new();
        var curve = TrackGenerator.GenerateTrack(new(1152 / 2, 648 / 2), numPoints, minSize, maxSize);
        var data = TrackGenerator.CreatePhysicsTrack(curve);
        _outWall.Polygon = [.. data.OuterWallPoints];
        _inWall.Polygon = [.. data.InnerWallPoints];

        for (var i = 0; i < data.Checkpoints.Count; i++)
        {
            (Vector2 inPoint, Vector2 outPoint) = data.Checkpoints[i];
            Vector2 dir = inPoint.DirectionTo(outPoint);
            float distance = inPoint.DistanceTo(outPoint);
            Vector2 middle = inPoint + distance * 0.5f * dir;

            Area2D ch = new();
            checkpoints.AddCheckpoint(ch);
            parent.AddChild(ch);
            ch.GlobalPosition = middle;

            CollisionShape2D shape = new();
            ch.AddChild(shape);
            SegmentShape2D s = new()
            {
                A = -distance * 0.5f * dir,
                B = distance * 0.5f * dir,  
            };
            shape.Shape = s;
            shape.DebugColor = i == 0 ? Colors.Yellow : i == data.Checkpoints.Count - 1 ? Colors.Red : Colors.Green;
        }
        return _lastCheckpoints = checkpoints;
    }


    private void Clear()
    {
        _outWall.Polygon = [];
        _inWall.Polygon = [];
        _lastCheckpoints?.Clear();
    }

    private static CollisionPolygon2D CreatePolygon(Node2D parent)
    {
        StaticBody2D wall = new();
        wall.AddToGroup("obstacle");
        parent.AddChild(wall);
        CollisionPolygon2D p = new()
        {
            BuildMode = CollisionPolygon2D.BuildModeEnum.Segments,  
        };
        wall.AddChild(p);
        return p;
    }
}
