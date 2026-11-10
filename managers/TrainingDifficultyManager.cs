using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Simulation.Components;
using Simulation.Scenes;
using Simulation.Util;

namespace Simulation.Managers;

/// <summary>
/// Use for auto-track setup after all agents stopped
/// </summary>
public class TrainingDifficultyManager
{
    public event Action<Checkpoints> TrackReady;

    public TrainingTrackSetuper TrackSetuper { get; }
    private readonly AgentsPool _pool;
    private float _previousFitness;
    private int _platoAmount;
    private float _previousCheckpointsFitness;


    public TrainingDifficultyManager(Node2D parent, AgentsPool pool)
    {
        _pool = pool;
        _pool.AllAgentsStopped += OnAllAgentsStopped;
        TrackSetuper = new(parent);

        LogUI.Send(LogUI.GetInstance().Plato, $"Plato: 0");
    }
    public TrainingDifficultyManager(Node2D parent, AgentsPool pool, float difficulty) : this(parent, pool)
    {
        TrackSetuper.SetDifficulty(difficulty);
    }


    public int GetPlato() => _platoAmount;

    public async Task<Checkpoints> CreateNewCheckpoints()
    {
        var c = await TrackSetuper.CreateTrack();
        _previousCheckpointsFitness = c.GetCheckpointsNumber() * AgentScoringComponent.CheckpointFitness;
        return c;
    }

    private void FinishEpoch(float checkpointsFitness)
    {
        Agent[] population = [.. _pool.GetAgents()];
        population = [.. population.OrderByDescending(a => a.Scoring.GetFitness())];

        float bestFitness = population[0].Scoring.GetFitness();

        if (bestFitness - checkpointsFitness < 1.01f)
        {
            _platoAmount++;

            if (_platoAmount > 5 && bestFitness < _previousFitness)
            {
                TrackSetuper.ChangeDifficulty(false);
            }
        }
        else
        {
            _platoAmount = 0;
            TrackSetuper.ChangeDifficulty(true);
        }
        _previousFitness = bestFitness;

        LogUI.Send(LogUI.GetInstance().Plato, $"Plato: {_platoAmount,2}");
    }

    private async void OnAllAgentsStopped()
    {
        FinishEpoch(_previousCheckpointsFitness);
        var c = await TrackSetuper.CreateTrack();
        TrackReady?.Invoke(c);
        _previousCheckpointsFitness = c.GetCheckpointsNumber() * AgentScoringComponent.CheckpointFitness;
    }
}
