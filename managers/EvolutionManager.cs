using System;
using System.Linq;
using Godot;
using Simulation.Data;
using Simulation.Managers;
using Simulation.Util;

namespace Simulation.Scenes;


public partial class EvolutionManager : Node2D
{
    public const int Population = 100;
    private const double DefaultMutationRate = 0.15;
    private const double PlatoMutationMultiply = 0.01;
    private const double PlatoMutationRate = 0.4;

    public event Action EpochStarted;
    
    [Export] private string _saveFile = "best_brain.json";
    [Export] private string _loadFile = "best_car_brain.json";

    private AgentsPool _pool;
    private TrainingDifficultyManager _difficultyManager;

    private int _generationIndex;
    private double _mutationRate = DefaultMutationRate;

    private bool _started = false;

    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Start();
    }

    public async void Start()
    {
        if(_started) return;
        _started = true;

        _pool = new(Population, this);
        _difficultyManager = new(this, _pool, 7);
        _difficultyManager.TrackReady += OnTrackReady;

        var gens = PopulationEvolutionist.GetStartGenomes(Population, _loadFile);
        var c = await _difficultyManager.CreateNewCheckpoints();

        LogUI.Send(LogUI.GetInstance().Generation, $"Generation: {_generationIndex,2}");
        LogUI.Send(LogUI.GetInstance().BestFitness, $"Best Fitness: N/A");
        LogUI.Send(LogUI.GetInstance().MutationRate, $"Mutation Rate: {_mutationRate:F2}");

        StartNewEpoch(gens, c);
    }


    private void InitializeAgents(Genome[] gens, Checkpoints c)
    {
        _pool.Respawn();
        Agent[] agents = [.. _pool.GetAgents()];
        
        for (var i = 0; i < agents.Length; i++)
        {
            var agent = agents[i];
            agent.GlobalPosition = c.GetSpawnPoint();

            agent.Initialize(gens[i] ?? new(), c);
        }
    }

    private void StartNewEpoch(Genome[] gens, Checkpoints c)
    {
        _generationIndex++;
        InitializeAgents(gens, c);
        EpochStarted?.Invoke();
    }

    private void OnTrackReady(Checkpoints c)
    {
        Agent[] population = [.. _pool.GetAgents().OrderByDescending(a => a.Scoring.GetFitness())];
        if (_saveFile != "") SaveManager.SaveBrain(population[0].GetGenome().Brain, _saveFile);
        GD.Print($"Generation {_generationIndex,2} Best Fitness: {population[0].Scoring.GetFitness()}");

        if (_difficultyManager.GetPlato() == 0)
        {
            _mutationRate = DefaultMutationRate;
        }
        else
        {
            _mutationRate += _difficultyManager.GetPlato() * PlatoMutationMultiply;
            _mutationRate = Mathf.Min(PlatoMutationRate, _mutationRate);
        }

        LogUI.Send(LogUI.GetInstance().Generation, $"Generation: {_generationIndex,2}");
        LogUI.Send(LogUI.GetInstance().BestFitness, $"Best Fitness: {population[0].Scoring.GetFitness(),4:F2}");
        LogUI.Send(LogUI.GetInstance().MutationRate, $"Mutation Rate: {_mutationRate:F2}");

        var gens = PopulationEvolutionist.EvolvePopulation([.. _pool.GetAgents()], _mutationRate);
        StartNewEpoch(gens, c);
    }
}
