using System;
using System.Collections.Generic;
using Godot;

namespace Simulation.Scenes;


public partial class Map : Node2D
{
    private readonly List<MapSegment> _createdSegments = [];

    public Vector2 GetSpawnPoint() => _createdSegments[0].StartPoint.GlobalPosition;
    public List<Checkpoints> GetCheckpoints()
    {
        List<Checkpoints> checkpoints = [];
        foreach (var s in _createdSegments)
        {
            checkpoints.Add(s.Checkpoints);
        }
        return checkpoints;
    }
    public Rect2 GetMapSize()
    {
        Vector2 pos = Vector2.Zero;
        Vector2 end = Vector2.Zero;
        foreach (var segment in _createdSegments)
        {
            var rect = segment.GetGlobalRect();
            Vector2 from = rect.Position;
            Vector2 to = rect.End;

            if(from.X < pos.X) pos.X = from.X;
            if(from.Y < pos.Y) pos.Y = from.Y;
            if(to.X < pos.X) pos.X = to.X;
            if(to.Y < pos.Y) pos.Y = to.Y;

            if(from.X > end.X) end.X = from.X;
            if(from.Y > end.Y) end.Y = from.Y;
            if(to.X > end.X) end.X = to.X;
            if(to.Y > end.Y) end.Y = to.Y;
        }
        Vector2 size = (end - pos).Abs();
        return new(pos, size);
    }
    public MapSegment GetLastSegment()
    {
        if(_createdSegments.Count == 0) return null;
        return _createdSegments[^1];
    }


    public bool CanPlaceSegment(Rect2 segment)
    {
        foreach (var s in _createdSegments)
        {
            if(s.GetGlobalRect().Intersects(segment)) return false;
        }
        return true;
    }

    public void AddSegment(MapSegment s) => _createdSegments.Add(s);
    public void RemoveLastSegment()
    {
        if(_createdSegments.Count == 0) throw new Exception($"Can not find any segments that can fit here!");
        _createdSegments[^1].QueueFree();
        _createdSegments.RemoveAt(_createdSegments.Count - 1);
    }
    public void Clear()
    {
        _createdSegments.Clear();
        for (int i = 0; i < GetChildCount(); i++)
        {
            GetChild(i).QueueFree();
        }
    }

    public override void _Draw()
    {
        DrawRect(GetMapSize(), Colors.Yellow, false, 5);
        foreach (var s in _createdSegments)
        {
            var rect = s.GetGlobalRect();
            Vector2 from = rect.Position;
            Vector2 to = rect.End;
            DrawRect(rect, Colors.Green, false, 5);
            DrawCircle(from, 10, Colors.Red);
            DrawCircle(s.StartPoint.GlobalPosition, 8, Colors.Cyan);
            DrawCircle(s.EndPoint.GlobalPosition, 8, Colors.Violet);
            DrawCircle(to, 10, Colors.Blue);
        }
    }
}
