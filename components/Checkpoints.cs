using System.Collections.Generic;
using Godot;

namespace Simulation.Util;


public class Checkpoints
{
    private const float StandartDistance = 250;
    private readonly List<Area2D> _checkpoints = [];

    public int GetCheckpointsNumber() => _checkpoints.Count;
    public Vector2 GetSpawnPoint() => _checkpoints[0].GlobalPosition;
    public float GetPctDistance(int nextIdx, Vector2 currentPos)
    {
        if(nextIdx >= GetCheckpointsNumber()) return 0;

        float distance = StandartDistance;
        Vector2 c1 = _checkpoints[nextIdx].GlobalPosition;
        
        if(nextIdx > 0)
        {
            Vector2 c0 = _checkpoints[nextIdx-1].GlobalPosition;
            distance = c0.DistanceTo(c1);
        }

        float currentDistance = currentPos.DistanceTo(c1);
        return Mathf.Max(1 - currentDistance / distance, 0);
    }
    public int GetCheckpointIndex(Area2D checkpoint) => _checkpoints.IndexOf(checkpoint);
    public bool IsFinalCheckpoint(int idx) => idx == GetCheckpointsNumber() - 1;

    public void AddCheckpoint(Area2D c) => _checkpoints.Add(c);
    public void Clear()
    {
        for (var i = 0; i < GetCheckpointsNumber(); i++)
        {
            _checkpoints[i].QueueFree();
        }
        _checkpoints.Clear();
    }
}
