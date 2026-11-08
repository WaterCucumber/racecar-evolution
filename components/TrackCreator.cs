using System.Threading.Tasks;
using Godot;
using Simulation.Util;

namespace Simulation.Scenes;


public partial class TrackCreator : Node
{
    [Export] private CollisionPolygon2D _outWall;
    [Export] private CollisionPolygon2D _inWall;
    [Export] private Checkpoints _checkpoints;

    private Curve2D _curve;

    private void Clear()
    {
        _outWall.Polygon = [];
        _inWall.Polygon = [];
        _checkpoints.Clear();
    }
    public async Task<Checkpoints> CreateTrackAsync(int numPoints, float minSize, float maxSize)
    {
        Clear();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        
        _curve = TrackGenerator.GenerateTrack(new(1152 / 2, 648 / 2), numPoints, minSize, maxSize);
        var data = TrackGenerator.CreatePhysicsTrack(_curve);
        _outWall.Polygon = [.. data.OuterWallPoints];
        _inWall.Polygon = [.. data.InnerWallPoints];

        for (var i = 0; i < data.Checkpoints.Count; i++)
        {
            (Vector2 inPoint, Vector2 outPoint) = data.Checkpoints[i];
            Vector2 dir = inPoint.DirectionTo(outPoint);
            float distance = inPoint.DistanceTo(outPoint);
            Vector2 middle = inPoint + distance * 0.5f * dir;

            Area2D ch = new();
            _checkpoints.AddChild(ch);
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
        _checkpoints.Update();
        return _checkpoints;
    }

}
