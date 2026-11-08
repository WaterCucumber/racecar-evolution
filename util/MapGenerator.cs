using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace Simulation.Scenes;


public partial class MapGenerator : Node2D
{
    public event Action<Map> MapGenerated;
    private readonly static Random _random = new();

    [Export] private PackedScene[] _segements;


    public async Task<Map> GenerateMap(int length = 3)
    {
        Map map = new();
        AddChild(map);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        for (var i = 0; i < length; i++)
        {
            List<PackedScene> segmentsLeft = [.. _segements];
            for (var attempt = 0; attempt < 20; attempt++)
            {
                if(segmentsLeft.Count == 0)
                {
                    map.RemoveLastSegment();
                    i -= 2;
                    break;
                }
                int idx = _random.Next(segmentsLeft.Count);
                var segment = segmentsLeft[idx];
                if(TryInsertSegment(segment, map) is MapSegment s)
                {
                    if(i != 0) s.StartBox.QueueFree();
                    break;
                }
                segmentsLeft.RemoveAt(idx);
            }
        }
        GD.Print($"Generated map {length} length (or less)");
        MapGenerated?.Invoke(map);
        return map;
    }
    

    private static MapSegment TryInsertSegment(PackedScene segmentScene, Map map)
    {
        List<Vector2> scales = [Vector2.One, Vector2.Right + Vector2.Up, Vector2.Left + Vector2.Down, Vector2.Left + Vector2.Up];
        for (var i = 0; i < 4; i++)
        {
            var idx = _random.Next(scales.Count);
            if(PushSegment(segmentScene, scales[idx], map) is MapSegment s) return s;
            scales.RemoveAt(idx);
        }
        return null;
    }

    private static MapSegment PushSegment(PackedScene segmentScene, Vector2 scale, Map map)
    {
        (Vector2 spawnPoint, float currentRotation) = GetLastSegementValues(map);
        var segment = segmentScene.Instantiate<MapSegment>();
        map.AddChild(segment);
        segment.Rotation = currentRotation * scale.X;
        segment.Scale = scale;
        segment.GlobalPosition = CalculateSegmentPosition(segment, spawnPoint);
        
        if (!map.CanPlaceSegment(segment.GetGlobalRect()))
        {
            segment.QueueFree();
            return null;
        }
        map.AddSegment(segment);
        return segment;
    }

    private static Vector2 CalculateSegmentPosition(MapSegment segment, Vector2 spawnPoint)
    {
        var offset = segment.GlobalPosition - segment.StartPoint.GlobalPosition;
        return offset + spawnPoint;
    }

    private static (Vector2 spawnPoint, float rotation) GetLastSegementValues(Map map)
    {
        Vector2 spawnPoint = Vector2.Zero;
        float currentRotation = 0f;

        if(map.GetLastSegment() is MapSegment lastSegment)
        {
            spawnPoint = lastSegment.EndPoint.GlobalPosition;
            currentRotation = Mathf.DegToRad(lastSegment.ExitRotationDegree);
        }
        return (spawnPoint, currentRotation);
    }
}
