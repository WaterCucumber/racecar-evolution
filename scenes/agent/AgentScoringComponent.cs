using System;
using Godot;
using Simulation.Scenes;
using Simulation.Util;

namespace Simulation.Components;


public class AgentScoringComponent
{
    // TODO: Replace Hard-coded variables
    private const float LapFitness = 250;
    public const float CheckpointFitness = 50;
    private const float NoCheckpointMaxTime = 5.0f;
    private const float InvalidCheckpointPenalty = 50;
    private const float NoCheckpointPenalty = 550;
    private const float BaseSlownessPenalty = 0.01f;
    private const float SlownessPenaltyGrow = 1.005f;
    private const float SlownessPenaltyMax = 5.0f;
    private const float WallCollidePenalty = 100;
    private const float AgentCollidePenalty = 1;

    public event Action Died;
    public event Action Finished;

    private readonly Entity _entity;
    private readonly Area2D _collisionCheckArea;
    private readonly Checkpoints _checkpoints;
    private bool _stopped;
    private float _noCheckpointTime;
    private float _lapTime;

    // Scoring
    private float _fitness;
    private float _slownessPenalty = BaseSlownessPenalty;
    private int _nextCheckpointIndex = 0;


    public float GetFitness() => _fitness;

    public AgentScoringComponent(Entity entity, Area2D collisionCheckArea)
    {
        _entity = entity;
        _collisionCheckArea = collisionCheckArea;
        _checkpoints = _entity.Checkpoints;

        _collisionCheckArea.AreaEntered += OnCheckpointReached;
        _collisionCheckArea.BodyEntered += OnBodyEntered;
    }

    public void Update(double delta)
    {
        if (_entity == null || _stopped) return;
        // Scoring
        float totalPenalty = _slownessPenalty;
        _fitness -= (float)delta * totalPenalty;
        _slownessPenalty += (float)delta * Mathf.Min(SlownessPenaltyMax, _slownessPenalty * SlownessPenaltyGrow);
        _noCheckpointTime += (float)delta;
        _lapTime += (float)delta;

        if (_noCheckpointTime >= NoCheckpointMaxTime)
        {
            _fitness -= NoCheckpointPenalty;
            Died?.Invoke();
            Stop();
        }
    }



    private void Stop()
    {
        _collisionCheckArea.AreaEntered -= OnCheckpointReached;
        _collisionCheckArea.BodyEntered -= OnBodyEntered;
        _stopped = true;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("obstacle"))
        {
            _fitness -= WallCollidePenalty * (1 - _checkpoints.GetPctDistance(_nextCheckpointIndex, _entity.GlobalPosition));
            Died?.Invoke();
            Stop();
        }
        else if (body is Entity a && a != _entity)
        {
            _fitness -= AgentCollidePenalty;
        }
    }

    private void OnCheckpointReached(Area2D a)
    {
        int idx = _checkpoints.GetCheckpointIndex(a);
        if (idx != -1)
        {
            if (_nextCheckpointIndex != idx)
            {
                _fitness -= InvalidCheckpointPenalty;
                return;
            }

            _fitness += CheckpointFitness;
            _slownessPenalty = BaseSlownessPenalty;
            _noCheckpointTime = 0;

            if (_checkpoints.IsFinalCheckpoint(idx))
            {
                _fitness += LapFitness / _lapTime;
                _lapTime = 0;
                _nextCheckpointIndex = 0;
                Finished?.Invoke();
                Stop();
            }
            else
            {
                _nextCheckpointIndex++;
            }
        }
    }
}
