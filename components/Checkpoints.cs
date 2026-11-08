using Godot;

namespace Simulation.Scenes;


public partial class Checkpoints : Node2D
{
    private const float StandartDistance = 250;
    private Area2D[] _checkpoints;

    public int GetCheckpoints() => _checkpoints.Length;
    public Vector2 GetSpawnPoint() => _checkpoints[0].GlobalPosition;
    public float GetPctDistance(int nextIdx, Vector2 currentPos)
    {
        if(nextIdx >= _checkpoints.Length) return 0;

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
    public int GetCheckpointIndex(Area2D checkpoint) => System.Array.IndexOf(_checkpoints, checkpoint);
    public bool IsFinalCheckpoint(int idx) => idx == _checkpoints.Length - 1;

    public void Update()
    {
        Godot.Collections.Array<Node> list = GetChildren();
        _checkpoints = new Area2D[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            Node child = list[i];
            _checkpoints[i] = (Area2D)child;
        }
    }
    public void Clear()
    {
        for (var i = 0; i < GetChildCount(); i++)
        {
            GetChild(i).QueueFree();
        }
    }
}
