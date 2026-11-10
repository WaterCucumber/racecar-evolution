using System;
using System.Linq;
using Godot;
using Simulation.Data;
using Simulation.Managers;
using Simulation.Util;

namespace Simulation.Scenes;


public partial class RaceManager : Node2D
{
    public event Action EpochStarted;
    
    [Export] private string[] _loadFiles;

    private AgentsPool _pool;
    private TrainingDifficultyManager _difficultyManager;

    
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

        _pool = new(_loadFiles.Length, this);
        _difficultyManager = new(this, _pool);
        _difficultyManager.TrackReady += OnTrackReady;

        var c = await _difficultyManager.CreateNewCheckpoints();

        LogUI.Toggle(LogUI.GetInstance().Generation, false);
        LogUI.Toggle(LogUI.GetInstance().MutationRate, false);
        LogUI.Send(LogUI.GetInstance().BestFitness, $"Best Fitness: N/A");

        StartNewEpoch(GetGenomes(), c);
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
        InitializeAgents(gens, c);
        EpochStarted?.Invoke();
    }

    private Genome[] GetGenomes()
    {
        Genome[] gens = new Genome[_loadFiles.Length];
        for (var i = 0; i < _loadFiles.Length; i++)
        {
            gens[i] = new();
            SaveManager.LoadBrain(gens[i].Brain, _loadFiles[i]);
        }
        return gens;
    }

    private void OnTrackReady(Checkpoints c)
    {
        Agent[] population = [.. _pool.GetAgents().OrderByDescending(a => a.Scoring.GetFitness())];

        GD.Print($"Difficulty: {_difficultyManager.TrackSetuper.GetDifficulty()}");

        for (var i = 0; i < population.Length; i++)
        {
            GD.Print($"\t#{i+1} {population[i].GetGenome().Brain.Id} ({population[i].Scoring.GetFitness()} pt.)");
        }

        LogUI.Send(LogUI.GetInstance().BestFitness, $"Best Fitness: {population[0].Scoring.GetFitness(),4:F2}");

        StartNewEpoch(GetGenomes(), c);
    }
}